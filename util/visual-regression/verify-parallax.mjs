// Real-component regressions for Parallax's selection, focus, and composite geometry.
// node util/visual-regression/verify-parallax.mjs http://localhost:5206
import assert from "node:assert/strict";
import { mkdir, readFile, writeFile } from "node:fs/promises";
import { launchBrowser, sleep } from "./browser.mjs";

const base = process.argv[2] ?? "http://localhost:5206";
const fontCss = process.env.PARALLAX_FONT_CSS
  ? await readFile(process.env.PARALLAX_FONT_CSS, "utf8")
  : null;
const output = new URL("./snapshots/parallax/", import.meta.url);
await mkdir(output, { recursive: true });
const browser = await launchBrowser(process.env.CHROME_PATH ?? "chromium", {
  hideScrollbars: false,
});
const { send, waitForEvent } = browser;
async function evaluate(expression) {
  let timer;
  try {
    return await Promise.race([
      browser.evaluate(expression),
      new Promise((_, reject) => {
        timer = setTimeout(
          () => reject(new Error(`Browser check timed out: ${expression}`)),
          15000,
        );
      }),
    ]);
  } finally {
    clearTimeout(timer);
  }
}

async function sample(name, mode) {
  const loaded = waitForEvent("Page.loadEventFired", 15000);
  await send("Page.navigate", {
    url: `${base}/DocsStatic?name=${name}&layout=_CleanLayout&theme=parallax&mode=${mode}`,
  });
  assert.ok(await loaded, `${name}: loaded`);
  assert.ok(
    await evaluate(`!!document.querySelector('link[href*="stellar-admin.parallax"]')`),
    "Parallax selected",
  );
  await prepareFonts(name);
  assert.ok(
    await evaluate(
      "getComputedStyle(document.documentElement).getPropertyValue('--sa-parallax-accent').trim()",
    ),
    "Parallax stylesheet loaded with its foundation tokens",
  );
}
async function prepareFonts(name) {
  if (fontCss) {
    await evaluate(`(() => {
      const style = document.createElement('style');
      style.textContent = ${JSON.stringify(fontCss)};
      document.head.append(style);
      return Promise.all([
        document.fonts.load('400 13px "Space Grotesk"'),
        document.fonts.load('500 13px "Space Grotesk"'),
        document.fonts.load('600 13px "Space Grotesk"'),
        document.fonts.load('400 13px "JetBrains Mono"'),
        document.fonts.load('500 13px "JetBrains Mono"')
      ]);
    })()`);
  }
  const fontsReady = await evaluate(
    "Promise.race([document.fonts.ready.then(() => true), new Promise(resolve => setTimeout(() => resolve(false), 8000))])",
  );
  if (!fontsReady) console.warn(`${name}: font loading timed out; continuing behavioral checks`);
}
async function capture(name, mode, width) {
  const shot = await send("Page.captureScreenshot", { format: "png" });
  await writeFile(
    new URL(`${name}-${mode}-${width}.png`, output),
    Buffer.from(shot.data, "base64"),
  );
}
async function force(selector, states) {
  const { root } = await send("DOM.getDocument");
  const { nodeId } = await send("DOM.querySelector", { nodeId: root.nodeId, selector });
  assert.ok(nodeId, selector);
  await send("CSS.forcePseudoState", { nodeId, forcedPseudoClasses: states });
  await sleep(180);
}
async function key(key, code, virtualKey) {
  for (const type of ["keyDown", "keyUp"]) {
    await send("Input.dispatchKeyEvent", { type, key, code, windowsVirtualKeyCode: virtualKey });
  }
}
try {
  await send("Page.enable");
  if (fontCss) {
    await send("Network.enable");
    await send("Network.setBlockedURLs", {
      urls: ["*fonts.googleapis.com*", "*fonts.gstatic.com*"],
    });
  }
  await send("DOM.enable");
  await send("CSS.enable");
  await send("Emulation.setFocusEmulationEnabled", { enabled: true });
  for (const mode of ["light", "dark"]) {
    for (const width of [1280, 390]) {
      await send("Emulation.setDeviceMetricsOverride", {
        width,
        height: 900,
        deviceScaleFactor: 1,
        mobile: false,
      });
      await sample("Empty/_WithBorder", mode);
      assert.equal(
        await evaluate(`(() => {
          const empty = document.querySelector('.sa-empty');
          const probe = document.createElement('div');
          probe.style.borderColor = 'var(--border)';
          document.body.append(probe);
          const style = getComputedStyle(empty);
          const themed = style.borderTopWidth === '1px' &&
            style.borderTopColor === getComputedStyle(probe).borderTopColor;
          probe.remove();
          empty.classList.remove('border');
          const borderless = getComputedStyle(empty).borderTopWidth === '0px';
          empty.classList.add('border');
          return themed && borderless;
        })()`),
        true,
        "optional empty-state border uses the standard surface border color",
      );
      await sample("Card/_Intro", mode);
      assert.equal(
        await evaluate(
          `getComputedStyle(document.querySelector('.sa-card-title')).fontFamily.includes('Space Grotesk')`,
        ),
        true,
        "card headings use the display family",
      );
      assert.equal(
        await evaluate(
          `getComputedStyle(document.querySelector('.sa-card-description')).fontFamily.includes('Space Grotesk')`,
        ),
        true,
        "card body copy uses the UI family",
      );
      await sample("Input/_Intro", mode);
      assert.equal(
        await evaluate(`(() => {
          const input = document.querySelector('.sa-input');
          const probe = document.createElement('div');
          probe.style.backgroundColor = 'var(--card)';
          document.body.append(probe);
          const different = getComputedStyle(input).backgroundColor !== getComputedStyle(probe).backgroundColor;
          probe.remove();
          return different;
        })()`),
        mode === "dark",
        "dark editable fields recess below cards; light fields share the card surface",
      );
      await sample("Checkbox/_Intro", mode);
      assert.equal(
        await evaluate(`(() => {
          const input = document.querySelector('.sa-checkbox');
          input.checked = true;
          input.disabled = true;
          const indicator = input.parentElement.querySelector('.sa-checkbox-indicator');
          return getComputedStyle(indicator).visibility === 'visible';
        })()`),
        true,
        "disabled checked controls retain their selection indicator",
      );
      await sample("PageHeader/_Nav", mode);
      assert.equal(
        await evaluate(`(() => {
          const nav = document.querySelector('.sa-page-header-nav');
          return nav.scrollHeight <= nav.clientHeight && nav.scrollWidth <= nav.clientWidth;
        })()`),
        true,
        "header tabs and underline fit without scrollbars",
      );
      assert.equal(
        await evaluate(`(() => {
          const nav = document.querySelector('.sa-page-header-nav');
          nav.querySelector('.sa-tabs-trigger:last-child').textContent = 'Long navigation label '.repeat(20);
          nav.scrollLeft = nav.scrollWidth;
          return nav.scrollLeft > 0 && nav.scrollHeight <= nav.clientHeight;
        })()`),
        true,
        "long header navigation remains horizontally scrollable without vertical overflow",
      );

      await sample("Button/_SizesVariants", mode);
      const primary = ".sa-button-size-default.sa-button-variant-default";
      await force(primary, ["focus-visible", "active"]);
      const action = await evaluate(`(() => {
        const el = document.querySelector('${primary}');
        const s = getComputedStyle(el);
        return { shadow: s.boxShadow, transform: s.transform, focused: el.matches(':focus-visible') };
      })()`);
      assert.notEqual(action.shadow, "none", "pressed action keeps keyboard focus");
      assert.equal(action.transform, "none", "pressed action does not move");
      assert.equal(action.focused, true);

      await sample("InputGroup/_Text", mode);
      assert.equal(
        await evaluate(`(() => {
          const addon = document.querySelector('.sa-input-group-addon[data-align="inline-start"]');
          const style = getComputedStyle(addon);
          return style.borderTopRightRadius === '0px' && parseFloat(style.borderTopLeftRadius) > 0;
        })()`),
        true,
        "text addons keep a square interior seam and rounded outside corners",
      );
      await evaluate(`(() => {
        const input = document.querySelector('.sa-input-group-input');
        input.readOnly = true;
        input.setAttribute('aria-invalid', 'true');
        input.focus();
      })()`);
      const field = await evaluate(`(() => {
        const el = document.activeElement;
        const group = el.closest('.sa-input-group');
        const inner = getComputedStyle(el), outer = getComputedStyle(group);
        return { innerBackground: inner.backgroundColor, innerShadow: inner.boxShadow,
          innerBorder: inner.borderWidth, outerShadow: outer.boxShadow, outerBorder: outer.borderColor,
          destructive: (() => {const probe=document.createElement('span');probe.style.color='var(--destructive)';document.body.append(probe);const color=getComputedStyle(probe).color;probe.remove();return color;})() };
      })()`);
      assert.equal(
        field.innerBackground,
        "rgba(0, 0, 0, 0)",
        "read-only grouped field stays transparent",
      );
      assert.equal(field.innerShadow, "none", "group owns the focus decoration");
      assert.equal(field.innerBorder, "0px", "one composite border");
      assert.notEqual(field.outerShadow, "none", "invalid focused group has a visible indication");
      assert.deepEqual(
        field.outerBorder.match(/[\d.]+/g).map(Number),
        field.destructive.match(/[\d.]+/g).map(Number),
      );

      const background = (selector) =>
        evaluate(
          `getComputedStyle(document.querySelector(${JSON.stringify(selector)})).backgroundColor`,
        );
      for (const [name, item] of [
        ["CheckboxItems", ".sa-dropdown-menu-checkbox-item"],
        ["RadioGroup", ".sa-dropdown-menu-radio-item"],
      ]) {
        await sample(`DropdownMenu/_${name}`, mode);
        await evaluate("document.querySelector('[data-slot=dropdown-menu-trigger]').click()");
        await sleep(180);
        await evaluate("document.activeElement.blur()");
        const checked = `${item}[aria-checked="true"]`;
        const unchecked = `${item}[aria-checked="false"]`;
        assert.equal(
          await evaluate(`(() => {
            const row = document.querySelector('${checked}');
            const indicator = row.querySelector('.sa-dropdown-menu-item-indicator');
            const bounds = row.getBoundingClientRect();
            const mark = indicator.getBoundingClientRect();
            return mark.width > 0 && mark.left > bounds.left + bounds.width / 2 &&
              getComputedStyle(indicator.querySelector('svg')).display !== 'none' &&
              getComputedStyle(document.querySelector('${unchecked} .sa-dropdown-menu-item-indicator')).display === 'none';
          })()`),
          true,
          "menu checkmarks appear on the right only for checked items",
        );
        const rest = await background(unchecked);
        assert.equal(await background(checked), rest, "checked menu items have no persistent fill");
        await force(checked, ["hover"]);
        const hover = await background(checked);
        assert.notEqual(hover, rest, "checked menu items retain hover feedback");
        await force(checked, []);
        await force(unchecked, ["hover"]);
        assert.equal(await background(unchecked), hover, "checked and unchecked hover match");
        await force(unchecked, []);
        await force(checked, ["focus", "focus-visible"]);
        assert.notEqual(
          await evaluate(`getComputedStyle(document.querySelector('${checked}')).boxShadow`),
          "none",
          "checked menu items retain visible keyboard focus",
        );
        await evaluate(
          "document.querySelector('.sa-dropdown-menu-content').classList.add('sa-menu-accent-bold')",
        );
        await sleep(180);
        assert.notEqual(
          await evaluate(
            `getComputedStyle(document.querySelector('${checked} .sa-dropdown-menu-item-indicator')).color`,
          ),
          await background(checked),
          "bold menu selection mark contrasts with its focused row",
        );
        await evaluate(
          "document.querySelector('.sa-dropdown-menu-content').classList.remove('sa-menu-accent-bold')",
        );
      }

      await capture("menu-focus", mode, width);
      for (const name of [
        "Dialog/_Intro",
        "Dialog/_StickyFooter",
        "AlertDialog/_SmallMedia",
        "AlertDialog/_Size",
      ]) {
        await sample(name, mode);
        await evaluate("document.querySelector('dialog').showModal()");
        await sleep(220);
        const layout = await evaluate(`(() => {
          const dialog = document.querySelector('dialog[open]');
          const r = dialog.getBoundingClientRect();
          const footer = dialog.querySelector('[data-slot$="footer"]');
          const f = footer.getBoundingClientRect();
          return {inside: r.left >= 0 && r.right <= innerWidth && r.top >= 0 && r.bottom <= innerHeight,
            footerVisible: f.top >= r.top && f.bottom <= r.bottom + 1,
            horizontalOverflow: dialog.scrollWidth > dialog.clientWidth};
        })()`);
        assert.ok(
          layout.inside && layout.footerVisible && !layout.horizontalOverflow,
          `${name}: dialog and footer contained`,
        );
        await capture(name.replaceAll("/", "-"), mode, width);
      }
      for (const name of [
        "InputGroup/_Buttons",
        "InputGroup/_Text",
        "InputGroup/_Textarea",
        "Card/_Intro",
        "Checkbox/_ChoiceCards",
        "Radio/_ChoiceCards",
        "Table/_RowSelection",
        "Progress/_Intro",
        "PageHeader/_Nav",
        "Sidebar/_FloatingVariant",
      ]) {
        await sample(name, mode);
        if (name === "Progress/_Intro") {
          assert.equal(
            await evaluate(
              `Array.from(document.querySelectorAll('.sa-progress-track')).every(t => t.scrollHeight <= t.clientHeight && t.querySelector('.sa-progress-indicator').getBoundingClientRect().height <= t.clientHeight)`,
            ),
            true,
            "progress fits its track",
          );
        }
        await capture(name.replaceAll("/", "-"), mode, width);
      }
      await sample("Toggle/_ModelBinding", mode);
      await evaluate("document.querySelector('.sa-toggle input[type=checkbox]').checked = true");
      const selectedToggle = await background(".sa-toggle");
      await force(".sa-toggle", ["hover"]);
      assert.equal(
        await background(".sa-toggle"),
        selectedToggle,
        "hover preserves model-bound toggle selection",
      );
      await evaluate("document.querySelector('.sa-toggle input[type=checkbox]').focus()");
      await key(" ", "Space", 32);
      assert.equal(
        await evaluate("document.querySelector('.sa-toggle input[type=checkbox]').checked"),
        false,
        "native toggle keyboard operation",
      );

      await sample("Slider/_Intro", mode);
      const before = await evaluate(
        "document.querySelector('[role=slider]').getAttribute('aria-valuenow')",
      );
      await evaluate("document.querySelector('[role=slider]').focus()");
      await key("ArrowRight", "ArrowRight", 39);
      assert.notEqual(
        await evaluate("document.querySelector('[role=slider]').getAttribute('aria-valuenow')"),
        before,
        "slider keyboard operation",
      );

      assert.equal(
        await evaluate(
          `Array.from(document.querySelectorAll('.sa-slider-track, .sa-slider-range, .sa-slider-thumb')).every(el => parseFloat(getComputedStyle(el).borderRadius) > 0)`,
        ),
        true,
        "slider track, range and thumb remain round",
      );
      assert.notEqual(
        await evaluate("getComputedStyle(document.querySelector('.sa-slider-thumb')).boxShadow"),
        "none",
        "slider retains focus after keyboard adjustment",
      );
      await capture("slider", mode, width);

      await sample("Switch/_Sizes", mode);
      for (const id of ["size-default", "size-small"]) {
        for (const checked of [false, true]) {
          await evaluate(`document.getElementById('${id}').checked = ${checked}`);
          await sleep(220);
          assert.equal(
            await evaluate(`(() => {
            const input = document.getElementById('${id}');
            const thumb = input.parentElement.querySelector('.sa-switch-thumb');
            const trackRect = input.getBoundingClientRect(), thumbRect = thumb.getBoundingClientRect();
            return thumbRect.left >= trackRect.left && thumbRect.right <= trackRect.right && thumbRect.top >= trackRect.top && thumbRect.bottom <= trackRect.bottom && parseFloat(getComputedStyle(input).borderRadius) > 0;
          })()`),
            true,
            "round switch thumb stays inside track at both sizes and states",
          );
        }
      }
      await evaluate("document.getElementById('size-default').focus()");
      await key(" ", "Space", 32);
      assert.equal(
        await evaluate("document.getElementById('size-default').checked"),
        false,
        "switch toggles with keyboard",
      );
      await sleep(220);
      await capture("switch", mode, width);

      await sample("SegmentedControl/_Intro", mode);
      await evaluate("document.querySelector('.sa-segmented-control input:checked').focus()");
      await sleep(220);
      assert.match(
        await evaluate(
          "getComputedStyle(document.querySelector('.sa-segmented-control-item:has(:checked)')).boxShadow",
        ),
        /3px/,
        "raised selected segment preserves its 3px keyboard ring",
      );

      await sample("InputOtp/_Intro", mode);
      await evaluate("document.querySelector('.sa-input-otp-input').focus()");
      await send("Input.insertText", { text: "123456" });
      assert.equal(
        await evaluate("document.querySelector('.sa-input-otp-input').value"),
        "123456",
        "OTP entry",
      );
      assert.equal(
        await evaluate("document.documentElement.scrollWidth <= innerWidth"),
        true,
        "OTP fits narrow viewport",
      );

      await sample("Sheet/_Sides", mode);
      for (const side of ["top", "right", "bottom", "left"]) {
        await evaluate(`document.getElementById('--sheet-sides-${side}').showModal()`);
        await evaluate(
          `Promise.all(document.getElementById('--sheet-sides-${side}').getAnimations().map(animation => animation.finished))`,
        );
        const bounds = await evaluate(
          `(() => {const panel = document.getElementById('--sheet-sides-${side}');const r=panel.getBoundingClientRect();return {left:r.left,right:r.right,top:r.top,bottom:r.bottom,width:innerWidth,height:innerHeight};})()`,
        );
        assert.ok(
          bounds.left >= -1 &&
            bounds.right <= bounds.width + 1 &&
            bounds.top >= -1 &&
            bounds.bottom <= bounds.height + 1,
          `${side} sheet contained at ${width}px`,
        );
        await capture(`sheet-${side}`, mode, width);
        await key("Escape", "Escape", 27);
        assert.equal(
          await evaluate(`document.getElementById('--sheet-sides-${side}').open`),
          false,
          "native Escape dismissal",
        );
      }

      await sample("Sidebar/_InsetVariant", mode);
      for (const state of ["expanded", "collapsed"]) {
        await evaluate(`document.querySelector('.sa-sidebar').dataset.state='${state}'`);
        await sleep(220);
        assert.equal(
          await evaluate("document.documentElement.scrollWidth <= innerWidth"),
          true,
          `${state} sidebar fits`,
        );
      }
      console.log(
        `PASS Parallax ${mode}/${width}: focus, selection, grouped validation, native inputs and overlay containment`,
      );
    }
  }
  await send("Emulation.setEmulatedMedia", {
    features: [{ name: "prefers-reduced-motion", value: "reduce" }],
  });
  await sample("Button/_Spinner", "dark");
  const motion = await evaluate(`(() => {
    const button = getComputedStyle(document.querySelector('.sa-button'));
    const spinner = getComputedStyle(document.querySelector('.sa-spinner'));
    return {transition:button.transitionDuration,animation:parseFloat(spinner.animationDuration),iterations:spinner.animationIterationCount};
  })()`);
  assert.equal(motion.transition, "0s");
  assert.ok(motion.animation <= 0.00001);
  assert.equal(motion.iterations, "1");
  console.log("PASS Parallax reduced motion");
  if (process.env.PARALLAX_PRO_URL) {
    for (const mode of ["light", "dark"]) {
      for (const width of [1280, 390]) {
        await send("Emulation.setDeviceMetricsOverride", {
          width,
          height: 900,
          deviceScaleFactor: 1,
          mobile: false,
        });
        const loaded = waitForEvent("Page.loadEventFired", 15000);
        await send("Page.navigate", {
          url: `${process.env.PARALLAX_PRO_URL}/DataGrid?theme=parallax&mode=${mode}`,
        });
        assert.ok(await loaded);
        await prepareFonts("Pro grid");
        assert.equal(
          await evaluate(`(() => {
          const grid = document.querySelector('.sa-data-grid');
          const table = grid.querySelector('.sa-table-container');
          if (table.scrollWidth > table.clientWidth) table.scrollLeft = table.scrollWidth;
          return grid.getBoundingClientRect().right <= innerWidth && document.documentElement.scrollWidth <= innerWidth &&
            (table.scrollWidth <= table.clientWidth || table.scrollLeft > 0);
        })()`),
          true,
          "Pro grid stays contained and wide tables remain scrollable",
        );
        await capture("pro-grid", mode, width);
      }
    }
    console.log("PASS Parallax Pro grid containment and scrolling");
  }
} finally {
  browser.ws.close();
  browser.chrome.kill();
}

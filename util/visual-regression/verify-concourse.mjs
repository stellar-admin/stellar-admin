// Real-component regressions for Concourse's selection, focus, and composite geometry.
// node util/visual-regression/verify-concourse.mjs http://localhost:5206
import assert from "node:assert/strict";
import { launchBrowser, sleep } from "./browser.mjs";

const base = process.argv[2] ?? "http://localhost:5206";
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
    url: `${base}/DocsStatic?name=${name}&layout=_CleanLayout&theme=concourse&mode=${mode}`,
  });
  assert.ok(await loaded, `${name}: loaded`);
  assert.ok(
    await evaluate(`!!document.querySelector('link[href*="stellar-admin.concourse"]')`),
    "Concourse selected",
  );
  const fontsReady = await evaluate(
    "Promise.race([document.fonts.ready.then(() => true), new Promise(resolve => setTimeout(() => resolve(false), 8000))])",
  );
  if (!fontsReady) console.warn(`${name}: font loading timed out; continuing behavioral checks`);
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
          destructive: getComputedStyle(document.documentElement).getPropertyValue('--destructive').trim() };
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
        `PASS Concourse ${mode}/${width}: focus, selection, grouped validation, native inputs and overlay containment`,
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
  console.log("PASS Concourse reduced motion");
} finally {
  browser.ws.close();
  browser.chrome.kill();
}

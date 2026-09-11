// Focused behavioural/style checks complement the full light/dark VRT captures.
// Run with a built DocsSamples instance: node util/visual-regression/verify-ledger.mjs http://localhost:5206
import assert from "node:assert/strict";
import { launchBrowser, sleep } from "./browser.mjs";

const base = process.argv[2] ?? "http://localhost:5206";
const { chrome, send, evaluate, waitForEvent } = await launchBrowser(
  process.env.CHROME_PATH ?? "chromium",
  { hideScrollbars: false },
);
async function sample(name, mode = "light") {
  const url = new URL("/DocsStatic", base);
  url.search = new URLSearchParams({ name, layout: "_CleanLayout", theme: "ledger", mode });
  const loaded = waitForEvent("Page.loadEventFired", 15000);
  await send("Page.navigate", { url: url.href });
  assert.ok(await loaded, `${name}: page loaded`);
  assert.equal(
    await evaluate("!!document.querySelector('link[href*=\"stellar-admin.ledger\"]')"),
    true,
    `${name}: Ledger stylesheet`,
  );
  assert.equal(
    await evaluate("document.querySelectorAll('sa-button,sa-input,sa-card').length"),
    0,
    `${name}: rendered tag helpers`,
  );
  await evaluate("document.fonts.ready");
}
try {
  await send("Page.enable");
  await send("DOM.enable");
  await send("CSS.enable");
  await send("Network.enable");
  await send("Network.setCacheDisabled", { cacheDisabled: true });
  await send("Emulation.setFocusEmulationEnabled", { enabled: true });
  await send("Emulation.setDeviceMetricsOverride", {
    width: 1280,
    height: 900,
    deviceScaleFactor: 1,
    mobile: false,
  });
  for (const mode of ["light", "dark"]) {
    for (const width of [1280, 390]) {
      await send("Emulation.setDeviceMetricsOverride", {
        width,
        height: 900,
        deviceScaleFactor: 1,
        mobile: false,
      });
      await sample("Sidebar/_InsetVariant", mode);
      for (const collapsed of [false, true]) {
        await evaluate(
          `document.querySelector('.sa-sidebar').dataset.state = '${collapsed ? "collapsed" : "expanded"}'`,
        );
        const inset = await evaluate(`(() => {
          const panel = document.querySelector('.sa-sidebar-inset');
          const style = getComputedStyle(panel);
          const rect = panel.getBoundingClientRect();
          return { top: rect.top, bottom: innerHeight - rect.bottom,
            radius: parseFloat(style.borderTopLeftRadius), shadow: style.boxShadow,
            left: parseFloat(style.marginLeft), overflow: style.overflowX,
            pageOverflow: document.documentElement.scrollWidth > innerWidth };
        })()`);
        assert.equal(inset.top, width >= 768 ? 8 : 0, `${mode}/${width}: inset top gutter`);
        assert.equal(inset.bottom, width >= 768 ? 8 : 0, `${mode}/${width}: inset bottom gutter`);
        assert.equal(inset.radius, width >= 768 ? 6 : 0, `${mode}/${width}: inset rounding`);
        assert.equal(
          inset.left,
          width >= 768 && collapsed ? 8 : 0,
          `${mode}/${width}: collapsed gutter`,
        );
        assert.equal(inset.pageOverflow, false, `${mode}/${width}: inset fits viewport`);
        if (width >= 768) {
          assert.notEqual(inset.shadow, "none", `${mode}: inset panel shadow`);
          assert.equal(inset.overflow, "clip", `${mode}: header follows rounded panel`);
        }
      }
      await sample("Sidebar/_FloatingVariant", mode);
      const floatingHeader = await evaluate(`(() => {
        const header = document.querySelector('.sa-app-header');
        const style = getComputedStyle(header);
        return { radius: parseFloat(style.borderBottomLeftRadius),
          rightRadius: parseFloat(style.borderBottomRightRadius), shadow: style.boxShadow,
          background: style.backgroundColor,
          bodyBackground: getComputedStyle(document.querySelector('.sa-sidebar-inset')).backgroundColor };
      })()`);
      assert.equal(
        floatingHeader.radius,
        width >= 768 ? 6 : 0,
        `${mode}/${width}: floating header lower corner`,
      );
      assert.equal(
        floatingHeader.rightRadius,
        floatingHeader.radius,
        `${mode}/${width}: matching header corners`,
      );
      assert.notEqual(
        floatingHeader.background,
        floatingHeader.bodyBackground,
        `${mode}: contrasting header retained`,
      );
      if (width >= 768)
        assert.notEqual(floatingHeader.shadow, "none", `${mode}: floating header shadow`);
      await sample("Accordion/_Intro", mode);
      await evaluate(
        "document.querySelectorAll('.sa-accordion-trigger').forEach(trigger => trigger.click())",
      );
      await sleep(350);
      const accordions =
        await evaluate(`Array.from(document.querySelectorAll('.sa-accordion-item')).map(item => {
        const title = item.querySelector('.sa-accordion-trigger');
        const content = item.querySelector('.sa-accordion-content-inner');
        function textBounds(element) {
          const walker = document.createTreeWalker(element, NodeFilter.SHOW_TEXT);
          let node;
          while ((node = walker.nextNode())) {
            if (!node.textContent.trim()) continue;
            const range = document.createRange();
            range.selectNodeContents(node);
            return range.getBoundingClientRect();
          }
        }
        const heading = textBounds(title);
        const body = textBounds(content);
        return {open: item.open, gap: body.top - heading.bottom,
          alignment: body.left - heading.left,
          bottomGap: item.getBoundingClientRect().bottom - body.bottom};
      })`);
      for (const accordion of accordions) {
        assert.equal(accordion.open, true);
        assert.ok(
          accordion.gap >= 0 && accordion.gap <= 14,
          `accordion title/content gap stays compact: ${JSON.stringify(accordion)}`,
        );
        assert.ok(Math.abs(accordion.alignment) < 1, "accordion title and content align");
        assert.ok(
          accordion.bottomGap >= 10 && accordion.bottomGap <= 16,
          "accordion content bottom spacing stays compact",
        );
      }
      for (const demo of ["Intro", "Sides"]) {
        await sample(`Sheet/_${demo}`, mode);
        const sides = demo === "Intro" ? ["right"] : ["top", "right", "bottom", "left"];
        for (const side of sides) {
          await evaluate(
            `document.querySelector('.sa-sheet-content[data-side=${side}]').showModal()`,
          );
          await sleep(550);
          const sheet = await evaluate(`(() => {
            const panel = document.querySelector('.sa-sheet-content[open]');
            const bounds = panel.getBoundingClientRect();
            const close = panel.querySelector('.sa-sheet-close').getBoundingClientRect();
            const footer = panel.querySelector('.sa-sheet-footer')?.getBoundingClientRect();
            return {top: bounds.top, bottom: bounds.bottom, height: bounds.height,
              overflow: panel.scrollHeight - panel.clientHeight,
              closeTop: close.top - bounds.top, closeRight: bounds.right - close.right,
              footerBottom: footer?.bottom};
          })()`);
          assert.ok(sheet.top >= 0 && sheet.bottom <= 900, `${demo}/${side}: sheet fits viewport`);
          assert.equal(sheet.overflow, 0, `${demo}/${side}: no unnecessary scrolling`);
          assert.ok(sheet.closeTop >= 12 && sheet.closeTop <= 13, "close button top inset");
          assert.ok(sheet.closeRight >= 12 && sheet.closeRight <= 13, "close button right inset");
          if (demo === "Sides") {
            assert.equal(
              sheet.height,
              ["top", "bottom"].includes(side) ? 450 : 900,
              `${side}: preserves requested height`,
            );
          } else {
            assert.ok(sheet.footerBottom <= sheet.bottom, "sheet footer remains visible");
          }
          await evaluate("document.querySelector('.sa-sheet-content[open]').close()");
        }
      }
    }
    await send("Emulation.setDeviceMetricsOverride", {
      width: 1280,
      height: 900,
      deviceScaleFactor: 1,
      mobile: false,
    });
    for (const demo of ["CheckboxItems", "RadioGroup"]) {
      await sample(`DropdownMenu/_${demo}`, mode);
      await evaluate("document.querySelector('[data-slot=dropdown-menu-trigger]').click()");
      await sleep(150);
      const checked = await evaluate(`(async () => {
        const item = document.querySelector('[data-state=checked]');
        item.focus();
        await new Promise(resolve => setTimeout(resolve, 250));
        const focused = getComputedStyle(item).backgroundColor;
        document.querySelector('[data-state=unchecked]').focus();
        await new Promise(resolve => setTimeout(resolve, 250));
        return {focused, idle: getComputedStyle(item).backgroundColor,
          indicator: getComputedStyle(item.querySelector('.sa-dropdown-menu-item-indicator')).display};
      })()`);
      assert.equal(
        checked.idle,
        "rgba(0, 0, 0, 0)",
        `${demo}: checked items have no persistent highlight`,
      );
      assert.notEqual(checked.focused, checked.idle, `${demo}: focus retains its highlight`);
      assert.notEqual(checked.indicator, "none", `${demo}: checked indicator stays visible`);
    }
    for (const width of [1280, 390]) {
      await send("Emulation.setDeviceMetricsOverride", {
        width,
        height: 900,
        deviceScaleFactor: 1,
        mobile: false,
      });
      await sample("AlertDialog/_SmallMedia", mode);
      await evaluate("document.querySelector('[command=show-modal]').click()");
      const layout = await evaluate(`(() => {
        const dialog = document.querySelector('.sa-alert-dialog-content');
        const header = dialog.querySelector('.sa-alert-dialog-header');
        const media = header.querySelector('.sa-alert-dialog-media').getBoundingClientRect();
        const title = header.querySelector('.sa-alert-dialog-title').getBoundingClientRect();
        const description = header.querySelector('.sa-alert-dialog-description').getBoundingClientRect();
        const bounds = header.getBoundingClientRect();
        return {open: dialog.open, align: getComputedStyle(header).textAlign,
          centerOffset: media.x + media.width / 2 - bounds.x - bounds.width / 2,
          mediaGap: title.top - media.bottom, textGap: description.top - title.bottom};
      })()`);
      assert.equal(layout.open, true);
      assert.equal(layout.align, "center", "small alert dialog text stays centered");
      assert.ok(Math.abs(layout.centerOffset) < 1, "small alert dialog media stays centered");
      assert.equal(layout.mediaGap, 14, "media has breathing room above the title");
      assert.equal(layout.textGap, 6, "title and description retain their spacing");
      await sample("AlertDialog/_Intro", mode);
      await evaluate("document.querySelector('[command=show-modal]').click()");
      const normalGap = await evaluate(`(() => {
        const header = document.querySelector('.sa-alert-dialog-header');
        const title = header.querySelector('.sa-alert-dialog-title').getBoundingClientRect();
        const description = header.querySelector('.sa-alert-dialog-description').getBoundingClientRect();
        return description.top - title.bottom;
      })()`);
      assert.equal(normalGap, 6, "normal alert dialog title and description retain their spacing");
    }
    await send("Emulation.setDeviceMetricsOverride", {
      width: 1280,
      height: 900,
      deviceScaleFactor: 1,
      mobile: false,
    });

    for (const demo of ["Intro", "Border"]) {
      await sample(`Table/_${demo}`, mode);
      const table = await evaluate(`(() => {
        const container = document.querySelector('.sa-table-container');
        const table = container.querySelector('.sa-table');
        const caption = table.querySelector('caption');
        return {
          containerWidth: container.getBoundingClientRect().width,
          tableWidth: table.getBoundingClientRect().width,
          border: getComputedStyle(container).borderTopWidth,
          radius: getComputedStyle(container).borderRadius,
          wrapperOverflow: getComputedStyle(container.parentElement).overflow,
          captionTop: caption?.getBoundingClientRect().top,
          footerBottom: table.querySelector('tfoot').getBoundingClientRect().bottom
        };
      })()`);
      assert.ok(Math.abs(table.tableWidth - table.containerWidth) < 1, "table fills its container");
      assert.equal(table.border, "0px", "table container does not duplicate an explicit border");
      assert.equal(table.radius, "0px", "table surface has no independent corner radius");
      if (demo === "Border") {
        assert.equal(
          table.wrapperOverflow,
          "hidden",
          "outer border clips the table surface to its corners",
        );
      }
      if (demo === "Intro") {
        assert.ok(table.captionTop >= table.footerBottom, "table caption appears below the footer");
      }
      const overflow = await evaluate(`(() => {
        const container = document.querySelector('.sa-table-container');
        container.style.width = '280px';
        container.scrollLeft = 40;
        return {overflow: getComputedStyle(container).overflowX,
          scrollLeft: container.scrollLeft, width: container.clientWidth};
      })()`);
      assert.equal(overflow.overflow, "auto", "wide tables scroll within their container");
      assert.equal(overflow.width, 280);
      assert.equal(overflow.scrollLeft, 40, "narrow table container remains scrollable");
    }

    await sample("Avatar/_Intro", mode);
    const avatars = await evaluate(`Array.from(document.querySelectorAll('.sa-avatar'), avatar => ({
      radius: getComputedStyle(avatar).borderRadius,
      borderRadius: getComputedStyle(avatar, '::after').borderRadius,
      width: avatar.getBoundingClientRect().width
    }))`);
    assert.equal(avatars.length, 4, "standalone and grouped avatars rendered");
    for (const avatar of avatars) {
      assert.equal(avatar.borderRadius, avatar.radius, "avatar border follows the avatar shape");
      assert.ok(parseFloat(avatar.borderRadius) >= avatar.width / 2, "avatar border is circular");
    }

    await sample("Slider/_Intro", mode);
    assert.equal(
      await evaluate("getComputedStyle(document.querySelector('.sa-slider-track')).height"),
      "6px",
      "visible horizontal slider track",
    );
    await evaluate("document.querySelector('[role=slider]').focus()");
    const value = await evaluate(
      "Number(document.querySelector('[role=slider]').getAttribute('aria-valuenow'))",
    );
    await send("Input.dispatchKeyEvent", {
      type: "keyDown",
      key: "ArrowRight",
      code: "ArrowRight",
      windowsVirtualKeyCode: 39,
    });
    assert.ok(
      (await evaluate(
        "Number(document.querySelector('[role=slider]').getAttribute('aria-valuenow'))",
      )) > value,
      "slider keyboard value",
    );

    await sample("Switch/_Intro", mode);
    assert.equal(
      await evaluate("getComputedStyle(document.querySelector('.sa-switch')).width"),
      "38px",
    );
    const before = await evaluate(
      "getComputedStyle(document.querySelector('.sa-switch-thumb')).transform",
    );
    await evaluate("document.querySelector('.sa-switch').click()");
    await sleep(250);
    assert.notEqual(
      await evaluate("getComputedStyle(document.querySelector('.sa-switch-thumb')).transform"),
      before,
      "switch thumb tracks native checked state",
    );

    const switchPageLoaded = waitForEvent("Page.loadEventFired", 15000);
    await send("Page.navigate", { url: new URL(`/Switch?theme=ledger&mode=${mode}`, base).href });
    assert.ok(await switchPageLoaded, "switch demo page loaded");
    for (const checked of [false, true]) {
      await evaluate(
        `document.querySelectorAll('.sa-switch').forEach(input => input.checked = ${checked})`,
      );
      await sleep(250);
      const switches =
        await evaluate(`(() => [...document.querySelectorAll('.sa-switch-wrapper')].map(wrapper => {
        const track = wrapper.querySelector('.sa-switch').getBoundingClientRect();
        const thumb = wrapper.querySelector('.sa-switch-thumb').getBoundingClientRect();
        return {size: wrapper.dataset.size, center: (thumb.top + thumb.bottom - track.top - track.bottom) / 2,
          left: thumb.left - track.left, right: track.right - thumb.right};
      }))()`);
      assert.ok(
        switches.length >= 7,
        "all switch demos including model binding and disabled states",
      );
      for (const control of switches) {
        assert.ok(
          Math.abs(control.center) < 0.5,
          `${mode}: ${control.size} thumb centered vertically`,
        );
        assert.ok(control.left >= 0 && control.right >= 0, "thumb stays inside track");
        assert.equal(
          checked ? control.right : control.left,
          3,
          "thumb has a 3px inset at the active end",
        );
      }
    }

    await sample("InputOtp/_Intro", mode);
    await evaluate("document.querySelector('.sa-input-otp-input').focus()");
    await send("Input.insertText", { text: "123" });
    assert.equal(
      await evaluate(
        "[...document.querySelectorAll('.sa-input-otp-slot')].map(e=>e.textContent).join('')",
      ),
      "123",
    );
    assert.ok(await evaluate("!!document.querySelector('.sa-input-otp-slot[data-active=true]')"));

    await sample("InputGroup/_Intro", mode);
    await evaluate("document.querySelector('.sa-input-group-input').focus()");
    assert.notEqual(
      await evaluate("getComputedStyle(document.querySelector('.sa-input-group')).boxShadow"),
      "none",
    );
    assert.equal(
      await evaluate(
        "getComputedStyle(document.querySelector('.sa-input-group-input')).borderTopWidth",
      ),
      "0px",
    );

    await sample("ButtonGroup/_WithSelect", mode);
    const nestedGroupSpacing = await evaluate(`(() => {
      const outer = document.querySelector('.sa-button-group');
      const [fields, action] = outer.children;
      return {gap: action.getBoundingClientRect().left - fields.getBoundingClientRect().right,
        innerGap: getComputedStyle(fields).columnGap};
    })()`);
    assert.deepEqual(
      nestedGroupSpacing,
      { gap: 8, innerGap: "0px" },
      `${mode}: nested groups separated, select and input joined`,
    );

    await sample("InputGroup/_Buttons", mode);
    const leadingInsets = await evaluate(`(() => {
      return [...document.querySelectorAll('.sa-input-group')].map(group => {
        const leading = group.querySelector('[data-align="inline-start"]') ?? group.querySelector('input');
        return leading.getBoundingClientRect().left + parseFloat(getComputedStyle(leading).paddingLeft) - group.getBoundingClientRect().left;
      });
    })()`);
    assert.deepEqual(
      leadingInsets,
      [7, 7, 7],
      `${mode}: button examples share a 6px inset inside the border`,
    );
    assert.equal(
      await evaluate(
        "getComputedStyle(document.querySelector('.sa-input-group-input[readonly]')).backgroundColor",
      ),
      "rgba(0, 0, 0, 0)",
      `${mode}: read-only grouped input shares the group surface`,
    );
    for (const example of ["Text", "Label"]) {
      await sample(`InputGroup/_${example}`, mode);
      const spacing = await evaluate(`(() => {
        const group = [...document.querySelectorAll('.sa-input-group')].at(-1);
        const input = group.querySelector('input');
        const style = getComputedStyle(input);
        return {height: input.getBoundingClientRect().height, top: parseFloat(style.paddingTop), bottom: parseFloat(style.paddingBottom)};
      })()`);
      assert.ok(
        spacing.height >= 38 && spacing.top >= 9 && spacing.bottom >= 9,
        `${mode}: ${example} stacked input retains vertical spacing`,
      );
    }
    await sample("InputGroup/_ButtonGroup", mode);
    for (const radius of ["6px", "12px"]) {
      await evaluate(`document.documentElement.style.setProperty('--radius', '${radius}')`);
      for (const orientation of ["horizontal", "vertical"]) {
        const corners = await evaluate(`(() => {
          const group = document.querySelector('.sa-button-group');
          group.classList.remove('sa-button-group-orientation-horizontal', 'sa-button-group-orientation-vertical');
          group.classList.add('sa-button-group-orientation-${orientation}');
          const style = getComputedStyle(group.lastElementChild);
          return [style.${orientation === "horizontal" ? "borderTopRightRadius" : "borderBottomLeftRadius"}, style.borderBottomRightRadius];
        })()`);
        assert.deepEqual(corners, [radius, radius], `${mode}: ${orientation} group end corners`);
      }
    }

    await sample("Button/_Intro", mode);
    const sidebar = await evaluate(`(() => {
      const content = document.createElement('div');
      content.className = 'sa-sidebar-content';
      content.style.cssText = 'position:fixed;width:200px;height:100px;flex:none';
      content.innerHTML = '<div style="min-height:400px">Overflowing navigation</div>';
      document.body.append(content);
      const style = getComputedStyle(content);
      content.scrollTop = 50;
      const result = {width: style.scrollbarWidth, scrollTop: content.scrollTop, gutter: content.offsetWidth - content.clientWidth};
      content.remove();
      return result;
    })()`);
    assert.equal(sidebar.width, "none", `${mode}: sidebar scrollbar hidden by theme`);
    assert.equal(sidebar.gutter, 0, `${mode}: sidebar has no scrollbar gutter`);
    assert.equal(sidebar.scrollTop, 50, `${mode}: sidebar remains scrollable`);
    const { root } = await send("DOM.getDocument");
    const { nodeId } = await send("DOM.querySelector", {
      nodeId: root.nodeId,
      selector: ".sa-button-variant-default",
    });
    await send("CSS.forcePseudoState", {
      nodeId,
      forcedPseudoClasses: ["active", "focus-visible"],
    });
    await sleep(250);
    const active = await evaluate(
      "(()=>{let s=getComputedStyle(document.querySelector('.sa-button-variant-default'));return {transform:s.transform,shadow:s.boxShadow}})()",
    );
    assert.equal(active.transform, "matrix(1, 0, 0, 1, 0, 1)");
    assert.ok(
      active.shadow.includes("inset") && active.shadow.includes("3px"),
      "pressed shadow plus focus ring",
    );
    await send("CSS.forcePseudoState", { nodeId, forcedPseudoClasses: [] });
    await evaluate("document.documentElement.style.setProperty('--radius','12px')");
    assert.equal(
      await evaluate("getComputedStyle(document.querySelector('.sa-button')).borderRadius"),
      "12px",
      "radius customization",
    );
    await evaluate("document.documentElement.style.removeProperty('--radius')");
    const contrast = await evaluate(`(() => {
      const canvas = document.createElement('canvas'); const ctx=canvas.getContext('2d');
      const rgb = color => {ctx.clearRect(0,0,1,1);ctx.fillStyle=color;ctx.fillRect(0,0,1,1);return [...ctx.getImageData(0,0,1,1).data].slice(0,3).map(v=>v/255);};
      const lum = color => rgb(color).map(v=>v<=.04045?v/12.92:((v+.055)/1.055)**2.4).reduce((sum,v,i)=>sum+v*[.2126,.7152,.0722][i],0);
      const ratio = (a,b) => (Math.max(lum(a),lum(b))+.05)/(Math.min(lum(a),lum(b))+.05);
      const body=getComputedStyle(document.body), button=getComputedStyle(document.querySelector('.sa-button-variant-default'));
      return {body:ratio(body.color,body.backgroundColor),primary:ratio(button.color,button.backgroundColor)};
    })()`);
    assert.ok(
      contrast.body >= 4.5 && contrast.primary >= 4.5,
      `${mode} body/primary contrast: ${JSON.stringify(contrast)}`,
    );
    console.log(
      `${mode}: controls, states, radius override, body/primary contrast passed`,
      contrast,
    );
  }
  await send("Emulation.setEmulatedMedia", {
    features: [{ name: "prefers-reduced-motion", value: "reduce" }],
  });
  assert.equal(
    await evaluate("getComputedStyle(document.querySelector('.sa-button')).transitionDuration"),
    "0s",
  );
  console.log("Ledger focused verification passed.");
} finally {
  chrome.kill();
}

// Run against ComponentPlayground: node util/dropdown-menu/check.mjs http://localhost:5207
import assert from "node:assert/strict";
import { launchBrowser, sleep } from "../visual-regression/browser.mjs";

const url = process.argv[2] ?? "http://localhost:5207";
const page = `${url}/Demo/DropdownMenuItems`;
const browser = await launchBrowser(process.env.CHROME_PATH ?? "chromium");
const results = [];

// Trusted input through the DevTools Protocol, since synthetic events skip native button activation
const keys = {
  Enter: { key: "Enter", code: "Enter", windowsVirtualKeyCode: 13, text: "\r" },
  Space: { key: " ", code: "Space", windowsVirtualKeyCode: 32, text: " " },
  ArrowDown: { key: "ArrowDown", code: "ArrowDown", windowsVirtualKeyCode: 40 },
  ArrowRight: { key: "ArrowRight", code: "ArrowRight", windowsVirtualKeyCode: 39 },
  End: { key: "End", code: "End", windowsVirtualKeyCode: 35 },
};

async function press(name) {
  const { text, ...key } = keys[name];
  await browser.send("Input.dispatchKeyEvent", { type: "keyDown", ...key, ...(text ? { text } : {}) });
  await browser.send("Input.dispatchKeyEvent", { type: "keyUp", ...key });
  await sleep(60);
}

async function click(selector) {
  const point = await browser.evaluate(`(() => {
    const r = document.querySelector(${JSON.stringify(selector)}).getBoundingClientRect();
    return { x: r.left + r.width / 2, y: r.top + r.height / 2 };
  })()`);
  await browser.send("Input.dispatchMouseEvent", { type: "mouseMoved", ...point });
  await browser.send("Input.dispatchMouseEvent", { type: "mousePressed", ...point, button: "left", clickCount: 1 });
  await browser.send("Input.dispatchMouseEvent", { type: "mouseReleased", ...point, button: "left", clickCount: 1 });
  await sleep(80);
}

async function load(query = "") {
  const loaded = browser.waitForEvent("Page.loadEventFired", 15000);
  await browser.send("Page.navigate", { url: `${page}${query}` });
  assert.ok(await loaded, "fixture loaded");
  await browser.evaluate("customElements.whenDefined('sel-dropdown-menu').then(() => true)");
  await sleep(150);
}

async function open() {
  await click("#check-trigger");
  assert.ok(await isOpen(), "menu opens");
}

const isOpen = () => browser.evaluate("document.getElementById('check-menu').matches(':popover-open')");
const active = () => browser.evaluate("document.activeElement?.id ?? ''");
const counter = (name) => browser.evaluate(`window.${name} ?? 0`);

async function focusItem(id) {
  for (let i = 0; i < 10 && (await active()) !== id; i++) {
    await press("ArrowDown");
  }
  assert.equal(await active(), id, `${id} focused by the arrow keys`);
}

async function waitForPost() {
  const loaded = browser.waitForEvent("Page.loadEventFired", 5000);
  if (!(await loaded)) {
    return null;
  }
  return browser.evaluate("document.querySelector('#posted')?.textContent ?? null");
}

async function check(name, run) {
  try {
    await run();
    results.push(`ok   ${name}`);
  } catch (error) {
    results.push(`FAIL ${name}: ${error.message}`);
  }
}

try {
  await browser.send("Page.enable");

  for (const theme of ["", "ledger", "ops", "soft"]) {
    await check(`plain and link items look the same (${theme || "default"})`, async () => {
      await load(theme ? `?theme=${theme}` : "");
      await open();
      // Park focus on the menu so neither item has the highlight
      await browser.evaluate("document.getElementById('check-menu').focus()");
      const styles = await browser.evaluate(`(() => {
        const pick = (id) => {
          const el = document.getElementById(id);
          const s = getComputedStyle(el);
          const r = el.getBoundingClientRect();
          return { width: r.width, height: r.height, left: r.left, ...Object.fromEntries(['display', 'padding', 'gap', 'borderRadius', 'borderWidth', 'backgroundColor', 'color', 'fontFamily', 'fontSize', 'fontWeight', 'lineHeight', 'textAlign', 'cursor', 'justifyContent', 'alignItems'].map(k => [k, s[k]])) };
        };
        return { button: pick('plain'), link: pick('link') };
      })()`);
      assert.deepEqual(styles.button, styles.link);
    });
  }

  await check("mouse click runs the item once and closes the menu", async () => {
    await load();
    await open();
    await click("#plain");
    assert.equal(await counter("plainClicks"), 1);
    assert.equal(await isOpen(), false);
  });

  for (const key of ["Enter", "Space"]) {
    await check(`${key} runs the item once and closes the menu`, async () => {
      await load();
      await open();
      await focusItem("plain");
      await press(key);
      assert.equal(await counter("plainClicks"), 1);
      assert.equal(await isOpen(), false);
    });
  }

  await check("close-on-click=false keeps the menu open", async () => {
    await load();
    await open();
    await click("#stay");
    await focusItem("stay");
    await press("Enter");
    assert.equal(await counter("stayClicks"), 2);
    assert.equal(await isOpen(), true);
  });

  await check("arrow keys skip a disabled item and clicks ignore it", async () => {
    await load();
    await open();
    const order = [];
    for (let i = 0; i < 7; i++) {
      order.push(await active());
      await press("ArrowDown");
    }
    assert.ok(!order.includes("disabled") && !order.includes("disabled-submit"), `focus order ${order}`);
    await click("#disabled");
    assert.equal(await counter("disabledClicks"), 0);
    assert.equal(await isOpen(), true);
  });

  await check("type-ahead focuses the matching item", async () => {
    await load();
    await open();
    await browser.send("Input.dispatchKeyEvent", { type: "keyDown", key: "s", code: "KeyS", text: "s" });
    await browser.send("Input.dispatchKeyEvent", { type: "keyUp", key: "s", code: "KeyS" });
    await sleep(60);
    assert.equal(await active(), "stay");
  });

  await check("mouse click on a submit item posts its form", async () => {
    await load();
    await open();
    const posted = waitForPost();
    await click("#submit");
    assert.equal(await posted, "posted:logout");
  });

  for (const key of ["Enter", "Space"]) {
    await check(`${key} on a submit item submits its form once`, async () => {
      await load();
      await open();
      await browser.evaluate(
        "document.addEventListener('submit', e => { e.preventDefault(); window.submits = (window.submits ?? 0) + 1; })",
      );
      await focusItem("submit");
      await press(key);
      await sleep(300);
      assert.equal(await counter("submits"), 1);
      assert.equal(await isOpen(), false);
    });
  }

  await check("Enter on a submit item posts its form", async () => {
    await load();
    await open();
    await focusItem("submit");
    const posted = waitForPost();
    await press("Enter");
    assert.equal(await posted, "posted:logout");
  });

  await check("a disabled submit item does not post", async () => {
    await load();
    await open();
    await browser.evaluate("document.getElementById('disabled-submit').click()");
    await sleep(500);
    assert.equal(await browser.evaluate("document.querySelector('#posted')"), null);
  });

  await check("Enter on a link item navigates", async () => {
    await load();
    await open();
    await focusItem("link");
    await press("Enter");
    assert.equal(await browser.evaluate("location.hash"), "#linked");
    assert.equal(await isOpen(), false);
  });

  await check("submenu opens and its plain item runs once", async () => {
    await load();
    await open();
    await press("End");
    await press("ArrowRight");
    assert.equal(await active(), "nested");
    await press("Enter");
    assert.equal(await counter("nestedClicks"), 1);
    assert.equal(await isOpen(), false);
  });

  await check("Escape closes the menu", async () => {
    await load();
    await open();
    await browser.send("Input.dispatchKeyEvent", { type: "keyDown", key: "Escape", code: "Escape", windowsVirtualKeyCode: 27 });
    await browser.send("Input.dispatchKeyEvent", { type: "keyUp", key: "Escape", code: "Escape", windowsVirtualKeyCode: 27 });
    await sleep(80);
    assert.equal(await isOpen(), false);
  });
} finally {
  browser.ws.close();
  browser.chrome.kill();
}

console.log(results.join("\n"));
process.exitCode = results.some((r) => r.startsWith("FAIL")) ? 1 : 0;

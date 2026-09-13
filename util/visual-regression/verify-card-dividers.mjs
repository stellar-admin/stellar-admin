// node util/visual-regression/verify-card-dividers.mjs http://localhost:5206
import assert from "node:assert/strict";
import { writeFile } from "node:fs/promises";
import { launchBrowser } from "./browser.mjs";

const base = process.argv[2] ?? "http://localhost:5206";
const browser = await launchBrowser(process.env.CHROME_PATH ?? "chromium", {
  hideScrollbars: false,
});
const failures = [];
try {
  await browser.send("Page.enable");
  for (const theme of ["aurora", "meridian", "observatory", "parallax"]) {
    for (const mode of ["light", "dark"]) {
      for (const width of [1280, 390]) {
        await browser.send("Emulation.setDeviceMetricsOverride", {
          width,
          height: 1000,
          deviceScaleFactor: 1,
          mobile: false,
        });
        const loaded = browser.waitForEvent("Page.loadEventFired", 15000);
        await browser.send("Page.navigate", {
          url: `${base}/Card?theme=${theme}&mode=${mode}`,
        });
        assert.ok(await loaded, "Card page loaded");
        const cards = await browser.evaluate(`(() => {
          return [...document.querySelectorAll('.sa-card')].map(card => {
            const header = card.querySelector(':scope > .sa-card-header');
            const footer = card.querySelector(':scope > .sa-card-footer');
            if (!header || !footer) return null;
            const previous = footer.previousElementSibling;
            const footerStyle = getComputedStyle(footer);
            const previousStyle = getComputedStyle(previous);
            return {
              title: header.textContent.trim().slice(0, 40),
              adjacent: previous === header,
              divider: parseFloat(previousStyle.borderBottomWidth) + parseFloat(footerStyle.borderTopWidth),
              gap: footer.getBoundingClientRect().top - previous.getBoundingClientRect().bottom,
              headerBorder: parseFloat(getComputedStyle(header).borderBottomWidth),
            };
          }).filter(Boolean);
        })()`);
        assert.ok(
          cards.some((card) => card.adjacent),
          "Header/footer composition present",
        );
        assert.ok(
          cards.some((card) => !card.adjacent),
          "Header/content/footer composition present",
        );
        for (const card of cards) {
          if (card.divider !== 1 || Math.abs(card.gap) > 0.1 || card.headerBorder !== 1) {
            failures.push({ theme, mode, width, ...card });
          }
        }
        if (process.env.CARD_SCREENSHOT_PREFIX && width === 1280) {
          const shot = await browser.send("Page.captureScreenshot", { format: "png" });
          await writeFile(
            `${process.env.CARD_SCREENSHOT_PREFIX}-${theme}-${mode}.png`,
            Buffer.from(shot.data, "base64"),
          );
        }
        console.log(`${theme} ${mode} ${width}: checked ${cards.length} card compositions`);
      }
    }
  }
  assert.deepEqual(failures, [], "Card section joins must have exactly one continuous 1px divider");
  console.log("PASS: all card dividers");
} finally {
  browser.ws.close();
  browser.chrome.kill();
}

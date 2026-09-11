// Keep native scrollbars visible: bordered tracks must not overflow vertically.
// node util/visual-regression/verify-concourse-progress.mjs http://localhost:5206
import assert from "node:assert/strict";
import { mkdir, writeFile } from "node:fs/promises";
import { launchBrowser, sleep } from "./browser.mjs";

const base = process.argv[2] ?? "http://localhost:5206";
const output = new URL("./snapshots/concourse-progress/", import.meta.url);
const browser = await launchBrowser(process.env.CHROME_PATH ?? "chromium", {
  hideScrollbars: false,
});
const failures = [];
try {
  await mkdir(output, { recursive: true });
  await browser.send("Page.enable");
  for (const mode of ["light", "dark"]) {
    for (const width of [1280, 390]) {
      await browser.send("Emulation.setDeviceMetricsOverride", {
        width,
        height: 900,
        deviceScaleFactor: 1,
        mobile: false,
      });
      for (const name of ["Intro", "WithLabel", "MinMax", "FileUploadList"]) {
        const loaded = browser.waitForEvent("Page.loadEventFired", 15000);
        await browser.send("Page.navigate", {
          url: `${base}/DocsStatic?name=Progress/_${name}&layout=_CleanLayout&theme=concourse&mode=${mode}`,
        });
        assert.ok(await loaded, `${name}: loaded`);
        await sleep(600);
        const tracks =
          await browser.evaluate(`Array.from(document.querySelectorAll('.sa-progress-track'), track => {
          const indicator = track.querySelector('.sa-progress-indicator');
          const bounds = indicator.getBoundingClientRect();
          return {
            scrollHeight: track.scrollHeight,
            clientHeight: track.clientHeight,
            clientWidth: track.clientWidth,
            height: bounds.height,
            width: bounds.width,
            percentage: parseFloat(indicator.style.width),
          };
        })`);
        assert.ok(tracks.length, `${name}: progress tracks rendered`);
        for (const track of tracks) {
          if (
            track.scrollHeight > track.clientHeight ||
            track.height > track.clientHeight ||
            Math.abs(track.width - (track.clientWidth * track.percentage) / 100) > 1
          ) {
            failures.push({ name, mode, viewportWidth: width, ...track });
          }
        }
        const capture = await browser.send("Page.captureScreenshot", { format: "png" });
        await writeFile(
          new URL(`${name}-${mode}-${width}.png`, output),
          Buffer.from(capture.data, "base64"),
        );
      }
    }
  }
  assert.deepEqual(failures, [], "Progress fills must fit the track without native scrollbars");
  console.log(
    "Concourse progress: all four samples passed in light/dark at desktop/mobile widths.",
  );
} finally {
  browser.ws.close();
  browser.chrome.kill();
}

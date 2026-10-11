// Screenshots of every page in the base theme, each preset and seeded random themes, light and dark,
// for review at checkpoints. Writes <out>/<theme>-<mode>-<page>.jpg. See README.md.
//
//   node util/theme-check/screenshots.mjs --out <folder> [options] <url>...

import { mkdirSync, writeFileSync } from "node:fs";
import { launchBrowser, sleep } from "../visual-regression/browser.mjs";
import { applyTheme, parseArguments, themesFor, withMode } from "./themes.mjs";

const options = parseArguments(process.argv.slice(2), {
  out: "",
  presets: "util/theme-check/presets",
  random: "6",
  seed: "7",
  modes: "light,dark",
  width: "1400",
  maxHeight: "6000",
  browser: process.env.CHROME_PATH ?? "chromium",
});
if (!options.out) throw new Error("Pass --out <folder>");
mkdirSync(options.out, { recursive: true });

const pageName = (url) => {
  const { pathname, searchParams } = new URL(url);
  const name = [pathname.replace(/\.html$/, ""), searchParams.get("sample")].filter(Boolean).join("-");
  return name.replace(/[^a-z0-9]+/gi, "-").replace(/^-|-$/g, "").toLowerCase() || "index";
};

const { chrome, send, waitForEvent, evaluate } = await launchBrowser(options.browser);
try {
  await send("Page.enable");
  for (const theme of themesFor(options))
    for (const mode of options.modes.split(","))
      for (const url of options.urls) {
        const width = +options.width;
        await send("Emulation.setDeviceMetricsOverride", { width, height: 1000, deviceScaleFactor: 1, mobile: false });
        const loaded = waitForEvent("Page.loadEventFired", 15000);
        await send("Page.navigate", { url: withMode(url, mode) });
        await loaded;
        await sleep(800);
        await evaluate(applyTheme(theme));
        await sleep(300);
        const height = Math.min(await evaluate("document.documentElement.scrollHeight"), +options.maxHeight);
        await send("Emulation.setDeviceMetricsOverride", { width, height, deviceScaleFactor: 1, mobile: false });
        await sleep(300);
        const shot = await send("Page.captureScreenshot", { format: "jpeg", quality: 85 });
        const file = `${options.out}/${theme.name}-${mode}-${pageName(url)}.jpg`;
        writeFileSync(file, Buffer.from(shot.data, "base64"));
        console.log(file);
      }
} finally {
  chrome.kill();
}

// Contrast check for token-driven themes: WCAG text contrast and the separation of parts (fills and
// borders against what is behind them), for the base theme, each preset and seeded random themes, in
// light and dark mode, on every page given. See README.md.
//
//   node util/theme-check/contrast.mjs [options] <url>...

import { writeFileSync } from "node:fs";
import { launchBrowser, sleep } from "../visual-regression/browser.mjs";
import { pageCheck } from "./page-check.mjs";
import { applyTheme, parseArguments, themesFor, withMode } from "./themes.mjs";

const options = parseArguments(process.argv.slice(2), {
  presets: "sandbox/html/token-spec/presets",
  random: "12",
  seed: "7",
  modes: "light,dark",
  minSeparation: "1.1",
  out: "",
  browser: process.env.CHROME_PATH ?? "chromium",
});

const { chrome, send, waitForEvent, evaluate } = await launchBrowser(options.browser);
const report = [];
try {
  await send("Page.enable");
  await send("Emulation.setDeviceMetricsOverride", {
    width: 1400,
    height: 1000,
    deviceScaleFactor: 1,
    mobile: false,
  });
  for (const theme of themesFor(options))
    for (const mode of options.modes.split(","))
      for (const url of options.urls) {
        const loaded = waitForEvent("Page.loadEventFired", 15000);
        await send("Page.navigate", { url: withMode(url, mode) });
        await loaded;
        await sleep(800);
        await evaluate(applyTheme(theme));
        await sleep(300);
        const result = await evaluate(`(${pageCheck})(${+options.minSeparation})`);
        report.push({ theme: theme.name, mode, url, knobs: theme.knobs, ...result });
      }
} finally {
  chrome.kill();
}

if (options.out) writeFileSync(options.out, JSON.stringify(report, null, 1));

const grouped = (key, format) => {
  const groups = {};
  for (const run of report)
    for (const item of run[key])
      (groups[item.element] ??= []).push(`${run.theme}/${run.mode} ${format(item)}`);
  return Object.entries(groups).sort((a, b) => b[1].length - a[1].length);
};
const failures = report.reduce((n, run) => n + run.failures.length, 0);
const faint = report.reduce((n, run) => n + run.faint.length, 0);

for (const run of report)
  console.log(
    `${run.theme.padEnd(18)} ${run.mode.padEnd(5)} ${run.url}: ${run.texts} texts, ${run.failures.length} below; ${run.parts} parts, ${run.faint.length} faint`,
  );
console.log(`\nText below contrast: ${failures} of ${report.reduce((n, run) => n + run.texts, 0)}`);
for (const [element, hits] of grouped("failures", (f) => `${f.contrast} "${f.text}"`).slice(0, 40))
  console.log(`${String(hits.length).padStart(4)}  ${element}   e.g. ${hits.slice(0, 3).join(", ")}`);
console.log(
  `\nFaint parts (separation below ${options.minSeparation}): ${faint} of ${report.reduce((n, run) => n + run.parts, 0)}`,
);
for (const [element, hits] of grouped("faint", (f) => `${f.separation} "${f.text}"`).slice(0, 40))
  console.log(`${String(hits.length).padStart(4)}  ${element}   e.g. ${hits.slice(0, 3).join(", ")}`);

process.exit(failures ? 1 : 0);

// Visual-regression tool for the DocsSamples site.
//
// Captures a full-page screenshot for every sample page at two viewports (plus scripted
// overlay open-state scenarios), then pixel-diffs two capture runs with pixelmatch. Pixels
// are exhaustive by construction — any change visible in a default-state screenshot produces
// a signal — where the tool's earlier curated computed-style comparison could always be
// missing the next property (the radio stroke regression was invisible to it).
//
// Pixel comparison is only deterministic when both runs share one environment (browser
// build, fonts, rasterizer). Capture base and head on the same machine in one sitting —
// the PR workflow does exactly that on a single CI runner. Comparing captures from
// different machines or browser versions will drown in rasterization noise.
//
//   node util/visual-regression/vrt.mjs capture --url http://localhost:5205 --out snapshots/base
//   node util/visual-regression/vrt.mjs compare snapshots/base snapshots/head [--out snapshots/diff]
//
// The DocsSamples app must already be running; page discovery reads this repo's
// docs/DocsSamples/Pages (override with --pages). The browser binary defaults to `chromium`;
// override with --browser <path> or the CHROME_PATH env var (GitHub runners ship Google
// Chrome, e.g. --browser google-chrome). Requires network access for the Geist webfont.
//
// Determinism controls, applied identically to every run: animations/transitions/caret are
// disabled; external image hosts (unsplash, dicebear) are blocked; fonts are awaited; fixed
// browser flags and viewport metrics. Same-environment captures are byte-identical in
// practice (selftest.mjs asserts this), so pixels flagged as anti-aliasing are still real
// changes — they are reported in a separate "AA-only" count (hairline changes) rather than
// counted into the headline changed-pixel number.
//
// compare exits 0 when the runs match, 1 when any page differs (report.md, summary.json,
// and per-page diff images land in the --out dir), 2 on usage or capture errors.
//
// Known limits: hover/focus-visible states are not swept (headless chromium reports
// hover:none; force them ad hoc with --blink-settings=primaryHoverType=2,... and
// CSS.forcePseudoState); only pages with a DocsSamples sample are covered; content clipped
// inside scrollable containers or stacked under overlays is not composited into screenshots.

import { spawn } from "node:child_process";
import { mkdirSync, readdirSync, readFileSync, statSync, writeFileSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import pixelmatch from "pixelmatch";
import { PNG } from "pngjs";

const repoRoot = resolve(dirname(fileURLToPath(import.meta.url)), "..", "..");
const defaultPagesDir = join(repoRoot, "docs", "DocsSamples", "Pages");

const VIEWPORTS = [
  { name: "desktop", width: 1280, height: 900 },
  { name: "mobile", width: 390, height: 844 },
];

const BLOCKED_URLS = ["*images.unsplash.com*", "*api.dicebear.com*"];

const FREEZE_CSS = `*, *::before, *::after {
  animation: none !important;
  transition: none !important;
  caret-color: transparent !important;
}`;

// Scenario screenshots: interactions performed after the initial screenshot, captured as
// "<page>__<name>" at the desktop viewport. `trigger` is a CSS selector clicked in page
// context (first match). Overlay open-states are otherwise invisible to the sweep.
const SCENARIOS = [
  { page: "DropdownMenu", name: "open", trigger: "[data-slot=dropdown-menu-trigger]" },
  { page: "Sheet", name: "open", trigger: 'button[command="show-modal"]' },
  { page: "Dialog", name: "open", trigger: 'button[command="show-modal"]' },
  { page: "AlertDialog", name: "open", trigger: 'button[command="show-modal"]' },
  { page: "Popover", name: "open", trigger: "button[popovertarget^='--popover']" },
  { page: "Collapsible", name: "open", trigger: 'button[command="--toggle"]' },
  { page: "Sidebar", name: "open", trigger: 'button[command="show-modal"]' },
];

// pixelmatch per-channel color distance threshold (0 = exact). 0.1 tolerates sub-perceptual
// rounding while its separate anti-aliasing detection absorbs rasterization edges.
const DEFAULT_THRESHOLD = 0.1;

// ---------------------------------------------------------------------------------------------
// CDP plumbing

async function launchBrowser(browserBinary) {
  const port = 9222 + Math.floor(Math.random() * 500);
  const chrome = spawn(
    browserBinary,
    [
      "--headless=new",
      `--remote-debugging-port=${port}`,
      "--no-first-run",
      "--no-default-browser-check",
      "--disable-gpu",
      "--force-color-profile=srgb",
      "--hide-scrollbars",
      "--font-render-hinting=none",
      "--window-size=1400,1000",
      "about:blank",
    ],
    { stdio: "ignore" },
  );

  let wsUrl;
  for (let i = 0; i < 100 && !wsUrl; i++) {
    await sleep(200);
    try {
      const targets = await (await fetch(`http://127.0.0.1:${port}/json`)).json();
      wsUrl = targets.find((t) => t.type === "page")?.webSocketDebuggerUrl;
    } catch {
      /* not up yet */
    }
  }
  if (!wsUrl) {
    chrome.kill();
    throw new Error(`${browserBinary} did not expose a debugging endpoint`);
  }

  const ws = new WebSocket(wsUrl);
  await new Promise((resolveOpen, reject) => {
    ws.onopen = resolveOpen;
    ws.onerror = reject;
  });

  let messageId = 0;
  const pending = new Map();
  const eventWaiters = [];
  ws.onmessage = (event) => {
    const message = JSON.parse(event.data);
    if (message.id && pending.has(message.id)) {
      pending.get(message.id)(message);
      pending.delete(message.id);
    } else if (message.method) {
      for (const waiter of [...eventWaiters]) {
        if (waiter.method === message.method) {
          eventWaiters.splice(eventWaiters.indexOf(waiter), 1);
          waiter.resolve(message.params);
        }
      }
    }
  };

  const send = (method, params = {}) =>
    new Promise((resolveSend, reject) => {
      const id = ++messageId;
      pending.set(id, (message) => {
        if (message.error) reject(new Error(`${method}: ${message.error.message}`));
        else resolveSend(message.result);
      });
      ws.send(JSON.stringify({ id, method, params }));
    });

  const waitForEvent = (method, timeoutMs) =>
    new Promise((resolveWait) => {
      const waiter = { method, resolve: resolveWait };
      eventWaiters.push(waiter);
      setTimeout(() => {
        const index = eventWaiters.indexOf(waiter);
        if (index >= 0) {
          eventWaiters.splice(index, 1);
          resolveWait(null);
        }
      }, timeoutMs);
    });

  const evaluate = async (expression) => {
    const result = await send("Runtime.evaluate", {
      expression,
      returnByValue: true,
      awaitPromise: true,
    });
    if (result.exceptionDetails) {
      throw new Error(`page evaluate failed: ${result.exceptionDetails.text}`);
    }
    return result.result?.value;
  };

  return { chrome, ws, send, waitForEvent, evaluate };
}

const sleep = (ms) => new Promise((resolveSleep) => setTimeout(resolveSleep, ms));

// ---------------------------------------------------------------------------------------------
// capture

function listPages(pagesDir) {
  const pages = readdirSync(pagesDir)
    .filter((name) => {
      const full = join(pagesDir, name);
      if (!statSync(full).isDirectory() || name === "Shared") return false;
      return readdirSync(full).some((f) => f.endsWith(".cshtml"));
    })
    .sort();
  return ["", ...pages]; // "" = Index
}

async function capture(baseUrl, outDir, pagesDir, browserBinary) {
  mkdirSync(outDir, { recursive: true });
  const { chrome, send, waitForEvent, evaluate } = await launchBrowser(browserBinary);
  const pages = listPages(pagesDir);
  let hadError = false;

  const screenshotTo = async (file) => {
    const screenshot = await send("Page.captureScreenshot", {
      format: "png",
      captureBeyondViewport: true,
    });
    writeFileSync(join(outDir, file), Buffer.from(screenshot.data, "base64"));
  };

  try {
    await send("Page.enable");
    await send("Network.enable");
    await send("Network.setBlockedURLs", { urls: BLOCKED_URLS });
    await send("Emulation.setEmulatedMedia", {
      features: [{ name: "prefers-reduced-motion", value: "reduce" }],
    });

    for (const page of pages) {
      const pageName = page === "" ? "Index" : page;
      const loaded = waitForEvent("Page.loadEventFired", 15000);
      await send("Page.navigate", { url: `${baseUrl}/${page}` });
      await loaded;

      // Freeze animations/transitions, then let fonts and layout settle.
      await evaluate(`(() => {
        const style = document.createElement("style");
        style.textContent = ${JSON.stringify(FREEZE_CSS)};
        document.head.appendChild(style);
      })()`);
      const fontsLoaded = await evaluate(
        `Promise.race([document.fonts.ready.then(() => true), new Promise((r) => setTimeout(() => r(false), 10000))])`,
      );
      if (!fontsLoaded) console.warn(`  warning: ${pageName}: fonts did not finish loading`);

      for (const viewport of VIEWPORTS) {
        await send("Emulation.setDeviceMetricsOverride", {
          width: viewport.width,
          height: viewport.height,
          deviceScaleFactor: 1,
          mobile: viewport.name === "mobile",
        });
        await sleep(250);
        await screenshotTo(`${pageName}.${viewport.name}.png`);
      }

      // Scenario screenshots (open states etc.) at desktop viewport.
      for (const scenario of SCENARIOS.filter((s) => s.page === pageName)) {
        await send("Emulation.setDeviceMetricsOverride", {
          width: VIEWPORTS[0].width,
          height: VIEWPORTS[0].height,
          deviceScaleFactor: 1,
          mobile: false,
        });
        const triggered = await evaluate(
          `(() => { const el = document.querySelector(${JSON.stringify(scenario.trigger)}); if (el) el.click(); return !!el; })()`,
        );
        if (!triggered) {
          console.warn(`  warning: ${pageName}__${scenario.name}: trigger not found`);
          continue;
        }
        await sleep(scenario.settleMs ?? 600);
        await screenshotTo(`${pageName}__${scenario.name}.desktop.png`);
      }

      console.log(`captured ${pageName}`);
    }
  } catch (error) {
    hadError = true;
    console.error(error);
  } finally {
    chrome.kill();
  }
  if (hadError) process.exit(2);
  console.log(`\n${pages.length} pages -> ${outDir}`);
}

// ---------------------------------------------------------------------------------------------
// compare

// Pads an image onto a white canvas so differently-sized captures stay comparable —
// the size delta itself then shows up as changed pixels along the grown edge.
function padTo(png, width, height) {
  if (png.width === width && png.height === height) return png;
  const padded = new PNG({ width, height });
  padded.data.fill(255);
  PNG.bitblt(png, padded, 0, 0, png.width, png.height, 0, 0);
  return padded;
}

function comparePair(baseFile, headFile, diffFile, threshold) {
  const base = PNG.sync.read(readFileSync(baseFile));
  const head = PNG.sync.read(readFileSync(headFile));

  const width = Math.max(base.width, head.width);
  const height = Math.max(base.height, head.height);
  const paddedBase = padTo(base, width, height);
  const paddedHead = padTo(head, width, height);

  // The diff image marks gated changes red and anti-aliasing-classified ones yellow.
  const diff = new PNG({ width, height });
  const changedPixels = pixelmatch(paddedBase.data, paddedHead.data, diff.data, width, height, {
    threshold,
  });
  // Second, count-only pass including AA-classified pixels: renders are deterministic per
  // environment, so any AA-only delta is a real (hairline) change, not rasterization noise.
  const allChangedPixels = pixelmatch(paddedBase.data, paddedHead.data, null, width, height, {
    threshold,
    includeAA: true,
  });
  const aaOnlyPixels = Math.max(0, allChangedPixels - changedPixels);

  if (changedPixels > 0 || aaOnlyPixels > 0) writeFileSync(diffFile, PNG.sync.write(diff));

  return {
    changedPixels,
    aaOnlyPixels,
    totalPixels: width * height,
    resized:
      base.width !== head.width || base.height !== head.height
        ? { base: [base.width, base.height], head: [head.width, head.height] }
        : null,
  };
}

// Percentage for display; a real diff must never print as "0".
function pctOf({ changedPixels, totalPixels }) {
  const pct = (changedPixels / totalPixels) * 100;
  return pct >= 0.01 ? pct.toFixed(2) : "<0.01";
}

function compare(baseDir, headDir, diffDir, threshold) {
  const baseFiles = readdirSync(baseDir).filter((f) => f.endsWith(".png")).sort();
  const headFiles = new Set(readdirSync(headDir).filter((f) => f.endsWith(".png")));
  mkdirSync(diffDir, { recursive: true });

  const changed = [];
  const removed = [];
  let comparedCount = 0;

  for (const file of baseFiles) {
    if (!headFiles.has(file)) {
      removed.push(file);
      continue;
    }
    headFiles.delete(file);
    comparedCount++;

    const result = comparePair(
      join(baseDir, file),
      join(headDir, file),
      join(diffDir, file.replace(/\.png$/, ".diff.png")),
      threshold,
    );
    if (result.changedPixels > 0 || result.aaOnlyPixels > 0) {
      changed.push({ file, ...result });
      console.log(
        `DIFF ${file}: ${result.changedPixels} pixels (${pctOf(result)}%), ${result.aaOnlyPixels} AA-only`,
      );
    }
  }
  const added = [...headFiles].sort();

  const summary = {
    comparedCount,
    changed: changed.map(({ file, changedPixels, aaOnlyPixels, totalPixels, resized }) => ({
      file,
      changedPixels,
      aaOnlyPixels,
      totalPixels,
      pct: pctOf({ changedPixels, totalPixels }),
      resized,
    })),
    added,
    removed,
  };
  writeFileSync(join(diffDir, "summary.json"), JSON.stringify(summary, null, 2) + "\n");

  const lines = [
    "# Visual regression report",
    "",
    `Base: ${baseDir}`,
    `Head: ${headDir}`,
    `Threshold: ${threshold} (AA-classified pixels counted separately as hairline changes)`,
    "",
  ];
  if (!changed.length && !added.length && !removed.length) {
    lines.push(`No differences across ${comparedCount} screenshots.`);
  } else {
    if (changed.length) {
      lines.push(`## Changed (${changed.length} of ${comparedCount} screenshots)`, "");
      lines.push("| Screenshot | Changed pixels | % of page | AA-only pixels | Size change |");
      lines.push("|---|---:|---:|---:|---|");
      for (const entry of summary.changed) {
        const size = entry.resized
          ? `${entry.resized.base.join("x")} -> ${entry.resized.head.join("x")}`
          : "";
        lines.push(
          `| ${entry.file} | ${entry.changedPixels} | ${entry.pct}% | ${entry.aaOnlyPixels} | ${size} |`,
        );
      }
      lines.push("");
    }
    if (added.length) lines.push(`## New screenshots (not in base)`, "", ...added.map((f) => `- ${f}`), "");
    if (removed.length) lines.push(`## Removed screenshots (missing in head)`, "", ...removed.map((f) => `- ${f}`), "");
  }
  const reportPath = join(diffDir, "report.md");
  writeFileSync(reportPath, lines.join("\n") + "\n");

  const diffCount = changed.length + added.length + removed.length;
  if (diffCount === 0) {
    console.log(`OK: ${comparedCount} screenshots compared, no differences`);
  } else {
    console.log(`\n${diffCount} screenshot(s) differ -> ${reportPath}`);
    process.exit(1);
  }
}

// ---------------------------------------------------------------------------------------------
// entry point

const [command, ...rest] = process.argv.slice(2);

function argValue(args, name, fallback) {
  const index = args.indexOf(name);
  return index >= 0 ? args[index + 1] : fallback;
}

const flagsWithValues = ["--url", "--out", "--pages", "--browser", "--threshold"];
function positionals(args) {
  const result = [];
  for (let i = 0; i < args.length; i++) {
    if (flagsWithValues.includes(args[i])) i++;
    else if (!args[i].startsWith("--")) result.push(args[i]);
  }
  return result;
}

if (command === "capture") {
  const url = argValue(rest, "--url", "http://localhost:5205").replace(/\/$/, "");
  const out = argValue(rest, "--out");
  if (!out) {
    console.error("capture requires --out <dir>");
    process.exit(2);
  }
  const browserBinary = argValue(rest, "--browser", process.env.CHROME_PATH || "chromium");
  await capture(url, resolve(out), resolve(argValue(rest, "--pages", defaultPagesDir)), browserBinary);
} else if (command === "compare") {
  const positional = positionals(rest);
  if (positional.length !== 2) {
    console.error("compare requires <baseDir> <headDir>");
    process.exit(2);
  }
  const baseDir = resolve(positional[0]);
  const headDir = resolve(positional[1]);
  const diffDir = resolve(argValue(rest, "--out", join(dirname(headDir), "diff")));
  compare(baseDir, headDir, diffDir, Number(argValue(rest, "--threshold", String(DEFAULT_THRESHOLD))));
} else {
  console.error(
    "usage: vrt.mjs capture --url <url> --out <dir> [--pages <PagesDir>] [--browser <path>] | compare <baseDir> <headDir> [--out <diffDir>] [--threshold n]",
  );
  process.exit(2);
}

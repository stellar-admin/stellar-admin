// Visual-regression tool for the DocsSamples site.
//
// Captures a per-element style snapshot (curated computed styles + bounding rect +
// ::before/::after/::placeholder) and a full-page screenshot for every sample page at two
// viewports, then diffs two capture runs property-by-property. The pass/fail signal is the
// style/rect data — screenshots are saved for human review only, since pixel comparison adds
// font-rasterization noise without adding precision.
//
// Elements are keyed by structural DOM path + data-slot, never by the class attribute: the
// tool exists to guard CSS refactors where every class attribute changes but the rendered
// result must not.
//
//   node util/visual-regression/vrt.mjs capture --url http://localhost:5205 --out snapshots/baseline
//   node util/visual-regression/vrt.mjs compare snapshots/baseline snapshots/after [--tolerance 0.5]
//
// The DocsSamples app must already be running. It now lives in the stellar-admin-pro repo, so
// point page discovery at its Pages folder with
// --pages ../stellar-admin-pro/docs/DocsSamples/Pages (defaults to this repo's old
// docs/DocsSamples/Pages location). Requires the
// system `chromium` binary and network access for the Geist webfont (load status is recorded
// in each snapshot's metadata).
//
// Determinism controls, applied identically to every run: animations/transitions/caret are
// disabled; external image hosts (unsplash, dicebear) are blocked; fonts are awaited; fixed
// chromium flags and viewport metrics.
//
// Known limits: hover/focus-visible states are not swept (headless chromium reports
// hover:none; force them ad hoc with --blink-settings=primaryHoverType=2,... and
// CSS.forcePseudoState). Only pages with a DocsSamples sample are covered.

import { spawn } from "node:child_process";
import { gzipSync, gunzipSync } from "node:zlib";
import { mkdirSync, readdirSync, readFileSync, statSync, writeFileSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

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

// Scenario snapshots: interactions performed after the initial snapshot, captured as
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

// The curated computed-style properties compared per element. Geometry is additionally
// covered exactly by the bounding rect, so this list targets paint/typography/stacking.
const STYLE_PROPS = [
  "display", "position", "top", "right", "bottom", "left", "z-index", "float",
  "overflow-x", "overflow-y", "visibility", "opacity", "box-sizing",
  "flex-direction", "flex-wrap", "flex-grow", "flex-shrink", "flex-basis",
  "grid-template-columns", "grid-template-rows", "grid-auto-flow",
  "grid-column-start", "grid-column-end", "grid-row-start", "grid-row-end",
  "align-items", "align-self", "align-content", "justify-content", "justify-items", "justify-self",
  "gap", "order", "place-items", "place-content",
  "margin-top", "margin-right", "margin-bottom", "margin-left",
  "padding-top", "padding-right", "padding-bottom", "padding-left",
  "width", "height", "min-width", "min-height", "max-width", "max-height",
  "border-top-width", "border-right-width", "border-bottom-width", "border-left-width",
  "border-top-style", "border-right-style", "border-bottom-style", "border-left-style",
  "border-top-color", "border-right-color", "border-bottom-color", "border-left-color",
  "border-top-left-radius", "border-top-right-radius",
  "border-bottom-left-radius", "border-bottom-right-radius",
  "outline-width", "outline-style", "outline-color", "outline-offset",
  "background-color", "background-image", "background-position", "background-size",
  "background-repeat", "background-clip", "background-origin",
  "color", "accent-color", "caret-color",
  "font-family", "font-size", "font-weight", "font-style", "font-stretch",
  "line-height", "letter-spacing", "word-spacing",
  "text-align", "text-decoration-line", "text-decoration-color", "text-decoration-style",
  "text-decoration-thickness", "text-underline-offset", "text-transform", "text-overflow",
  "text-wrap-mode", "text-wrap-style", "white-space-collapse", "vertical-align",
  "box-shadow", "text-shadow", "transform", "translate", "rotate", "scale",
  "filter", "backdrop-filter", "mix-blend-mode", "isolation",
  "cursor", "pointer-events", "user-select", "touch-action", "appearance",
  "object-fit", "object-position", "aspect-ratio",
  "list-style-type", "list-style-position", "border-collapse", "border-spacing", "table-layout",
  "content", "clip-path", "mask-image", "scrollbar-width", "color-scheme",
];

// ---------------------------------------------------------------------------------------------
// CDP plumbing

async function launchChromium() {
  const port = 9222 + Math.floor(Math.random() * 500);
  const chrome = spawn(
    "chromium",
    [
      "--headless=new",
      `--remote-debugging-port=${port}`,
      "--no-first-run",
      "--no-default-browser-check",
      "--disable-gpu",
      "--force-color-profile=srgb",
      "--hide-scrollbars",
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
    throw new Error("chromium did not expose a debugging endpoint");
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
// Snapshot script, executed in page context

const SNAPSHOT_FN = `(() => {
  const props = ${JSON.stringify(STYLE_PROPS)};

  const styleDict = new Map();
  const internStyles = (styleText) => {
    let id = styleDict.get(styleText);
    if (id === undefined) {
      id = styleDict.size;
      styleDict.set(styleText, id);
    }
    return id;
  };

  const styleText = (computed) => props.map((p) => computed.getPropertyValue(p)).join("|");

  const pathOf = (element) => {
    const segments = [];
    let node = element;
    while (node && node !== document.documentElement) {
      const tag = node.tagName.toLowerCase();
      let index = 1;
      let sibling = node.previousElementSibling;
      while (sibling) {
        if (sibling.tagName === node.tagName) index++;
        sibling = sibling.previousElementSibling;
      }
      segments.unshift(tag + (index > 1 ? ":" + index : ""));
      node = node.parentElement;
    }
    return segments.join(">");
  };

  const elements = [];
  for (const element of document.documentElement.querySelectorAll("*")) {
    if (["SCRIPT", "STYLE", "LINK", "META", "TITLE", "TEMPLATE"].includes(element.tagName)) continue;
    const rect = element.getBoundingClientRect();
    const entry = {
      p: pathOf(element),
      r: [rect.x, rect.y, rect.width, rect.height].map((v) => Math.round(v * 100) / 100),
      s: internStyles(styleText(getComputedStyle(element))),
    };
    const slot = element.getAttribute("data-slot");
    if (slot) entry.slot = slot;
    for (const pseudo of ["::before", "::after"]) {
      const computed = getComputedStyle(element, pseudo);
      if (computed.content !== "none" && computed.content !== "") {
        entry[pseudo === "::before" ? "b" : "a"] = internStyles(styleText(computed));
      }
    }
    if (element.matches("input, textarea")) {
      entry.ph = internStyles(styleText(getComputedStyle(element, "::placeholder")));
    }
    elements.push(entry);
  }

  return {
    elements,
    styles: [...styleDict.keys()],
    meta: {
      fontsLoaded: document.fonts.status === "loaded",
      elementCount: elements.length,
      title: document.title,
    },
  };
})()`;

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

async function capture(baseUrl, outDir, pagesDir) {
  mkdirSync(outDir, { recursive: true });
  const { chrome, send, waitForEvent, evaluate } = await launchChromium();
  const pages = listPages(pagesDir);
  let hadError = false;

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

        const snapshot = await evaluate(SNAPSHOT_FN);
        snapshot.meta.page = pageName;
        snapshot.meta.viewport = viewport.name;
        writeFileSync(
          join(outDir, `${pageName}.${viewport.name}.json.gz`),
          gzipSync(JSON.stringify(snapshot)),
        );

        const screenshot = await send("Page.captureScreenshot", {
          format: "png",
          captureBeyondViewport: true,
        });
        writeFileSync(
          join(outDir, `${pageName}.${viewport.name}.png`),
          Buffer.from(screenshot.data, "base64"),
        );
      }

      // Scenario snapshots (open states etc.) at desktop viewport.
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
        const snapshot = await evaluate(SNAPSHOT_FN);
        snapshot.meta.page = `${pageName}__${scenario.name}`;
        snapshot.meta.viewport = "desktop";
        writeFileSync(
          join(outDir, `${pageName}__${scenario.name}.desktop.json.gz`),
          gzipSync(JSON.stringify(snapshot)),
        );
        const screenshot = await send("Page.captureScreenshot", {
          format: "png",
          captureBeyondViewport: true,
        });
        writeFileSync(
          join(outDir, `${pageName}__${scenario.name}.desktop.png`),
          Buffer.from(screenshot.data, "base64"),
        );
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

function loadSnapshot(dir, file) {
  return JSON.parse(gunzipSync(readFileSync(join(dir, file))).toString());
}

function keyOf(entry) {
  return entry.slot ? `${entry.p} [data-slot=${entry.slot}]` : entry.p;
}

function diffStyles(props, beforeText, afterText) {
  const before = beforeText.split("|");
  const after = afterText.split("|");
  const changes = [];
  for (let i = 0; i < props.length; i++) {
    if (before[i] !== after[i]) changes.push(`${props[i]}: '${before[i]}' -> '${after[i]}'`);
  }
  return changes;
}

function compare(baselineDir, currentDir, tolerance) {
  const baselineFiles = readdirSync(baselineDir).filter((f) => f.endsWith(".json.gz")).sort();
  const currentFiles = new Set(readdirSync(currentDir).filter((f) => f.endsWith(".json.gz")));

  const report = [];
  let diffCount = 0;

  for (const file of baselineFiles) {
    if (!currentFiles.has(file)) {
      report.push(`## ${file}\n- MISSING in current run`);
      diffCount++;
      continue;
    }
    currentFiles.delete(file);

    const baseline = loadSnapshot(baselineDir, file);
    const current = loadSnapshot(currentDir, file);
    const currentByKey = new Map(current.elements.map((e) => [keyOf(e), e]));
    const lines = [];

    for (const baseEntry of baseline.elements) {
      const key = keyOf(baseEntry);
      const currentEntry = currentByKey.get(key);
      if (!currentEntry) {
        lines.push(`- element removed: ${key}`);
        continue;
      }
      currentByKey.delete(key);

      const rectDelta = baseEntry.r.map((v, i) => Math.abs(v - currentEntry.r[i]));
      if (rectDelta.some((d) => d > tolerance)) {
        lines.push(
          `- ${key}\n  rect: [${baseEntry.r.join(", ")}] -> [${currentEntry.r.join(", ")}]`,
        );
      }
      for (const [field, label] of [["s", ""], ["b", "::before "], ["a", "::after "], ["ph", "::placeholder "]]) {
        const baseStyle = baseEntry[field] !== undefined ? baseline.styles[baseEntry[field]] : null;
        const currentStyle =
          currentEntry[field] !== undefined ? current.styles[currentEntry[field]] : null;
        if (baseStyle === currentStyle) continue;
        if (baseStyle === null || currentStyle === null) {
          lines.push(`- ${key}\n  ${label}pseudo-element ${baseStyle === null ? "added" : "removed"}`);
          continue;
        }
        const changes = diffStyles(STYLE_PROPS, baseStyle, currentStyle);
        if (changes.length) {
          lines.push(`- ${key}\n  ${changes.map((c) => `${label}${c}`).join("\n  ")}`);
        }
      }
    }
    for (const key of currentByKey.keys()) lines.push(`- element added: ${key}`);

    if (lines.length) {
      report.push(`## ${file}\n${lines.join("\n")}`);
      diffCount += lines.length;
      console.log(`DIFF ${file}: ${lines.length} element(s)`);
    }
  }
  for (const file of currentFiles) {
    report.push(`## ${file}\n- NEW in current run (not in baseline)`);
    diffCount++;
  }

  const reportPath = join(currentDir, "report.md");
  writeFileSync(
    reportPath,
    `# Visual regression report\n\nBaseline: ${baselineDir}\nCurrent: ${currentDir}\nTolerance: ${tolerance}px\n\n` +
      (report.length ? report.join("\n\n") : "No differences.") +
      "\n",
  );

  if (diffCount === 0) {
    console.log(`OK: ${baselineFiles.length} snapshots compared, no differences`);
  } else {
    console.log(`\nFAIL: ${diffCount} difference(s) across ${report.length} snapshot(s) -> ${reportPath}`);
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

if (command === "capture") {
  const url = argValue(rest, "--url", "http://localhost:5205").replace(/\/$/, "");
  const out = argValue(rest, "--out");
  if (!out) {
    console.error("capture requires --out <dir>");
    process.exit(2);
  }
  await capture(url, resolve(out), resolve(argValue(rest, "--pages", defaultPagesDir)));
} else if (command === "compare") {
  const positional = rest.filter((a) => !a.startsWith("--") && a !== argValue(rest, "--tolerance"));
  if (positional.length !== 2) {
    console.error("compare requires <baselineDir> <currentDir>");
    process.exit(2);
  }
  compare(resolve(positional[0]), resolve(positional[1]), Number(argValue(rest, "--tolerance", "0")));
} else {
  console.error(
    "usage: vrt.mjs capture --url <url> --out <dir> [--pages <PagesDir>] | compare <baseline> <current> [--tolerance px]",
  );
  process.exit(2);
}

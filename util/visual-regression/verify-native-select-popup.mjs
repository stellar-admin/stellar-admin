// Desktop-only regression: headless CDP screenshots do not capture Wayland popup clipping.
import assert from "node:assert/strict";
import { execFileSync } from "node:child_process";
import { mkdtemp, mkdir, readFile, rm } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { PNG } from "pngjs";
import { launchBrowser, sleep } from "./browser.mjs";

const baseUrl = process.argv[2] || "http://localhost:5206";
const out = new URL("./snapshots/native-select-popup/", import.meta.url).pathname;
const profile = await mkdtemp(join(tmpdir(), "sa-select-popup-"));
await mkdir(out, { recursive: true });
const b = await launchBrowser(process.env.CHROME_PATH || "chromium", {
  headless: false,
  hideScrollbars: false,
  extraArgs: [`--user-data-dir=${profile}`],
});
try {
  for (const theme of ["observatory", "concourse"]) {
    for (const mode of ["light", "dark"]) {
      for (const offset of [100, 150]) {
        await b.send("Page.navigate", { url: `${baseUrl}/Field?theme=${theme}&mode=${mode}` });
        await sleep(600);
        const fontPath = process.env[`${theme.toUpperCase()}_FONT_CSS`];
        if (fontPath) {
          const css = await readFile(fontPath, "utf8");
          await b.evaluate(
            `{ const style = document.createElement('style'); style.textContent = ${JSON.stringify(css)}; document.head.append(style); }`,
          );
        }
        await b.evaluate(`(async () => {
          await document.fonts.load('13px "${theme === "observatory" ? "IBM Plex Sans" : "Source Sans 3"}"');
          await document.fonts.ready;
          const select = document.querySelector('#exp-year');
          select.scrollIntoView({block: 'start'});
          window.scrollBy(0, -${offset});
        })()`);
        const r = await b.evaluate(
          "document.querySelector('#exp-year').getBoundingClientRect().toJSON()",
        );
        for (const type of ["mousePressed", "mouseReleased"]) {
          await b.send("Input.dispatchMouseEvent", {
            type,
            x: r.x + 20,
            y: r.y + 15,
            button: "left",
            clickCount: 1,
          });
        }
        await sleep(250);
        const clients = JSON.parse(execFileSync("hyprctl", ["clients", "-j"]));
        const window = clients.find((c) => c.pid === b.chrome.pid);
        assert(window, "Cannot locate the isolated test browser window");
        const monitors = JSON.parse(execFileSync("hyprctl", ["monitors", "-j"]));
        assert.equal(
          monitors.find((m) => m.id === window.monitor)?.scale,
          1.6,
          "Run this regression on a display already configured at 1.6×; do not change user settings",
        );
        const path = join(out, `${theme}-${mode}-${offset}.png`);
        execFileSync("grim", [
          "-g",
          `${window.at[0]},${window.at[1]} ${window.size[0]}x${window.size[1]}`,
          path,
        ]);
        // Light native menus have a gray frame and blue selected row. Dark captures
        // require visual review because platform selection/frame colours can differ.
        if (mode === "light") {
          const result = edge(PNG.sync.read(await readFile(path)));
          assert(
            result.ratio > 0.9,
            `${theme} at offset ${offset}: incomplete popup bottom edge (${result.ratio})`,
          );
        }
        for (const [key, code] of [
          ["Escape", 27],
          ["ArrowDown", 40],
        ]) {
          await b.send("Input.dispatchKeyEvent", {
            type: "keyDown",
            key,
            windowsVirtualKeyCode: code,
          });
          await b.send("Input.dispatchKeyEvent", {
            type: "keyUp",
            key,
            windowsVirtualKeyCode: code,
          });
        }
        assert.equal(
          await b.evaluate("document.querySelector('#exp-year').value"),
          "2025",
          "Native keyboard selection must still work",
        );
        console.log(`${theme} ${mode} ${offset}: captured; keyboard selection passed`);
      }
    }
  }
} finally {
  b.ws.close();
  b.chrome.kill();
  // Chromium releases its temporary profile asynchronously; leave it if still busy.
  await sleep(300);
  await rm(profile, { recursive: true, force: true }).catch(() => {});
}

function edge(p) {
  const at = (x, y) => {
    const i = (y * p.width + x) * 4;
    return [...p.data.subarray(i, i + 3)];
  };
  const blue = (x, y) => {
    const [r, g, b] = at(x, y);
    return b > 150 && b - r > 70 && g - r > 25;
  };
  const gray = (x, y) => {
    const [r, g, b] = at(x, y);
    return Math.max(r, g, b) - Math.min(r, g, b) < 9 && r > 60 && r < 220;
  };
  let band;
  for (let y = 180; y < p.height && !band; y++)
    for (let x = 0; x < p.width; x++)
      if (blue(x, y)) {
        let end = x;
        while (end < p.width && blue(end, y)) end++;
        if (end - x > 60) {
          band = { x, end, y };
          break;
        }
        x = end;
      }
  if (!band) throw Error("No selected row");
  let best;
  for (let x = band.end - 2; x <= band.end + 4; x++) {
    let y = band.y + 5;
    while (y < p.height && gray(x, y)) y++;
    if (!best || y > best.y) best = { x, y };
  }
  let ratio = 0;
  for (let y = best.y - 4; y <= best.y; y++) {
    let hits = 0,
      total = 0;
    for (let x = band.x + 10; x < band.end - 10; x++) {
      hits += gray(x, y);
      total++;
    }
    ratio = Math.max(ratio, hits / total);
  }
  return { band, best, ratio };
}

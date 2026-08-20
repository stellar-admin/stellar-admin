// Self-test for vrt.mjs: proves the pixel pipeline actually fires on known, historically
// human-visible regressions.
//
// Standing rule: any visual bug that reaches human eyes without the tool firing becomes a
// permanent case in CASES below. Each case reverse-applies the fixing commit's diff to the
// working tree, rebuilds the library CSS bundles, re-captures, and asserts the expected
// pages are flagged; the patch is then re-applied. A final re-capture asserts a clean tree
// produces zero diffs (the determinism check).
//
//   node util/visual-regression/selftest.mjs [--url http://localhost:5206] [--browser <path>]
//
// Starts its own DocsSamples instance on the --url port (default 5206 to stay clear of a
// dev instance on 5205), so that port must be free. Requires dotnet, npm, and a git
// checkout (the case patches come from git history). Exits 0 on pass, 1 on fail.
// The working tree's src/StellarAdmin.TagHelpers files must not have uncommitted changes
// that conflict with the case patches.

import { spawn, execFileSync } from "node:child_process";
import { rmSync, existsSync, readFileSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const toolDir = dirname(fileURLToPath(import.meta.url));
const repoRoot = resolve(toolDir, "..", "..");
const snapshotsDir = join(toolDir, "snapshots", "selftest");

const CASES = [
  {
    // The lucide circle icon's stroke rendered as a visible gray ring around the radio
    // indicator; invisible to the tool's old curated-style comparison (stroke was not in
    // the property list), which is why the comparison is pixel-based now.
    name: "radio-stroke-ring",
    commit: "4a498ded88567882ffb9238f15d429bd2b2104f4",
    expect: [/^Radio\./],
  },
];

function argValue(args, name, fallback) {
  const index = args.indexOf(name);
  return index >= 0 ? args[index + 1] : fallback;
}
const args = process.argv.slice(2);
const url = argValue(args, "--url", "http://localhost:5206").replace(/\/$/, "");
const browser = argValue(args, "--browser", process.env.CHROME_PATH || "chromium");

const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

function git(...gitArgs) {
  return execFileSync("git", ["-C", repoRoot, ...gitArgs], { encoding: "utf8" });
}

function applyPatch(commit, reverse) {
  const patch = git("show", commit);
  execFileSync("git", ["-C", repoRoot, "apply", ...(reverse ? ["-R"] : []), "-"], {
    input: patch,
  });
}

function buildCss() {
  execFileSync("npm", ["run", "build:css"], {
    cwd: join(repoRoot, "src", "StellarAdmin.TagHelpers", "Client"),
    stdio: "inherit",
  });
}

// Runs vrt.mjs as a real CLI invocation so exit codes are exercised too.
function vrt(vrtArgs) {
  const result = spawn("node", [join(toolDir, "vrt.mjs"), ...vrtArgs], { stdio: "inherit" });
  return new Promise((resolveRun) => result.on("exit", resolveRun));
}

async function captureTo(name) {
  const out = join(snapshotsDir, name);
  rmSync(out, { recursive: true, force: true });
  const code = await vrt(["capture", "--url", url, "--out", out, "--browser", browser]);
  if (code !== 0) throw new Error(`capture ${name} failed (exit ${code})`);
  return out;
}

async function main() {
  // Start DocsSamples; its build also rebuilds the library CSS bundles when stale.
  console.log("starting DocsSamples ...");
  const app = spawn(
    "dotnet",
    ["run", "--project", join(repoRoot, "docs", "DocsSamples"), "--no-launch-profile"],
    {
      env: {
        ...process.env,
        ASPNETCORE_ENVIRONMENT: "Development",
        ASPNETCORE_URLS: url,
      },
      stdio: ["ignore", "inherit", "inherit"],
    },
  );
  let appExited = false;
  app.on("exit", () => {
    appExited = true;
  });

  try {
    let up = false;
    for (let i = 0; i < 240 && !up && !appExited; i++) {
      await sleep(1000);
      try {
        up = (await fetch(url, { redirect: "manual" })).status < 500;
      } catch {
        /* not up yet */
      }
    }
    if (!up) throw new Error("DocsSamples did not come up");

    const failures = [];
    const cleanDir = await captureTo("clean");

    for (const testCase of CASES) {
      console.log(`\n=== case: ${testCase.name} (reverse of ${testCase.commit.slice(0, 7)})`);
      applyPatch(testCase.commit, true);
      try {
        buildCss();
        const brokenDir = await captureTo(`broken-${testCase.name}`);
        const diffDir = join(snapshotsDir, `diff-${testCase.name}`);
        rmSync(diffDir, { recursive: true, force: true });
        const code = await vrt(["compare", cleanDir, brokenDir, "--out", diffDir]);

        if (code !== 1) {
          failures.push(`${testCase.name}: expected compare to exit 1, got ${code}`);
          continue;
        }
        const summary = JSON.parse(readFileSync(join(diffDir, "summary.json"), "utf8"));
        const changedFiles = summary.changed.map((c) => c.file);
        for (const expected of testCase.expect) {
          if (!changedFiles.some((f) => expected.test(f))) {
            failures.push(`${testCase.name}: no changed screenshot matches ${expected}`);
          }
        }
        console.log(`case ${testCase.name}: flagged ${changedFiles.length} screenshot(s)`);
      } finally {
        applyPatch(testCase.commit, false);
        buildCss();
      }
    }

    // Determinism check: a clean tree re-captured against the same app must produce zero diffs.
    console.log("\n=== determinism check");
    const recaptureDir = await captureTo("clean-again");
    const diffDir = join(snapshotsDir, "diff-determinism");
    rmSync(diffDir, { recursive: true, force: true });
    const code = await vrt(["compare", cleanDir, recaptureDir, "--out", diffDir]);
    if (code !== 0) failures.push(`determinism: expected zero diffs, compare exited ${code}`);

    if (failures.length) {
      console.error(`\nSELF-TEST FAILED:\n${failures.map((f) => `- ${f}`).join("\n")}`);
      process.exitCode = 1;
    } else {
      console.log("\nSELF-TEST PASSED");
    }
  } finally {
    app.kill();
  }
}

await main();

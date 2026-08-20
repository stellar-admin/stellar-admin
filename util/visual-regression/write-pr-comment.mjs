// Renders the sticky PR comment body for the Visual Regression workflow from a compare
// run's summary.json. Kept out of the workflow YAML so the format is versioned and
// testable alongside the tool.
//
//   node util/visual-regression/write-pr-comment.mjs --summary <summary.json> --run-url <url> [--artifact-url <url>] --out <comment.md>

import { existsSync, mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { dirname } from "node:path";

// The marker vrt-comment.yml uses to find and update the existing sticky comment.
export const STICKY_MARKER = "<!-- stellar-admin-vrt-report -->";

function argValue(args, name) {
  const index = args.indexOf(name);
  return index >= 0 ? args[index + 1] : undefined;
}

const args = process.argv.slice(2);
const summaryPath = argValue(args, "--summary");
const runUrl = argValue(args, "--run-url");
// Direct-download URL of the screenshots artifact; GitHub cannot deep-link individual
// files inside an artifact, so a one-click zip download is the closest thing.
const artifactUrl = argValue(args, "--artifact-url");
const outPath = argValue(args, "--out");
if (!summaryPath || !outPath) {
  console.error("usage: write-pr-comment.mjs --summary <summary.json> --run-url <url> --out <comment.md>");
  process.exit(2);
}

const lines = [STICKY_MARKER, "## Visual regression report", ""];

if (!existsSync(summaryPath)) {
  lines.push("Skipped: the merge-base has no `docs/DocsSamples`, so there is nothing to compare against.");
} else {
  const summary = JSON.parse(readFileSync(summaryPath, "utf8"));
  const diffCount = summary.changed.length + summary.added.length + summary.removed.length;

  if (diffCount === 0) {
    lines.push(`**Visual changes: none** (${summary.comparedCount} screenshots compared).`);
  } else {
    lines.push(
      `**Visual changes: ${summary.changed.length} of ${summary.comparedCount} screenshots changed**` +
        (summary.added.length ? `, ${summary.added.length} new` : "") +
        (summary.removed.length ? `, ${summary.removed.length} removed` : "") +
        ".",
      "",
      "This check is advisory — intentional visual changes are fine; the reviewer compares the",
      artifactUrl
        ? `base/head screenshot pairs and highlighted diffs in the [screenshots artifact](${artifactUrl}) (direct zip download; [run](${runUrl})).`
        : `base/head screenshot pairs and highlighted diffs in the [run's artifacts](${runUrl}).`,
      "",
    );
    if (summary.changed.length) {
      lines.push(
        "| Screenshot | Changed pixels | % of page | AA-only pixels | Size change |",
        "|---|---:|---:|---:|---|",
      );
      // Cap the table so a sweeping refactor doesn't produce an unreadable comment.
      const shown = summary.changed.slice(0, 50);
      for (const entry of shown) {
        const size = entry.resized
          ? `${entry.resized.base.join("x")} -> ${entry.resized.head.join("x")}`
          : "";
        lines.push(
          `| ${entry.file} | ${entry.changedPixels} | ${entry.pct}% | ${entry.aaOnlyPixels} | ${size} |`,
        );
      }
      if (summary.changed.length > shown.length) {
        lines.push("", `... and ${summary.changed.length - shown.length} more (see the run artifact).`);
      }
      lines.push("");
    }
    if (summary.added.length) {
      lines.push(`**New screenshots:** ${summary.added.map((f) => `\`${f}\``).join(", ")}`, "");
    }
    if (summary.removed.length) {
      lines.push(`**Removed screenshots:** ${summary.removed.map((f) => `\`${f}\``).join(", ")}`, "");
    }
  }
}

mkdirSync(dirname(outPath), { recursive: true });
writeFileSync(outPath, lines.join("\n") + "\n");
console.log(`wrote ${outPath}`);

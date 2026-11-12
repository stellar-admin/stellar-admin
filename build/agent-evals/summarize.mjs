// Tabulates the graded runs of a run root: per run, then the mean per variant.
// Usage: node summarize.mjs <run-root>    Prints Markdown.
import { existsSync, readFileSync } from "node:fs";
import { join } from "node:path";

const root = process.argv[2];
const read = (path) => (existsSync(path) ? readFileSync(path, "utf8") : null);
const json = (path) => {
  const text = read(path);
  try {
    return text ? JSON.parse(text) : null;
  } catch {
    return null;
  }
};
const points = { pass: 1, partial: 0.5, fail: 0 };

function summarize(id, variant) {
  const dir = join(root, id);
  const meta = json(join(dir, "meta.json"));
  if (!meta) return null;
  const events = (read(join(dir, "agent.jsonl")) ?? "").split("\n").filter(Boolean).map((line) => {
    try {
      return JSON.parse(line);
    } catch {
      return {};
    }
  });
  const result = events.findLast((event) => event.type === "result") ?? {};
  const toolCalls = events
    .filter((event) => event.type === "assistant")
    .flatMap((event) => event.message?.content ?? [])
    .filter((block) => block.type === "tool_use").length;
  const usage = result.usage ?? {};
  // Lines the agent added or changed, leaving out vendored client libraries
  const lines = (read(join(dir, "numstat.txt")) ?? "")
    .split("\n")
    .map((line) => line.split("\t"))
    .filter(([added, , path]) => path && added !== "-" && !path.startsWith("wwwroot/lib/"))
    .reduce((sum, [added]) => sum + Number(added), 0);
  const checks = json(join(dir, "grade", "checks.json"));
  const grade = json(join(dir, "grade", "grade.json"));
  const all = [...(checks?.checks ?? []), ...(grade?.checks ?? [])];
  const rubric = grade?.rubric ?? [];
  return {
    id,
    variant,
    built: checks ? checks.build : null,
    score: all.reduce((sum, check) => sum + (points[check.result] ?? 0), 0),
    of: checks?.build === false ? 5 + 13 : all.length,
    rubric: rubric.length ? rubric.reduce((sum, item) => sum + item.score, 0) / rubric.length : null,
    minutes: meta.seconds / 60,
    cost: result.total_cost_usd ?? null,
    input: (usage.input_tokens ?? 0) + (usage.cache_creation_input_tokens ?? 0) + (usage.cache_read_input_tokens ?? 0),
    output: usage.output_tokens ?? 0,
    turns: result.num_turns ?? null,
    toolCalls,
    lines,
    skills: meta.skills.trim(),
    failed: all.filter((check) => check.result !== "pass").map((check) => check.id).join(" "),
  };
}

const runs = (read(join(root, "runs.tsv")) ?? "")
  .split("\n")
  .filter(Boolean)
  .map((line) => line.split("\t"))
  .map(([id, variant]) => summarize(id, variant))
  .filter(Boolean);

const number = (value, digits = 0) => (value === null || Number.isNaN(value) ? "–" : value.toFixed(digits));
const thousands = (value) => `${Math.round(value / 1000)}k`;

console.log("| Run | Variant | Checks | Rubric | Minutes | Cost | Input | Output | Turns | Tool calls | Lines | Not passed |");
console.log("| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |");
for (const run of runs) {
  console.log(
    `| ${run.id} | ${run.variant} | ${run.score}/${run.of} | ${number(run.rubric, 1)} | ${number(run.minutes, 1)} | ${run.cost === null ? "–" : "$" + run.cost.toFixed(2)} | ${thousands(run.input)} | ${thousands(run.output)} | ${run.turns ?? "–"} | ${run.toolCalls} | ${run.lines} | ${run.failed}${run.skills ? " (skills: " + run.skills + ")" : ""} |`,
  );
}

console.log("\n| Variant | Runs | Checks | Rubric | Minutes | Cost | Input | Output | Tool calls | Lines |");
console.log("| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |");
const mean = (values) => (values.length ? values.reduce((sum, value) => sum + value, 0) / values.length : null);
for (const variant of ["razor", "sa-docs", "sa-skills"]) {
  const group = runs.filter((run) => run.variant === variant);
  if (!group.length) continue;
  const pick = (key) => group.map((run) => run[key]).filter((value) => value !== null);
  console.log(
    `| ${variant} | ${group.length} | ${number(mean(group.map((run) => run.score / run.of)) * 100)}% | ${number(mean(pick("rubric")), 1)} | ${number(mean(pick("minutes")), 1)} | $${number(mean(pick("cost")), 2)} | ${thousands(mean(pick("input")))} | ${thousands(mean(pick("output")))} | ${number(mean(pick("toolCalls")))} | ${number(mean(pick("lines")))} |`,
  );
}

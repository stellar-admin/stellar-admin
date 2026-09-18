import { test } from "node:test";
import assert from "node:assert/strict";
import { mkdtempSync, mkdirSync, writeFileSync, rmSync, symlinkSync } from "node:fs";
import { tmpdir } from "node:os";
import { dirname, join } from "node:path";
import { check } from "./check.mjs";

function fixture(run) {
  const root = mkdtempSync(join(tmpdir(), "consumer-skills-"));
  const write = (path, text) => {
    const file = join(root, path);
    mkdirSync(dirname(file), { recursive: true });
    writeFileSync(file, text);
  };
  write(
    "sample/SKILL.md",
    "---\nname: sample\ndescription: >-\n  A multiline\n  description.\n---\n# Sample\n[Guide](references/guide.md#setup-1)\n",
  );
  write("sample/LICENSE", "MIT License");
  write(
    "sample/references/guide.md",
    "# Setup\n# Setup\n[Home][home]\n\n[home]: ../SKILL.md#sample\n\n```md\n[Not a link](missing.md)\n# Not a heading\n```\n",
  );
  write("unfinished/references/notes.md", "[Not yet packaged](missing.md)");
  try {
    run({ root, write, verify: () => check(root) });
  } finally {
    rmSync(root, { recursive: true, force: true });
  }
}

test("valid bundle supports folded YAML, duplicate headings and reference links; ignores code and unfinished products", () => {
  fixture(({ verify }) => assert.deepEqual(verify(), { errors: [], skills: 1, documents: 2 }));
});

test("rejects malformed YAML and missing descriptions", () => {
  fixture(({ write, verify }) => {
    write("sample/SKILL.md", "---\nname: sample\nname: duplicate\n---\n");
    assert.match(verify().errors.join("\n"), /invalid YAML/);
    write("sample/SKILL.md", "---\nname: sample\ndescription: []\n---\n");
    assert.match(verify().errors.join("\n"), /description must be a nonempty string/);
  });
});

test("rejects missing entry metadata and mismatched names", () => {
  fixture(({ write, verify }) => {
    write("sample/SKILL.md", "# No frontmatter");
    assert.match(verify().errors.join("\n"), /missing YAML/);
    write("sample/SKILL.md", "---\nname: different\ndescription: Sample\n---\n");
    assert.match(verify().errors.join("\n"), /name must match/);
  });
});

test("rejects missing license and references", () => {
  fixture(({ root, verify }) => {
    rmSync(join(root, "sample/LICENSE"));
    rmSync(join(root, "sample/references"), { recursive: true });
    const errors = verify().errors.join("\n");
    assert.match(errors, /missing nonempty license/);
    assert.match(errors, /missing bundled Markdown references/);
  });
});

test("rejects missing link targets and heading anchors", () => {
  fixture(({ write, verify }) => {
    write(
      "sample/references/guide.md",
      "# Setup\n[Missing](no.md)\n[Anchor](../SKILL.md#absent)\n[Code heading](#not-a-heading)\n```\n# Not a heading\n```\n",
    );
    const errors = verify().errors.join("\n");
    assert.match(errors, /missing link target: no.md/);
    assert.match(errors, /missing heading anchor: ..\/SKILL.md#absent/);
    assert.match(errors, /missing heading anchor: #not-a-heading/);
  });
});

test("rejects cross-skill and encoded path escapes even when the target exists", () => {
  fixture(({ write, verify }) => {
    write(
      "sample/references/guide.md",
      "[External dependency](../../unfinished/references/notes.md)\n[Encoded](%2e%2e/%2e%2e/unfinished/references/notes.md)",
    );
    assert.equal(verify().errors.filter((error) => error.includes("link escapes")).length, 2);
  });
});

test("rejects symlinked bundle dependencies without walking directory cycles", () => {
  fixture(({ root, verify }) => {
    symlinkSync(join(root, "sample"), join(root, "sample/references/loop"));
    assert.match(verify().errors.join("\n"), /symlinks are not supported/);
  });
});

test("fails rather than succeeding when no skills are discovered", () => {
  fixture(({ root, verify }) => {
    rmSync(join(root, "sample/SKILL.md"));
    assert.match(verify().errors.join("\n"), /No installable consumer skills/);
  });
});

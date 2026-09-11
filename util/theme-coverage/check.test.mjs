import { test } from "node:test";
import assert from "node:assert/strict";
import { mkdtempSync, mkdirSync, writeFileSync, rmSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { check } from "./check.mjs";

function fixture(run) {
  const root = mkdtempSync(join(tmpdir(), "theme-coverage-"));
  const write = (path, text) => {
    const file = join(root, path);
    mkdirSync(join(file, ".."), { recursive: true });
    writeFileSync(file, text);
  };
  const manifest = {
    version: 1,
    themes: { ledger: { source: "custom" } },
    components: {
      Button: {
        tags: ["sa-button"],
        hooks: ["sa-button"],
        example: "example.html",
        themes: { ledger: { status: "reviewed", rules: ["sa-button"] } },
      },
    },
  };
  write(
    "src/StellarAdmin.TagHelpers/TagHelpers/Button/ButtonTagHelper.cs",
    '[HtmlTargetElement("sa-button")] class Button { string css = "sa-button"; }',
  );
  write(
    "src/StellarAdmin.TagHelpers/Client/css/themes/ledger.css",
    ".sa-button { border: 1px solid; }",
  );
  write(
    "src/StellarAdmin.TagHelpers/StellarAdmin.TagHelpers.csproj",
    '<ClientOutput Include="wwwroot/stellar-admin.ledger.css" />',
  );
  write("example.html", "<button>Example</button>");
  const verify = () => {
    write("util/theme-coverage/coverage.json", JSON.stringify(manifest));
    return check(root).errors;
  };
  try {
    run({ write, manifest, verify });
  } finally {
    rmSync(root, { recursive: true, force: true });
  }
}

test("reviewed theme coverage passes", () =>
  fixture(({ verify }) => assert.deepEqual(verify(), [])));
test("a new component requires explicit coverage", () =>
  fixture(({ write, verify }) => {
    write(
      "src/StellarAdmin.TagHelpers/TagHelpers/Calendar/CalendarTagHelper.cs",
      '[HtmlTargetElement("sa-calendar")] class Calendar {}',
    );
    assert.ok(verify().some((e) => e.includes("Calendar: component inventory changed")));
  }));
test("new subcomponents and variant hooks invalidate the review", () =>
  fixture(({ write, verify }) => {
    write(
      "src/StellarAdmin.TagHelpers/TagHelpers/Button/NewTagHelper.cs",
      '[HtmlTargetElement("sa-button-extra")] class Extra { string css = "sa-button-extra"; }',
    );
    const errors = verify();
    assert.ok(errors.some((e) => e.includes("tags changed")));
    assert.ok(errors.some((e) => e.includes("hooks changed")));
  }));
test("a new theme requires component coverage and output registration", () =>
  fixture(({ write, verify }) => {
    write("src/StellarAdmin.TagHelpers/Client/css/themes/new.css", ".sa-button {}");
    const errors = verify();
    assert.ok(errors.some((e) => e.includes("Theme inventory changed")));
    assert.ok(errors.some((e) => e.includes("missing ClientOutput")));
    assert.ok(errors.some((e) => e.includes("Button/new")));
  }));
test("pending support cannot silently ship", () =>
  fixture(({ manifest, verify }) => {
    manifest.components.Button.themes.ledger.status = "pending";
    assert.ok(verify().some((e) => e.includes("pending or undeclared")));
  }));
test("reviewed rule must exist as an exact selector", () =>
  fixture(({ write, verify }) => {
    write(
      "src/StellarAdmin.TagHelpers/Client/css/themes/ledger.css",
      ".sa-button-extra { color: red; }",
    );
    assert.ok(verify().some((e) => e.includes("missing theme rule .sa-button")));
  }));
test("shared-only needs an explanation", () =>
  fixture(({ manifest, verify }) => {
    manifest.components.Button.themes.ledger = { status: "shared-only" };
    assert.ok(verify().some((e) => e.includes("requires a rationale")));
    manifest.components.Button.themes.ledger.reason = "Uses the shared base.";
    assert.deepEqual(verify(), []);
  }));

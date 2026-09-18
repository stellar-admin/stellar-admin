import { existsSync, readdirSync, readFileSync, realpathSync, statSync } from "node:fs";
import { dirname, isAbsolute, join, relative, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import MarkdownIt from "markdown-it";
import GithubSlugger from "github-slugger";
import { parseDocument } from "yaml";

const markdown = new MarkdownIt();
const defaultRoot = resolve(dirname(fileURLToPath(import.meta.url)), "../../skills");
const inside = (root, path) => {
  const rel = relative(root, path);
  return rel !== ".." && !rel.startsWith("../") && !isAbsolute(rel);
};

function body(text) {
  return text.replace(/^\uFEFF/, "").replace(/^---\r?\n[\s\S]*?\r?\n---(?:\r?\n|$)/, "");
}

function anchors(text) {
  const slugger = new GithubSlugger();
  const tokens = markdown.parse(body(text), {});
  const result = new Set();
  for (let i = 0; i < tokens.length; i++) {
    if (tokens[i].type !== "heading_open") continue;
    const title = (tokens[i + 1].children ?? [])
      .map((token) => (["text", "code_inline", "image"].includes(token.type) ? token.content : ""))
      .join("");
    result.add(slugger.slug(title));
  }
  return result;
}

export function check(skillsRoot = defaultRoot) {
  const errors = [];
  let skills = 0;
  let documents = 0;
  const names = new Set();
  const fail = (file, message) => errors.push(`${relative(skillsRoot, file)}: ${message}`);
  for (const entry of readdirSync(skillsRoot, { withFileTypes: true })) {
    const directory = join(skillsRoot, entry.name);
    const entryPoint = join(directory, "SKILL.md");
    if (!entry.isDirectory() || !existsSync(entryPoint)) continue;
    skills++;
    const root = realpathSync(directory);
    const files = [];
    function walk(path) {
      if (!inside(root, realpathSync(path))) {
        fail(path, "symlink escapes the skill bundle");
        return;
      }
      const stats = statSync(path);
      if (stats.isDirectory()) {
        for (const child of readdirSync(path, { withFileTypes: true })) {
          // Directory symlinks can cycle and are not portable installation inputs.
          if (child.isSymbolicLink()) {
            fail(join(path, child.name), "symlinks are not supported in skill bundles");
          } else {
            walk(join(path, child.name));
          }
        }
      } else if (path.endsWith(".md")) files.push(path);
    }
    walk(directory);
    const text = readFileSync(entryPoint, "utf8").replace(/^\uFEFF/, "");
    const frontmatter = text.match(/^---\r?\n([\s\S]*?)\r?\n---(?:\r?\n|$)/);
    if (!frontmatter) fail(entryPoint, "missing YAML frontmatter");
    else {
      const yaml = parseDocument(frontmatter[1]);
      for (const error of yaml.errors) fail(entryPoint, `invalid YAML: ${error.message}`);
      if (!yaml.errors.length) {
        const metadata = yaml.toJSON();
        for (const key of ["name", "description"]) {
          if (typeof metadata?.[key] !== "string" || !metadata[key].trim()) {
            fail(entryPoint, `${key} must be a nonempty string`);
          }
        }
        if (typeof metadata?.name === "string") {
          if (!/^[a-z0-9]+(?:-[a-z0-9]+)*$/.test(metadata.name) || metadata.name.length > 64) {
            fail(
              entryPoint,
              "name must use lowercase letters, numbers and single hyphens (max 64 characters)",
            );
          }
          if (metadata.name !== entry.name) fail(entryPoint, "name must match the skill directory");
          if (names.has(metadata.name)) fail(entryPoint, "duplicate skill name");
          names.add(metadata.name);
        }
      }
    }
    const license = ["LICENSE", "LICENSE.md", "LICENSE.txt"].find((name) => {
      const path = join(directory, name);
      return existsSync(path) && statSync(path).isFile() && readFileSync(path, "utf8").trim();
    });
    if (!license) fail(entryPoint, "missing nonempty license");
    if (!files.some((file) => inside(join(directory, "references"), file))) {
      fail(entryPoint, "missing bundled Markdown references");
    }
    for (const file of files) {
      documents++;
      const tokens = markdown.parse(body(readFileSync(file, "utf8")), {});
      const visit = (tokens) => {
        for (const token of tokens) {
          const url =
            token.type === "link_open"
              ? token.attrGet("href")
              : token.type === "image"
                ? token.attrGet("src")
                : null;
          if (url !== null && !/^(?:[a-z][a-z0-9+.-]*:|\/\/)/i.test(url)) {
            try {
              const hash = url.indexOf("#");
              const path = decodeURIComponent((hash < 0 ? url : url.slice(0, hash)).split("?")[0]);
              const fragment = hash < 0 ? "" : decodeURIComponent(url.slice(hash + 1));
              const target = path ? resolve(dirname(file), path) : file;
              if (!inside(root, target)) fail(file, `link escapes the skill bundle: ${url}`);
              else if (!existsSync(target)) fail(file, `missing link target: ${url}`);
              else if (!inside(root, realpathSync(target)))
                fail(file, `link resolves outside the skill bundle: ${url}`);
              else if (
                fragment &&
                target.endsWith(".md") &&
                !anchors(readFileSync(target, "utf8")).has(fragment)
              ) {
                fail(file, `missing heading anchor: ${url}`);
              }
            } catch (error) {
              fail(file, `invalid local link ${url}: ${error.message}`);
            }
          }
          if (token.children) visit(token.children);
        }
      };
      visit(tokens);
    }
  }
  if (!skills) errors.push("No installable consumer skills found.");
  return { errors, skills, documents };
}

if (process.argv[1] && resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  const result = check();
  if (result.errors.length) {
    console.error(result.errors.join("\n"));
    process.exitCode = 1;
  } else
    console.log(
      `Validated ${result.skills} consumer skill(s), ${result.documents} Markdown files.`,
    );
}

# Consumer skill validation

Run from the product repository root:

```bash
npm ci --prefix build/skill-validation --ignore-scripts --no-audit --no-fund
npm test --prefix build/skill-validation
npm run check --prefix build/skill-validation
```

CI runs these commands on every pull request and push to master. Dependencies are pinned in `package-lock.json`; the checker performs no network requests. The separate SkillsGenerator drift check still verifies generated component references.

Each immediate directory under `skills/` containing `SKILL.md` is an installable bundle. Directories containing only future product references are skipped. The check fails if no installable skills are found.

Validation requires YAML frontmatter with a nonempty name and description, a lowercase hyphenated name matching the directory, a nonempty license, and bundled Markdown references. All Markdown files in each bundle are checked for local link/image targets and Markdown heading anchors, including reference-style links, same-file anchors and duplicate headings. Code examples are excluded. Local targets must stay inside the bundle; symlinks are rejected to keep installations self-contained and portable.

External URLs are not fetched. The checker validates Markdown headings rather than custom HTML anchors, and only Markdown links/images, not arbitrary paths in prose or executable examples. Installation and agent behavior remain manual checks when packaging or setup instructions change; this tool does not install skills or invoke agents.

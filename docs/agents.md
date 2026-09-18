# Working with coding agents

## Repository entry points

Start product sessions in this repository. `AGENTS.md` is the maintained entry point. Claude Code 2.1.277 or later reads it directly when no `CLAUDE.md` exists. This support is not yet available on Bedrock, Vertex, or Foundry, as noted in the [2.1.277 release notes](https://code.claude.com/docs/en/changelog#2-1-277). Website-only sessions start in the separate website repository, which has its own entry point. No workspace checkout is required for either repository's ordinary development.

Product conventions live in [conventions](conventions/), detailed guidance in [repo guides](repos/), and handoffs in [plans](plans/README.md). Follow the current task; historical plans and old repository paths describe prior work, not instructions to resume it.

## Skills

Development skills have one canonical copy in `.agents/skills/`. Claude Code discovers them through the relative directory symlinks at `.claude/skills/<name>`. Direct `AGENTS.md` support does not replace this skill discovery path. Keep their relative documentation and sibling-skill links intact. These developer workflows are separate from the product-specific consumer guidance under `skills/`. See the [consumer skills overview](../skills/README.md) for available and planned skills.

On systems without symlink checkout support, read the canonical folders directly or configure local discovery; do not hand-maintain two versions. Personal permissions and IDE settings are not imported from the former workspace.

## Handoffs and simultaneous work

Record agreed decisions, changed files, checks actually run, remaining work, and the next step in the relevant indexed plan. Private memory and agent chat history are supplementary, not required inputs for future contributors.

For simultaneous work, isolate each affected repository with its own worktree and coordinate generator destinations and ports. Cross-repo exports can use `STELLARADMIN_WEBSITE_DIR` to point at an isolated website checkout. Stop only your own processes and preserve other contributors' edits.

## Discovery check

From a fresh session in a standalone product checkout, ask: “Without changing files, list the project instructions you loaded, the development skills available, and where to find conventions, theme specifications, plans, and consumer-reference generation commands.” Confirm the root instructions, four development skills, and product-relative references are available without a parent workspace. Repeat in a website-only checkout to confirm its local entry point and verification commands.

Filesystem/link checks alone do not prove agent adherence. Report fresh-session validation separately and identify any unavailable client or authentication failure. Discovery references: [Codex instructions](https://learn.chatgpt.com/docs/agent-configuration/agents-md), [Codex skills](https://learn.chatgpt.com/docs/build-skills), [Claude memory](https://code.claude.com/docs/en/memory), and [Claude skills](https://code.claude.com/docs/en/skills).

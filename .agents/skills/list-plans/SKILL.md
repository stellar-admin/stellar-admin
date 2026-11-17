---
name: list-plans
description: List the outstanding (live) plans in docs/plans/, each with its title, status and a one-sentence description of what it is about. Use when a developer asks which plans are outstanding, open, proposed or in progress, or asks for an overview of the plans.
---

# List outstanding plans

Read-only. Do not edit plans, the index or anything else while listing.

## Which plans count

Outstanding plans are the Markdown files directly in `docs/plans/`, excluding `README.md`. Completed, superseded and closed plans live in `docs/plans/archive/` and `docs/plans/oss/archive/`; skip both.

## Steps

1. List `docs/plans/*.md` (not recursive) and drop `README.md`.
2. For each plan, read its opening: the `#` heading, the status line (`Status: …`, or `Current status: **…**` under a code audit heading) and the first paragraph or two after it.
3. Write one sentence describing what the plan is about, from the plan itself rather than the index. Describe the goal in plain words; leave out dates, history and file paths.
4. Compare with the live plans in `docs/plans/README.md`. Mention any plan file missing from the index, or any index entry without a file, after the list. Do not fix it unless asked.

## Output

Group by status in this order: in progress, open, proposed, backlog, deferred, then anything else. Within a group, keep the order of `docs/plans/README.md`.

```
**In progress**
- **Widgets** (`widgets.md`): Lets developers place widgets such as stats and charts on the home, index, view and form pages.

**Proposed**
- **App header breadcrumb** (`app-header-breadcrumb.md`): Shows a breadcrumb in the top bar in place of the brand title.
```

Keep it to the list. No preamble, no recommendations about which plan to pick up next.

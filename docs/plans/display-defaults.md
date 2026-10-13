# Display defaults and column handlers

Status: proposed (2026-10-13).

Two open items from the archived [info-lists](archive/info-lists.md) plan:

1. The default display for each type is hard-coded in `FieldDisplayDefaults`. Let applications configure it, possibly together with editor defaults and column displays (decision 90).
2. A display with a handler throws in an index column, since its handler would run once per row (decision 129). Support it, for example with a handler that prepares data for the whole page at once.

Research, design and implement as appropriate.

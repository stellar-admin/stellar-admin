# Culture-aware date formats

Status: proposed (2026-11-08).

## Problem

The default date display formats are fixed custom strings in `Displays/Date.cshtml` (both the display and grid display templates): `d MMM yyyy`, `HH:mm` and `d MMM yyyy, HH:mm`. Only the month name follows the current culture; the day-month-year order and the 24-hour clock do not, so en-US shows `14 Oct 2026, 09:30` and cultures such as ja-JP, hu-HU and de-DE get the wrong order or punctuation.

## Proposal

- Date: a medium date derived from the current culture's `LongDatePattern`, with the weekday (`dddd`) and its adjacent punctuation removed and `MMMM` replaced by `MMM`.
- Time: the standard short time (`t`).
- Date and time: the derived date and the short time.
- Check the derivation across all cultures in a scratch script before adopting it, and note ICU versus NLS differences.

## Follow-up

- Update the website docs to match, including the default formats in `content/docs/admin/number-and-date-displays.mdx`, and the `DateDisplay.Format` XML documentation.

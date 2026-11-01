# App header breadcrumb

Status: proposed (2026-11-01).

Show a breadcrumb in the app header (the top bar), in place of the brand title, which the sidebar already shows: `Products`, `Products › Waxed Canvas Daypack`, `Products › Waxed Canvas Daypack › Edit`, `Products › New product`. Split out of [page layout](page-layout.md)'s view, create and edit pass (Jerrie, 2026-11-01), because it isn't a simple change: the trail belongs to each page, but the app header is rendered by the outer layout (`_Layout.cshtml`), so every page has to hand the layout its trail.

- Today the app header shows `Branding.Title` (`_Layout.cshtml`), and the resource pages show no breadcrumb. The POC `sandbox/html/record-pages.html` renders the breadcrumb in the top bar, with a long trail clipping instead of widening the page.
- Every page that uses the layout needs a trail, or a fallback: the resource index, view, create and edit pages, the home page, and developers' own pages and Razor Pages that use the admin panel's layout.
- Open: how a page hands over its trail (view data, a section, a scoped service, a tag helper the layout reads), and what developers' pages get when they say nothing.
- Open: what the trail holds: the sidebar group (`Commerce`), the resource, the record's title (`UseTitle`), the page; which crumbs link; and how it shortens on a phone.
- Open: whether htmx swaps that change the page without a full load (boosted links) update the header's trail.
- Open: whether the brand title goes entirely or stays for pages without a trail.

To be filled out before implementing.

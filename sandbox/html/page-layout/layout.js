// Page layout prototype: the resource index page as locked in (sandbox only). Widgets are always real widget
// cards in the library's widget grid: Price summary and Top categories are the AdminPlayground's own markup, captured
// into the <template> elements on the page, and the chart stands in for any widget a developer writes. The layout
// only decides where the cards sit. The grid rows and filter pills are prototype data.

const ICONS = {
  search: '<path d="m21 21-4.34-4.34"/><circle cx="11" cy="11" r="8"/>',
  x: '<path d="M18 6 6 18"/><path d="m6 6 12 12"/>',
  plus: '<path d="M5 12h14"/><path d="M12 5v14"/>',
  filter: '<path d="M3 6h18"/><path d="M7 12h10"/><path d="M10 18h4"/>',
  tag: '<path d="M12.586 2.586A2 2 0 0 0 11.172 2H4a2 2 0 0 0-2 2v7.172a2 2 0 0 0 .586 1.414l8.704 8.704a2.426 2.426 0 0 0 3.42 0l6.58-6.58a2.426 2.426 0 0 0 0-3.42z"/><circle cx="7.5" cy="7.5" r=".5" fill="currentColor"/>',
  banknote: '<rect width="20" height="12" x="2" y="6" rx="2"/><circle cx="12" cy="12" r="2"/><path d="M6 12h.01M18 12h.01"/>',
  chevronRight: '<path d="m9 18 6-6-6-6"/>',
  chevronLeft: '<path d="m15 18-6-6 6-6"/>',
  more: '<circle cx="12" cy="12" r="1"/><circle cx="19" cy="12" r="1"/><circle cx="5" cy="12" r="1"/>',
  eye: '<path d="M2.062 12.348a1 1 0 0 1 0-.696 10.75 10.75 0 0 1 19.876 0 1 1 0 0 1 0 .696 10.75 10.75 0 0 1-19.876 0"/><circle cx="12" cy="12" r="3"/>',
  pencil: '<path d="M12 3H5a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.375 2.625a1 1 0 0 1 3 3l-9.013 9.014a2 2 0 0 1-.853.505l-2.873.84a.5.5 0 0 1-.62-.62l.84-2.873a2 2 0 0 1 .506-.852z"/>',
  trash: '<path d="M10 11v6"/><path d="M14 11v6"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"/><path d="M3 6h18"/><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/>',
  panelLeft: '<rect width="18" height="18" x="3" y="3" rx="2"/><path d="M9 3v18"/>',
  chevronDown: '<path d="m6 9 6 6 6-6"/>',
  check: '<path d="M20 6 9 17l-5-5"/>',
  calendar: '<path d="M8 2v4"/><path d="M16 2v4"/><rect width="18" height="18" x="3" y="4" rx="2"/><path d="M3 10h18"/>',
  box: '<path d="M21 8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16Z"/><path d="m3.3 7 8.7 5 8.7-5"/><path d="M12 22V12"/>',
  text: '<path d="M4 7V4h16v3"/><path d="M9 20h6"/><path d="M12 4v16"/>',
  download: '<path d="M12 15V3"/><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><path d="m7 10 5 5 5-5"/>',
};
const icon = (name, cls = "") => `<svg${cls ? ` class="${cls}"` : ""} xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">${ICONS[name]}</svg>`;
const esc = (s) => String(s).replace(/[&<>"]/g, (c) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;" })[c]);
const btn = (label, { variant = "default", size = "default", iconName, attrs = "", cls = "" } = {}) =>
  `<button type="button" data-slot="button" class="sa-button sa-button-variant-${variant} sa-button-size-${size}${cls ? " " + cls : ""}" ${attrs}>${iconName ? icon(iconName) : ""}${label}</button>`;
const tpl = (id) => document.getElementById(id).innerHTML;

let menuCount = 0;
function moreMenu(size = "icon") {
  const id = `pl-menu-${++menuCount}`;
  return `${btn("", { variant: "outline", size, iconName: "more", attrs: `popovertarget="${id}" aria-haspopup="menu" aria-label="More actions"` })}
    <div id="${id}" popover role="menu" data-slot="dropdown-menu-content" data-anchor-side="bottom" class="sa-dropdown-menu-content sa-dropdown-menu-content-logical anchored-bottom-end">
      <button type="button" role="menuitem" tabindex="-1" data-slot="dropdown-menu-item" data-variant="default" class="sa-dropdown-menu-item">${icon("download")}Export CSV</button>
    </div>`;
}

/* ---------- Prototype data ---------- */
const PRODUCTS = [
  [1, "STA-001", "Stationery", "A5 Ruled Notebook, 192 Pages", 8.99, "New, Bestseller"],
  [2, "TRV-014", "Travel accessories", "Packing Cubes, Set of 4", 24.5, "Bestseller"],
  [3, "TRV-022", "Travel accessories", "Universal Travel Adapter", 19.99, ""],
  [4, "BAG-003", "Bags", "Carry-on Duffel, 40 L", 64, "New"],
  [5, "STA-007", "Stationery", "0.5 mm Mechanical Pencil", 4.99, "Clearance"],
  [6, "GUI-101", "Guidebooks", "Kyoto in Four Seasons", 18.95, "Staff pick"],
  [7, "BAG-011", "Bags", "Waxed Canvas Daypack", 89.99, ""],
  [8, "TRV-031", "Travel accessories", "Memory Foam Neck Pillow", 22, "Bestseller"],
  [9, "GUI-117", "Guidebooks", "Walking Lisbon", 15.5, ""],
  [10, "STA-019", "Stationery", "Travel Journal, Linen Cover", 16.75, "New"],
];
const SCOPES_FEW = [["All products", 150], ["Under 50", 112], ["50 and over", 38]];
const SCOPES_MANY = [...SCOPES_FEW, ["New arrivals", 24], ["Clearance", 9], ["Out of stock", 6], ["Archived", 41]];
const scopes = () => (state.scopes === "many" ? SCOPES_MANY : state.scopes === "off" ? [] : SCOPES_FEW);

/* ---------- State, kept per viewer; the query string overrides it without saving: ?scopes=many&controls=hide ---------- */
const store = {
  get: (k) => { try { return localStorage.getItem("pl2:" + k); } catch { return null; } },
  set: (k, v) => { try { localStorage.setItem("pl2:" + k, v); } catch { /* private window */ } },
};
const query = new URLSearchParams(location.search);
const DEFAULTS = { widgets: "both", filters: "two", search: "on", scopes: "few", counts: "on", theme: "", mode: "light" };
const state = Object.fromEntries(Object.entries(DEFAULTS).map(([k, d]) => [k, query.get(k) ?? store.get(k) ?? d]));
// Values saved by earlier rounds of the prototype
if (state.scopes === "on") state.scopes = "few";
if (state.filters === "on") state.filters = "two";
const on = (k) => state[k] === "on";
const hasFilters = () => ["two", "many"].includes(state.filters);
const total = () => ({ two: 38, many: 31 })[state.filters] ?? 150;

/* ---------- Widgets: real widget cards, laid out by the library's widget grid ---------- */
const widgetGrid = (inner) => `<div class="sa-widgets" data-slot="widgets"><div class="sa-widgets-grid">${inner}</div></div>`;
const withSpan = (html, n) => html.replace('data-span="12"', `data-span="${n}"`);
// A chart, standing in for any widget a developer writes; the chart and Top categories show a column of widgets
function chartWidget(spanCount) {
  const values = [9, 14, 11, 18, 16, 22, 19, 26, 21, 24, 30, 27];
  const months = ["Nov", "Dec", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct"];
  const max = 32;
  const bars = values.map((v, i) => `<rect x="${i * 25 + 4}" y="${100 - (v / max) * 100}" width="17" height="${(v / max) * 100}" rx="2"/>`).join("");
  return `<div class="sa-widget" data-slot="widget" data-span="${spanCount}" data-state="loaded" data-widget="Products added">
    <div class="sa-card" data-slot="card">
      <div class="sa-card-header" data-slot="card-header"><div class="sa-card-title" data-slot="card-title">Products added</div><div class="sa-card-description" data-slot="card-description">Per month, last 12 months</div></div>
      <div class="sa-card-content" data-slot="card-content"><svg class="pl-chart" viewBox="0 0 300 100" preserveAspectRatio="none" role="img" aria-label="Products added per month">${bars}</svg>
        <div class="pl-chart-axis">${months.filter((_, i) => i % 3 === 0).map((m) => `<span>${m}</span>`).join("")}</div></div>
    </div></div>`;
}
const topWidgets = () => (["top", "both"].includes(state.widgets) ? widgetGrid(tpl("tpl-widget-price-summary")) : "");
const asideWidgets = () => (["aside", "both"].includes(state.widgets) ? widgetGrid(tpl("tpl-widget-top-categories") + chartWidget(12)) : "");
// The list with the aside widgets beside it, using the library's aside layout (below the list when narrow)
function withAside(list) {
  const aside = asideWidgets();
  if (!aside) return list;
  return `<div class="sa-widgets-aside"><div class="sa-widgets-aside-grid">${list}<div class="sa-widgets-column">${aside}</div></div></div>`;
}

/* ---------- The list's parts ---------- */
const count = (n, cls = "pl-tab-count") => (on("counts") ? `<span class="${cls}">${n}</span>` : "");
// The selected scope; choosing one re-renders the page, as the real page reloads
let selectedScope = 0;
// With `more`, every scope is both a tab and an item of a More tab's menu; fitScopeRows() shows each in one place
function scopeTabs(variant, { more = false } = {}) {
  if (!scopes().length) return "";
  const selected = Math.min(selectedScope, scopes().length - 1);
  const id = `pl-menu-${++menuCount}`;
  const moreTab = more
    ? `<button type="button" role="tab" aria-selected="false" data-slot="tabs-trigger" class="sa-tabs-trigger pl-scope-more" popovertarget="${id}" aria-haspopup="menu">More${icon("chevronDown")}</button>
      <div id="${id}" popover role="menu" data-slot="dropdown-menu-content" data-anchor-side="bottom" class="sa-dropdown-menu-content sa-dropdown-menu-content-logical anchored-bottom-start">${scopes().map(([s, n], i) =>
        `<a href="#" role="menuitem" tabindex="-1" data-scope="${i}" data-slot="dropdown-menu-item" data-variant="default" class="sa-dropdown-menu-item">${s}${on("counts") ? `<span class="pl-menu-count">${n}</span>` : ""}</a>`).join("")}</div>`
    : "";
  return `<div data-slot="tabs" data-orientation="horizontal" class="sa-tabs"><div class="sa-tabs-list sa-tabs-list-variant-${variant}" data-slot="tabs-list" data-variant="${variant}" role="tablist">${scopes().map(([s, n], i) =>
    `<a href="#" role="tab" data-scope="${i}" data-slot="tabs-trigger" aria-selected="${i === selected}"${i === selected ? ' data-active=""' : ""} class="sa-tabs-trigger">${s}${count(n)}</a>`).join("")}${moreTab}</div></div>`;
}

// The scope row keeps as many scopes as fit beside the search and moves the rest into More; the selected scope is
// always a tab. The card's width alone decides whether the search drops under the scopes (layout.css), so every page
// has the same arrangement at a given width
function fitScopeRow(row) {
  const tabs = [...row.querySelectorAll('[role="tab"][data-scope]')];
  const items = [...row.querySelectorAll('[role="menuitem"][data-scope]')];
  const more = row.querySelector(".pl-scope-more");
  const tools = row.querySelector(".pl-tools");
  const selected = tabs.findIndex((t) => t.getAttribute("aria-selected") === "true");
  tabs.forEach((t) => (t.hidden = false));
  more.hidden = false;
  const gap = parseFloat(getComputedStyle(row.querySelector(".sa-tabs-list")).columnGap) || 0;
  const widths = tabs.map((t) => t.offsetWidth);
  const rowStyle = getComputedStyle(row);
  const stacked = rowStyle.flexWrap === "wrap";
  const inner = row.clientWidth - parseFloat(rowStyle.paddingLeft) - parseFloat(rowStyle.paddingRight);
  const space = inner - (tools && !stacked ? tools.offsetWidth + parseFloat(rowStyle.columnGap) : 0);
  let shown = new Set(tabs.keys());
  if (widths.reduce((a, w, i) => a + w + (i ? gap : 0), 0) > space) {
    shown = new Set([selected]);
    let used = more.offsetWidth + gap + widths[selected];
    for (let i = 0; i < tabs.length; i++) {
      if (i === selected) continue;
      if (used + gap + widths[i] > space) break;
      used += gap + widths[i];
      shown.add(i);
    }
  }
  tabs.forEach((t, i) => (t.hidden = !shown.has(i)));
  items.forEach((t, i) => (t.hidden = shown.has(i)));
  more.hidden = shown.size === tabs.length;
}
const scopeRowObserver = new ResizeObserver((entries) => entries.forEach((e) => fitScopeRow(e.target.querySelector(".pl-scopes-row"))));
function fitScopeRows() {
  scopeRowObserver.disconnect();
  const rows = document.querySelectorAll(".pl-scopes-row");
  rows.forEach((row) => {
    fitScopeRow(row);
    scopeRowObserver.observe(row.closest(".pl-panel"));
  });
  // Measuring runs before the first paint; while the web font loads, the tabs stay hidden so they don't move after
  if (rows.length && document.fonts.status === "loading") {
    rows.forEach((row) => row.setAttribute("data-fitting", ""));
    document.fonts.ready.then(() => rows.forEach((row) => { fitScopeRow(row); row.removeAttribute("data-fitting"); }));
  }
}
function search() {
  if (!on("search")) return "";
  return `<div class="sa-input-group pl-search" role="group" data-slot="input-group">
    <input placeholder="Search products..." aria-label="Search products" name="search" data-slot="input-group-control" class="sa-input sa-input-group-input">
    <div role="group" data-slot="input-group-addon" data-align="start" class="sa-input-group-addon sa-input-group-addon-align-start">${icon("search")}</div>
  </div>`;
}
function chip(iconName, field, op, value) {
  return `<span class="sa-filter-chip" data-slot="filter-chip">
    <button type="button" class="sa-filter-chip-part sa-filter-chip-field">${icon(iconName)}${field}</button>
    <button type="button" class="sa-filter-chip-part sa-filter-chip-op">${op}</button>
    <button type="button" class="sa-filter-chip-part sa-filter-chip-value">${value}</button>
    <button type="button" class="sa-filter-chip-part sa-filter-chip-remove" aria-label="Remove ${field} filter">${icon("x")}</button>
  </span>`;
}
const chips = () => [
    chip("tag", "Category", "is any of", "Stationery, Bags"),
    chip("banknote", "Price", "at least", "$10"),
    ...(state.filters === "many"
      ? [
          chip("banknote", "Price", "at most", "$80"),
          chip("box", "Stock", "is", "In stock"),
          chip("text", "Name", "contains", "leather"),
          chip("tag", "Tags", "includes all of", "Handmade, Gift, Eco"),
          chip("calendar", "Added", "is after", "1 Sep 2026"),
        ]
      : []),
  ].join("");
// The filter bar as the filters worktree settled it: a Filter button when empty; pills, an add button and Clear
function filterBar() {
  if (state.filters === "hidden") return "";
  const items = hasFilters()
    ? `${chips()}${btn("", { variant: "ghost", size: "icon-sm", iconName: "plus", attrs: 'aria-label="Add filter"' })}${btn("Clear", { variant: "ghost", size: "sm" })}`
    : btn(`Filter<kbd class="sa-kbd">F</kbd>`, { variant: "outline", size: "sm", iconName: "filter", cls: "sa-filter-bar-trigger" });
  return `<div class="sa-filter-bar" data-slot="filter-bar">${items}</div>`;
}

function table() {
  const head = ["Sku", "Name", "Category", "Unit price", "Tags", ""];
  return `<div class="sa-table-container" data-slot="table-container"><table class="sa-table" data-slot="table">
    <thead class="sa-table-header" data-slot="table-header"><tr class="sa-table-row" data-slot="table-row">${head.map((h, i) => `<th class="sa-table-head${i === 3 ? " text-right" : ""}" data-slot="table-head">${h ? h : '<span class="sr-only">Actions</span>'}</th>`).join("")}</tr></thead>
    <tbody class="sa-table-body" data-slot="table-body">${PRODUCTS.map((p) => `<tr class="sa-table-row" data-slot="table-row">
      <td class="sa-table-cell" data-slot="table-cell">${p[1]}</td>
      <td class="sa-table-cell font-medium" data-slot="table-cell">${esc(p[3])}</td>
      <td class="sa-table-cell" data-slot="table-cell">${p[2]}</td>
      <td class="sa-table-cell text-right" data-slot="table-cell"><span class="sa-number-display">${p[4].toFixed(2)}</span></td>
      <td class="sa-table-cell" data-slot="table-cell">${p[5]}</td>
      <td class="sa-table-cell w-px whitespace-nowrap" data-slot="table-cell">${["eye", "pencil", "trash"].map((n, i) => btn("", { variant: "outline", size: "icon-sm", iconName: n, attrs: `aria-label="${["View", "Edit", "Delete"][i]} product"` })).join(" ")}</td></tr>`).join("")}</tbody></table></div>`;
}
function pager() {
  return `<span class="sa-data-grid-range" data-slot="data-grid-range"><span class="sa-data-grid-range-value">1–10</span> of ${total()}</span>
    <nav aria-label="pagination" class="sa-pagination" data-slot="data-grid-pager"><ul class="sa-pagination-content" data-slot="pagination-content">
      <li>${btn("", { variant: "ghost", size: "icon-sm", iconName: "chevronLeft", attrs: 'aria-label="Previous page" disabled' })}</li>
      <li>${btn("1", { variant: "outline", size: "icon-sm", attrs: 'aria-current="page"' })}</li><li>${btn("2", { variant: "ghost", size: "icon-sm" })}</li><li>${btn("3", { variant: "ghost", size: "icon-sm" })}</li><li>${btn("4", { variant: "ghost", size: "icon-sm" })}</li>
      <li>${btn("", { variant: "ghost", size: "icon-sm", iconName: "chevronRight", attrs: 'aria-label="Next page"' })}</li>
    </ul></nav>`;
}

/* ---------- A's panel head: the scopes as underline tabs, the filter bar and the search ---------- */
// The filter bar is one unit at the start of its row, so the Filter button turns into the pills in the same place;
// the search always sits at the card's right edge
const tools = () => (on("search") ? `<div class="pl-tools">${search()}</div>` : "");
// Locked in (Jerrie, 2026-10-31): the scopes on the left and the search on the right of one row, the filter bar on
// the row below. Without scopes, the filter bar and the search share one row
function panelHead() {
  const bar = filterBar();
  if (!scopes().length) return bar || on("search") ? `<div class="pl-panel-toolbar">${bar}${tools()}</div>` : "";
  return `<div class="pl-scopes-underline pl-scopes-row">${scopeTabs("line", { more: true })}${tools()}</div>${bar ? `<div class="pl-panel-toolbar">${bar}</div>` : ""}`;
}

/* ---------- Page chrome ---------- */
const sidebarTrigger = () => `<button type="button" data-slot="sidebar-trigger" aria-label="Toggle Sidebar" command="--toggle-sidebar" commandfor="${document.querySelector("sel-sidebar").id}" class="sa-button sa-button-variant-ghost sa-button-size-icon-sm">${icon("panelLeft", "size-4")}</button>
  <div data-slot="separator" class="sa-separator sa-separator-vertical sa-app-header-separator" role="separator" aria-orientation="vertical"></div>`;
const breadcrumb = () => `<nav aria-label="breadcrumb" data-slot="breadcrumb"><ol data-slot="breadcrumb-list" class="sa-breadcrumb-list">
  <li data-slot="breadcrumb-item" class="sa-breadcrumb-item"><a href="#" data-slot="breadcrumb-link" class="sa-breadcrumb-link">Commerce</a></li>
  <li role="presentation" aria-hidden="true" data-slot="breadcrumb-separator" class="sa-breadcrumb-separator">${icon("chevronRight")}</li>
  <li data-slot="breadcrumb-item" class="sa-breadcrumb-item"><span aria-current="page" data-slot="breadcrumb-page" class="sa-breadcrumb-page">Products</span></li></ol></nav>`;
const appHeader = () => `<header data-slot="app-header" class="sa-app-header">${sidebarTrigger()}${breadcrumb()}</header>`;
const pageHeader = () => `<header data-slot="page-header" class="sa-page-header">
    <h1 data-slot="page-header-title" class="sa-page-header-title">Products</h1>
    <p data-slot="page-header-description" class="sa-page-header-description">Everything Voyager Travel sells in its online shop.</p>
    <div data-slot="page-header-actions" class="sa-page-header-actions">${moreMenu()}${btn("Create product")}</div>
  </header>`;
const page = (inner) => `<div data-slot="page-container" data-width="full" class="sa-page-container sa-page-container-width-full pl-page">${inner}</div>`;

/* ---------- The page ---------- */
const LAYOUT = {
  note: "One card holds everything about the records: the scopes, the search, the filter bar, the table and the pager. The scopes are underline tabs with the search on the right of their row; the filter bar starts the row below, or shares the search's row when there are no scopes. Top widgets sit between the page header and the card; aside widgets line up with its top.",
  render: () => appHeader() + page(`
      ${pageHeader()}
      ${topWidgets()}
      ${withAside(`<section class="sa-data-grid sa-card sa-resource-index-grid pl-panel" data-slot="data-grid" aria-label="Products">
        ${panelHead()}
        ${table()}
        <div class="sa-card-footer sa-data-grid-footer" data-slot="card-footer">${pager()}</div>
      </section>`)}`),
};

/* ---------- Render and the prototype controls ---------- */
function render() {
  document.getElementById("proto-main").innerHTML = LAYOUT.render();
  fitScopeRows();
}
const group = (key, options, stack = false) => `<div class="group${stack ? " stack" : ""}">${options.map(([label, value]) => `<button type="button" data-key="${key}" data-value="${value}" aria-pressed="${state[key] === value}">${label}</button>`).join("")}</div>`;
function buildControls() {
  const panel = document.createElement("aside");
  panel.className = "pl-proto-controls";
  panel.innerHTML = `<header><span>Index page layout</span><button type="button" data-collapse>Hide</button></header>
    <p class="note">${LAYOUT.note}</p>
    <h3>Content</h3><div class="rows">
      <span>Scopes</span>${group("scopes", [["None", "off"], ["Three", "few"], ["Seven", "many"]])}
      <span>Counts</span>${group("counts", [["Off", "off"], ["On", "on"]])}
      <span>Search</span>${group("search", [["Off", "off"], ["On", "on"]])}
      <span>Filters</span>${group("filters", [["Off", "hidden"], ["None", "off"], ["Two", "two"], ["Seven", "many"]])}
      <span>Widgets</span>${group("widgets", [["None", "none"], ["Top", "top"], ["Aside", "aside"], ["Both", "both"]])}
      <span>Theme</span>${group("theme", [["Default", ""], ["Ops", "ops"], ["Soft", "soft"], ["Ledger", "ledger"]])}
      <span>Mode</span>${group("mode", [["Light", "light"], ["Dark", "dark"]])}
    </div>`;
  document.body.appendChild(panel);
  panel.addEventListener("click", (e) => {
    const b = e.target.closest("[data-key]");
    if (!b) return;
    state[b.dataset.key] = b.dataset.value;
    store.set(b.dataset.key, b.dataset.value);
    panel.querySelectorAll(`[data-key="${b.dataset.key}"]`).forEach((x) => x.setAttribute("aria-pressed", String(x === b)));
    if (b.dataset.key === "theme") applyTheme(); else if (b.dataset.key === "mode") applyMode(); else render();
  });
  panel.querySelector("[data-collapse]").addEventListener("click", (e) => {
    e.target.textContent = panel.toggleAttribute("data-collapsed") ? "Show" : "Hide";
  });
}
function applyTheme() {
  const link = document.getElementById("theme-css");
  if (state.theme) link.href = `../../util/theme-check/presets/${state.theme}.css`; else link.removeAttribute("href");
}
function applyMode() { document.documentElement.classList.toggle("dark", state.mode === "dark"); }

// Choosing a scope, from a tab or the More menu
document.getElementById("proto-main").addEventListener("click", (e) => {
  const scope = e.target.closest("[data-scope]");
  if (!scope) return;
  e.preventDefault();
  selectedScope = Number(scope.dataset.scope);
  render();
});
buildControls();
if (query.get("controls") === "hide") document.querySelector(".pl-proto-controls [data-collapse]").click();
applyTheme();
applyMode();
render();

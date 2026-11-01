// Record pages prototype: the resource index, view, edit and create pages, today's and the new layout (sandbox only).
// The fields, sections, cards, buttons and widgets use the markup the tag helpers emit, captured from the AdminPlayground
// (products, customers and form-mixed-editors, 2026-10-31). The product's Pricing and Inventory sections are prototype
// data, so that the pages have a long form to lay out; the Products resource itself has five fields.

const ICONS = {
  x: '<path d="M18 6 6 18"/><path d="m6 6 12 12"/>',
  more: '<circle cx="12" cy="12" r="1"/><circle cx="19" cy="12" r="1"/><circle cx="5" cy="12" r="1"/>',
  pencil: '<path d="M12 3H5a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.375 2.625a1 1 0 0 1 3 3l-9.013 9.014a2 2 0 0 1-.853.505l-2.873.84a.5.5 0 0 1-.62-.62l.84-2.873a2 2 0 0 1 .506-.852z"/>',
  trash: '<path d="M10 11v6"/><path d="M14 11v6"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"/><path d="M3 6h18"/><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/>',
  eye: '<path d="M2.062 12.348a1 1 0 0 1 0-.696 10.75 10.75 0 0 1 19.876 0 1 1 0 0 1 0 .696 10.75 10.75 0 0 1-19.876 0"/><circle cx="12" cy="12" r="3"/>',
  copy: '<rect width="14" height="14" x="8" y="8" rx="2"/><path d="M4 16c-1.1 0-2-.9-2-2V4c0-1.1.9-2 2-2h10c1.1 0 2 .9 2 2"/>',
  panelLeft: '<rect width="18" height="18" x="3" y="3" rx="2"/><path d="M9 3v18"/>',
  chevronRight: '<path d="m9 18 6-6-6-6"/>',
  chevronDown: '<path d="m6 9 6 6 6-6"/>',
  check: '<path d="M20 6 9 17l-5-5"/>',
  search: '<path d="m21 21-4.34-4.34"/><circle cx="11" cy="11" r="8"/>',
  minus: '<path d="M5 12h14"/>',
};
const icon = (name, cls = "") => `<svg${cls ? ` class="${cls}"` : ""} xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">${ICONS[name]}</svg>`;
const esc = (s) => String(s).replace(/[&<>"]/g, (c) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;" })[c]);
const btn = (label, { variant = "default", size = "default", iconName, attrs = "", cls = "", type = "button" } = {}) =>
  `<button type="${type}" data-slot="button" class="sa-button sa-button-variant-${variant} sa-button-size-${size}${cls ? " " + cls : ""}" ${attrs}>${iconName ? icon(iconName) : ""}${label}</button>`;
const link = (label, { variant = "default", size = "default", iconName, attrs = "", href = "#" } = {}) =>
  `<a href="${href}" data-slot="button" class="sa-button sa-button-variant-${variant} sa-button-size-${size}" ${attrs}>${iconName ? icon(iconName) : ""}${label}</a>`;
const tpl = (id) => document.getElementById(id).innerHTML;

let menuCount = 0;
function moreMenu(items) {
  const id = `rp-menu-${++menuCount}`;
  return `${btn("", { variant: "outline", size: "icon", iconName: "more", attrs: `popovertarget="${id}" aria-haspopup="menu" aria-label="More actions"` })}
    <div id="${id}" popover role="menu" data-slot="dropdown-menu-content" data-anchor-side="bottom" class="sa-dropdown-menu-content sa-dropdown-menu-content-logical anchored-bottom-end">
      ${items.map(([label, iconName, variant = "default"]) => label === "-"
        ? `<div role="separator" data-slot="dropdown-menu-separator" class="sa-dropdown-menu-separator"></div>`
        : `<button type="button" role="menuitem" tabindex="-1" data-slot="dropdown-menu-item" data-variant="${variant}" class="sa-dropdown-menu-item">${icon(iconName)}${label}</button>`).join("")}
    </div>`;
}

/* ---------- Prototype data: one product, its sections and fields ---------- */
const RECORD = { title: "Waxed Canvas Daypack", sku: "BAG-011", category: "Bags" };
const TAGS = ["Bestseller", "Clearance", "Eco-friendly", "New", "Staff pick", "Travel essentials"];
const SECTIONS = [
  {
    id: "product", title: "Product", description: "What customers see in the shop.", cols: 2,
    fields: [
      { id: "Name", label: "Name", type: "text", value: "Waxed Canvas Daypack" },
      { id: "CategoryId", label: "Category", type: "lookup", value: "Bags", media: "BA", detail: "Category #3" },
      { id: "Description", label: "Description", type: "textarea", span: 2, value: "A 22 L daypack in waxed cotton canvas with leather straps, a padded laptop sleeve and a water-resistant base. Made to age well on the road." },
      { id: "Tags", label: "Tags", type: "checkboxes", span: 2, value: ["Bestseller", "Eco-friendly"] },
    ],
  },
  {
    id: "pricing", title: "Pricing", description: "The price before tax, and the price it's compared with on sale.", cols: 2,
    fields: [
      { id: "Price", label: "Price", type: "number", value: "89.99" },
      { id: "CompareAtPrice", label: "Compare-at price", type: "number", value: "109.00", description: "Shown struck through while the product is on sale." },
      { id: "TaxClass", label: "Tax class", type: "select", value: "Standard", options: ["Standard", "Reduced", "Zero-rated"] },
      { id: "OnSale", label: "On sale", type: "switch", value: true },
    ],
  },
  {
    id: "inventory", title: "Inventory", description: "Stock levels and when the product can be ordered.", cols: 2,
    fields: [
      { id: "Sku", label: "Sku", type: "text", value: "BAG-011" },
      { id: "StockOnHand", label: "Stock on hand", type: "number", value: "38" },
      { id: "AvailableFrom", label: "Available from", type: "date", value: "2026-09-01", display: "1 Sep 2026" },
      { id: "TrackStock", label: "Track stock", type: "switch", value: true },
    ],
  },
];
const allFields = () => SECTIONS.flatMap((s) => s.fields);

/* ---------- State, kept per viewer; the query string overrides it without saving: ?page=edit&layout=b ---------- */
const store = {
  get: (k) => { try { return localStorage.getItem("rp:" + k); } catch { return null; } },
  set: (k, v) => { try { localStorage.setItem("rp:" + k, v); } catch { /* private window */ } },
};
const query = new URLSearchParams(location.search);
const DEFAULTS = { page: "view", layout: "new", rows: "25", sections: "on", widgets: "on", theme: "", mode: "light" };
const state = Object.fromEntries(Object.entries(DEFAULTS).map(([k, d]) => [k, query.get(k) ?? store.get(k) ?? d]));
// Layouts of the first round (A, B and C), which B replaced
if (!["current", "new"].includes(state.layout)) state.layout = "new";
const on = (k) => state[k] === "on";
const creating = () => state.page === "create";

/* ---------- Grids and fields, as the tag helpers emit them ---------- */
const cols = (n) => (n > 1 ? ` data-columns="multiple" style="--sa-cols-md:${n};--sa-cols-lg:${n}"` : ` data-columns="single"`);
const grid = (items, n = 1) => `<div data-slot="grid" class="sa-grid"><div class="sa-grid-content" data-slot="grid-content"${cols(n)}>${items.join("")}</div></div>`;
const gridItem = (html, span = 1) => `<div data-slot="grid-item" class="sa-grid-item"${span > 1 ? ` style="--sa-span-md:${span};--sa-span-lg:${span}"` : ""}>${html}</div>`;
const fieldGroup = (html) => `<div data-slot="field-group" class="sa-field-group">${html}</div>`;
const fieldError = (f) => `<div class="sa-field-error field-validation-valid" data-valmsg-for="Entity.${f.id}" data-valmsg-replace="true" role="alert" data-slot="field-error"></div>`;
const fieldDescription = (f) => (f.description ? `<p class="sa-field-description" data-slot="field-description">${esc(f.description)}</p>` : "");
const label = (f) => `<label class="sa-label sa-field-label" data-slot="field-label" for="Entity_${f.id}">${esc(f.label)}</label>`;
const vertical = (inner) => `<div class="sa-field sa-field-orientation-vertical" data-orientation="vertical" data-slot="field">${inner}</div>`;

function editor(f) {
  const v = creating() ? (f.type === "checkboxes" ? [] : f.type === "switch" ? false : "") : f.value;
  switch (f.type) {
    case "text":
    case "number":
    case "date":
      return vertical(`${label(f)}<input type="${f.type}"${f.type === "number" ? ' step="any"' : ""} id="Entity_${f.id}" name="Entity.${f.id}" value="${esc(v)}" data-slot="input" class="sa-input">${fieldError(f)}${fieldDescription(f)}`);
    case "textarea":
      return vertical(`${label(f)}<textarea rows="4" style="field-sizing: fixed" id="Entity_${f.id}" name="Entity.${f.id}" data-slot="textarea" class="sa-textarea">${esc(v)}</textarea>${fieldError(f)}`);
    case "select":
      return vertical(`${label(f)}<div data-slot="native-select-wrapper" data-size="default" class="sa-native-select-wrapper"><select data-slot="native-select" data-size="default" class="sa-native-select" id="Entity_${f.id}" name="Entity.${f.id}">${creating() ? '<option value="">Not specified</option>' : ""}${f.options.map((o) => `<option${o === v ? ' selected="selected"' : ""}>${o}</option>`).join("")}</select><svg class="sa-native-select-icon" aria-hidden="true" data-slot="native-select-icon" xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="m6 9 6 6 6-6"></path></svg></div>${fieldError(f)}`);
    case "switch":
      return `<div class="sa-field sa-field-orientation-horizontal" data-orientation="horizontal" data-slot="field"><span data-slot="switch" data-size="default" class="sa-switch-wrapper"><input type="checkbox"${v ? ' checked="checked"' : ""} id="Entity_${f.id}" name="Entity.${f.id}" value="true" role="switch" data-slot="switch-input" class="sa-switch"><span class="sa-switch-thumb" data-slot="switch-thumb"></span></span><div class="sa-field-content" data-slot="field-content">${label(f)}${fieldError(f)}</div></div>`;
    case "checkboxes":
      return `<div class="sa-field sa-field-orientation-vertical" data-orientation="vertical" data-slot="field"><label id="Entity_${f.id}-label" class="sa-label sa-field-label" data-slot="field-label">${esc(f.label)}</label><div data-slot="checkbox-group" role="group" class="sa-field-group" data-columns="multiple" style="--sa-cols-md:3;--sa-cols-lg:3" aria-labelledby="Entity_${f.id}-label">${TAGS.map((t, i) =>
        `<label class="sa-field-label" data-slot="field-label" for="Entity_${f.id}_${i}"><div class="sa-field sa-field-orientation-horizontal" data-orientation="horizontal" data-slot="field"><div class="sa-field-content" data-slot="field-content"><div class="sa-field-title" data-slot="field-label">${t}</div></div><span class="sa-input-control-wrapper"><input type="checkbox" name="Entity.${f.id}" id="Entity_${f.id}_${i}" value="${i}"${v.includes(t) ? ' checked="checked"' : ""} data-slot="input" class="sa-checkbox"><span class="sa-checkbox-indicator">${icon("check", "sa-checkbox-indicator-icon")}</span></span></div></label>`).join("")}</div>${fieldError(f)}</div>`;
    case "lookup":
      return `<div class="sa-field sa-field-orientation-vertical" data-orientation="vertical" data-slot="field">
    <label id="Entity_${f.id}-label" data-slot="field-label" class="sa-label sa-field-label">${esc(f.label)}</label>
    <fieldset class="contents" role="none"><admin-lookup-sheet-editor class="contents">
      <input type="hidden" data-lookup="value" id="Entity_${f.id}" name="Entity.${f.id}" value="${creating() ? "" : 3}" />
      <div data-lookup="selected" class="sa-item sa-item-size-xs sa-item-variant-outline flex-nowrap" role="group" aria-labelledby="Entity_${f.id}-label" data-slot="item" data-variant="outline" data-size="xs"${creating() ? ' hidden="hidden"' : ""}>
        <div data-lookup="media" data-slot="item-media" data-variant="default" class="sa-item-media sa-item-media-variant-default"><span class="sa-avatar sa-choice-media" data-media="avatar" data-placement="lookup-lg" data-slot="avatar" data-size="default"><span class="sa-avatar-fallback" data-slot="avatar-fallback">${f.media}</span></span></div>
        <div data-lookup="text" class="sa-item-content min-w-0 gap-0" data-slot="item-content"><div class="sa-item-title w-full text-sm" data-slot="item-title"><span class="truncate" data-lookup="title">${esc(f.value)}</span></div><p class="sa-item-description truncate text-xs" data-slot="item-description">${f.detail}</p></div>
        <div class="sa-item-actions gap-0.5" data-slot="item-actions">${btn(`Change<span class="sr-only"> ${f.label}</span>`, { variant: "ghost", size: "xs" })}${btn(`<span class="sr-only">Clear ${f.label}</span>`, { variant: "ghost", size: "icon-xs", iconName: "x" })}</div>
      </div>
      <div data-lookup="empty" class="flex gap-2"${creating() ? "" : ' hidden="hidden"'}>
        <div class="sa-input-group w-auto min-w-0 items-stretch overflow-hidden border-dashed p-0 text-sm h-12 text-muted-foreground flex-1" role="group" data-slot="input-group">
          <button type="button" class="hover:bg-muted/50 hover:text-foreground flex min-w-0 flex-1 items-center justify-center gap-2 px-3 outline-none transition-colors" data-slot="input-group-control" data-lookup="open">${icon("search", "size-4 shrink-0")}<span class="truncate">Choose category</span></button>
        </div>
      </div>
    </admin-lookup-sheet-editor></fieldset>${fieldError(f)}
</div>`;
  }
  return "";
}

// A display field's value, as the display templates render it
function displayValue(f) {
  const value = (inner, attrs = "") => `<div class="sa-display-field-value" data-slot="display-field-value"${attrs}>${inner}</div>`;
  switch (f.type) {
    case "number": return value(esc(f.value), ' data-kind="number"');
    case "date": return value(esc(f.display));
    case "select": return value(esc(f.value));
    case "switch": return value(`${icon(f.value ? "check" : "minus")}${f.value ? "Yes" : "No"}`, ` data-boolean="${f.value}"`);
    case "textarea": return value(esc(f.value), ' data-kind="text"');
    case "checkboxes": return value(f.value.map((t) => `<span data-slot="badge" class="sa-badge sa-badge-variant-secondary">${t}</span>`).join(""));
    case "lookup": return value(`<a href="#">${esc(f.value)}</a>`);
    default: return value(`<span class="sa-text-display" data-multiline="false">${esc(f.value)}</span>`);
  }
}
const displayField = (f, orientation = "vertical") =>
  `<div data-slot="display-field" data-orientation="${orientation}" class="sa-field sa-display-field"><div class="sa-display-field-label" data-slot="display-field-label">${esc(f.label)}</div>${displayValue(f)}</div>`;

// A form section, in one of the library's section layouts (split, stacked or card)
const formSection = (s, content, layout) => `<section data-slot="form-section" data-layout="${layout}" aria-labelledby="rp-section-${s.id}" id="section-${s.id}" class="sa-form-section"><div class="sa-form-section-inner"><div class="sa-form-section-header" data-slot="form-section-header"><h2 class="sa-form-section-title" data-slot="form-section-title" id="rp-section-${s.id}">${s.title}</h2>${s.description ? `<p class="sa-form-section-description" data-slot="form-section-description">${s.description}</p>` : ""}</div><div class="sa-form-section-content" data-slot="form-section-content">${content}</div></div></section>`;

// The fields of a section as a grid of editors or display fields
const sectionEditors = (s) => grid(s.fields.map((f) => gridItem(editor(f), s.cols > 1 ? f.span ?? 1 : 1)), s.cols);
const sectionDisplays = (s) => grid(s.fields.map((f) => gridItem(displayField(f), s.cols > 1 ? f.span ?? 1 : 1)), s.cols);

// Today's _FormFields: sections in the configured section layout, or a flat column of fields
function formFields(layout, fieldsOf) {
  if (!on("sections")) return fieldGroup(grid(allFields().map((f) => gridItem(fieldsOf === sectionDisplays ? displayField(f) : editor(f)))));
  return fieldGroup(grid(SECTIONS.map((s) => gridItem(formSection(s, fieldsOf(s), layout)))));
}

/* ---------- Page chrome ---------- */
const appHeader = () => `<header data-slot="app-header" class="sa-app-header">
    <button type="button" data-slot="sidebar-trigger" aria-label="Toggle Sidebar" command="--toggle-sidebar" commandfor="${document.querySelector("sel-sidebar").id}" class="sa-button sa-button-variant-ghost sa-button-size-icon-sm">${icon("panelLeft", "size-4")}</button>
    <div data-slot="separator" class="sa-separator sa-separator-vertical sa-app-header-separator" role="separator" aria-orientation="vertical" data-orientation="vertical"></div>
    ${state.layout === "new" ? breadcrumb(...crumbs()) : '<span class="text-sm font-medium">Voyager Travel</span>'}
  </header>`;
const page = (inner, { width = "large", cls = "", layout = "split" } = {}) =>
  `<div class="sa-page-container sa-page-container-width-${width} ${cls}" data-form-layout="${layout}" data-slot="page-container" data-width="${width}">${inner}</div>`;
const breadcrumb = (...items) => `<nav aria-label="breadcrumb" data-slot="breadcrumb"><ol data-slot="breadcrumb-list" class="sa-breadcrumb-list">${items.map((item, i) => i < items.length - 1
  ? `<li data-slot="breadcrumb-item" class="sa-breadcrumb-item"><a href="#" data-slot="breadcrumb-link" class="sa-breadcrumb-link">${item}</a></li><li role="presentation" aria-hidden="true" data-slot="breadcrumb-separator" class="sa-breadcrumb-separator">${icon("chevronRight")}</li>`
  : `<li data-slot="breadcrumb-item" class="sa-breadcrumb-item"><span aria-current="page" data-slot="breadcrumb-page" class="sa-breadcrumb-page">${item}</span></li>`).join("")}</ol></nav>`;
const pageHeader = ({ crumbs, title, description, actions, cls = "" }) => `<header class="sa-page-header ${cls}" data-slot="page-header">
    ${crumbs ? breadcrumb(...crumbs) : ""}
    <h1 data-slot="page-header-title" class="sa-page-header-title">${title}</h1>
    ${description ? `<p data-slot="page-header-description" class="sa-page-header-description">${description}</p>` : ""}
    ${actions ? `<div data-slot="page-header-actions" class="sa-page-header-actions">${actions}</div>` : ""}
  </header>`;

// Widgets: the page's real widget cards. The view and edit pages have the category's prices beside the fields; the
// create page has the category tip above the form, as the Products resource configures them
const widgetGrid = (inner) => `<div class="sa-widgets" data-slot="widgets"><div class="sa-widgets-grid">${inner}</div></div>`;
const topWidgets = () => (on("widgets") && creating() ? `<div id="form-page-widgets-top">${widgetGrid(`<div class="sa-widget" data-slot="widget" data-span="12" data-state="loaded" data-widget="Category tip">${tpl("tpl-alert-create-tip")}</div>`)}</div>` : "");
const withAside = (content, { sticky = !["view"].includes(state.page) } = {}) => (on("widgets") && !creating()
  ? `<div class="sa-widgets-aside"><div class="sa-widgets-aside-grid"><div>${content}</div><div class="sa-widgets-column"${sticky ? " data-sticky" : ""}>${widgetGrid(tpl("tpl-widget-category-prices"))}</div></div></div>`
  : content);

const deleteDialog = () => `<sel-dialog><dialog id="delete-confirm-dialog" data-slot="alert-dialog-content" data-size="default" closedby="closerequest" class="sa-alert-dialog-content">
    <div data-slot="alert-dialog-header" class="sa-alert-dialog-header">
      <h2 data-slot="alert-dialog-title" class="sa-alert-dialog-title">Delete Product</h2>
      <p data-slot="alert-dialog-description" class="sa-alert-dialog-description">Are you sure you want to delete this Product?</p>
    </div>
    <div data-slot="alert-dialog-footer" class="sa-alert-dialog-footer"><form method="dialog">
      <button type="submit" value="cancel" data-slot="alert-dialog-cancel" class="sa-button sa-button-variant-outline sa-button-size-default sa-alert-dialog-cancel">Cancel</button>
      <button type="submit" value="confirm" data-slot="alert-dialog-action" class="sa-button sa-button-variant-destructive sa-button-size-default sa-alert-dialog-action">Delete Product</button>
    </form></div>
  </dialog></sel-dialog>`;
const deleteAttrs = 'command="show-modal" commandfor="delete-confirm-dialog"';

const submitLabel = () => (creating() ? "Add Product" : "Save Product");
const formTitle = () => (creating() ? "New product" : RECORD.title);
const crumbs = () => ({ index: ["Products"], create: ["Products", "New product"], edit: ["Products", RECORD.title, "Edit"] })[state.page] ?? ["Products", RECORD.title];
const recordMeta = () => `${RECORD.sku} <span aria-hidden="true">·</span> ${RECORD.category}`;
// The new layout puts the breadcrumb in the app header, in place of the brand title, which the sidebar shows


// The index page: the products index's list card as the admin panel renders it, between its widgets
const indexWidgets = (id) => (on("widgets") ? widgetGrid(tpl(id)) : "");
const indexPage = (header) => page(`${header}${on("widgets") ? `<div id="index-page-widgets-top">${indexWidgets("tpl-widget-price-summary")}</div>` : ""}${on("widgets")
  ? `<div class="sa-widgets-aside"><div class="sa-widgets-aside-grid">${tpl("tpl-index-list")}<div id="index-page-widgets-aside" class="sa-widgets-column">${indexWidgets("tpl-widget-top-categories")}</div></div></div>`
  : tpl("tpl-index-list")}`, { width: "full" });

/* ---------- The layouts, per page: today's, and the new one ---------- */
const LAYOUTS = {
  index: {
    current: {
      name: "Today",
      note: "Today's index page, as committed: the title and Create in the page header, the price summary above the list card and the top categories beside it.",
      render: () => indexPage(pageHeader({ title: "Products", actions: link("Create") })),
    },
    new: {
      name: "New",
      note: "<b>The same sticky header as the record pages.</b> Products and Create stay in view while the list scrolls, and the breadcrumb is in the top bar. The list card's scopes and search scroll with the list.",
      render: () => indexPage(pageHeader({ title: "Products", actions: link("Create"), cls: "rp-sticky-header" })),
    },
  },
  view: {
    current: {
      name: "Today",
      note: "Today's view page: a generic title (View Product), Back, a solid destructive Delete and Edit as three equal buttons, and the sections in the split layout under a ruled header.",
      render: () => page(`${pageHeader({
        title: "View Product", cls: "sa-resource-view-header",
        actions: `${link("Back", { variant: "outline", attrs: 'data-slot="view-back"' })}${btn("Delete Product", { variant: "destructive", attrs: deleteAttrs })}${link("Edit Product")}`,
      })}${withAside(formFields("split", sectionDisplays))}`, { cls: "sa-resource-view" }) + deleteDialog(),
    },
    new: {
      name: "New",
      note: "<b>Only the header changes.</b> The record is the title, with its sku and category under it, and a breadcrumb instead of Back. The header sticks as the page scrolls, so Edit is always in the same place and in reach. The fields keep today's layout.",
      render: () => page(`${pageHeader({
        title: RECORD.title, description: recordMeta(), cls: "rp-sticky-header",
        actions: `${btn("Delete", { variant: "outline", attrs: deleteAttrs, cls: "rp-outline-danger" })}${link("Edit", { iconName: "pencil" })}`,
      })}${withAside(formFields("split", sectionDisplays))}`, { cls: "sa-resource-view" }) + deleteDialog(),
    },
  },
  edit: {
    current: {
      name: "Today",
      note: "Today's edit page: a generic title (Edit Product) with no way back but Cancel, the sections in the split layout, and Delete, Cancel and Save at the bottom, below every section.",
      render: () => currentForm(),
    },
    new: {
      name: "New",
      note: "<b>Only the header changes.</b> Cancel and Save move from the bottom of the form to the sticky header, in the same place as Edit and Create, so a long form can be saved from anywhere. Delete and View are in the menu, away from Save. The fields keep today's layout.",
      render: () => formPage(),
    },
  },
  create: {
    current: {
      name: "Today",
      note: "Today's create page: the title (Add new Product), the tip widget, the sections in the split layout and Cancel and Add at the bottom.",
      render: () => currentForm(),
    },
    new: {
      name: "New",
      note: "<b>The edit page's header.</b> Add sits where Create is on the index page, in the sticky header. The tip widget and the fields keep today's layout.",
      render: () => formPage(),
    },
  },
};

function currentForm() {
  const footer = `<div class="sa-resource-form-footer">${creating() ? "" : btn("Delete Product", { variant: "destructive", attrs: deleteAttrs })}<div class="sa-resource-form-actions">${link("Cancel", { variant: "outline", attrs: 'data-slot="form-cancel"' })}${btn(submitLabel(), { type: "submit", attrs: 'data-form-submit aria-keyshortcuts="Control+Enter Meta+Enter"' })}</div></div>`;
  return page(`${pageHeader({ title: creating() ? "Add new Product" : "Edit Product", cls: "sa-resource-form-header" })}${topWidgets()}${withAside(
    `<form method="post" data-resource-form>${fieldGroup(`${formFields("split", sectionEditors)}${footer}`)}</form>`)}`, { cls: "sa-resource-form" }) + (creating() ? "" : deleteDialog());
}

function formPage() {
  return page(`${pageHeader({
    title: formTitle(), description: creating() ? "Add a product to the shop." : recordMeta(), cls: "rp-sticky-header",
    actions: `${creating() ? "" : moreMenu([["View product", "eye"], ["-"], ["Delete product", "trash", "destructive"]])}${link("Cancel", { variant: "outline", attrs: 'data-slot="form-cancel"' })}${btn(submitLabel(), { attrs: 'form="rp-form" type="submit" data-form-submit aria-keyshortcuts="Control+Enter Meta+Enter"' })}`,
  })}${topWidgets()}${withAside(`<form method="post" id="rp-form" data-resource-form>${fieldGroup(formFields("split", sectionEditors))}</form>`)}`,
  { cls: "sa-resource-form" }) + (creating() ? "" : deleteDialog());
}

/* ---------- Render and the prototype controls ---------- */
const current = () => LAYOUTS[state.page][state.layout];
function render() {
  const main = document.getElementById("proto-main");
  main.innerHTML = appHeader() + current().render();
  document.querySelector(".rp-proto-controls .note").innerHTML = current().note;
  if (state.page === "index") showRows(main);
  const header = main.querySelector(".rp-sticky-header");
  if (header) new ResizeObserver(() => main.style.setProperty("--rp-header-h", `${header.offsetHeight}px`)).observe(header);
}

// The list card holds 25 rows; with 10, the rest go and the pager says so
function showRows(main) {
  const n = Number(state.rows);
  main.querySelectorAll(".sa-table-body > .sa-table-row").forEach((row, i) => {
    if (i < n) return;
    if (row.nextElementSibling?.matches(".sa-table-row-detail")) row.nextElementSibling.remove();
    row.remove();
  });
  const range = main.querySelector(".sa-data-grid-range-value");
  if (range) range.textContent = `1–${n}`;
  main.querySelectorAll('.sa-data-grid-page-size [data-slot="tabs-trigger"]').forEach((t) => {
    const selected = t.textContent.trim() === String(n);
    t.setAttribute("aria-selected", String(selected));
    t.toggleAttribute("data-active", selected);
  });
}

const group = (key, options) => `<div class="group">${options.map(([text, value]) => `<button type="button" data-key="${key}" data-value="${value}" aria-pressed="${state[key] === value}">${text}</button>`).join("")}</div>`;
function buildControls() {
  const panel = document.createElement("aside");
  panel.className = "rp-proto-controls";
  panel.innerHTML = `<header><span>Record pages</span><button type="button" data-collapse>Hide</button></header>
    <div class="rows">
      <span>Page</span>${group("page", [["Index", "index"], ["View", "view"], ["Edit", "edit"], ["Create", "create"]])}
      <span>Layout</span>${group("layout", [["Today", "current"], ["New", "new"]])}
      <span>Index rows</span>${group("rows", [["10", "10"], ["25", "25"]])}
    </div>
    <p class="note"></p>
    <h3>Content</h3><div class="rows">
      <span>Sections</span>${group("sections", [["Three", "on"], ["None", "off"]])}
      <span>Widgets</span>${group("widgets", [["On", "on"], ["Off", "off"]])}
      <span>Theme</span>${group("theme", [["Default", ""], ["Ops", "ops"], ["Soft", "soft"], ["Ledger", "ledger"]])}
      <span>Mode</span>${group("mode", [["Light", "light"], ["Dark", "dark"]])}
    </div>
    <p class="note keys">Keys: <b>1–4</b> page, <b>0</b> today, <b>N</b> new.</p>`;
  document.body.appendChild(panel);
  panel.addEventListener("click", (e) => {
    const b = e.target.closest("[data-key]");
    if (b) set(b.dataset.key, b.dataset.value);
  });
  panel.querySelector("[data-collapse]").addEventListener("click", (e) => {
    e.target.textContent = panel.toggleAttribute("data-collapsed") ? "Show" : "Hide";
  });
}
function set(key, value) {
  state[key] = value;
  store.set(key, value);
  document.querySelectorAll(`.rp-proto-controls [data-key="${key}"]`).forEach((x) => x.setAttribute("aria-pressed", String(x.dataset.value === value)));
  if (key === "theme") applyTheme(); else if (key === "mode") applyMode(); else { render(); window.scrollTo(0, 0); }
}
function applyTheme() {
  const css = document.getElementById("theme-css");
  if (state.theme) css.href = `../../util/theme-check/presets/${state.theme}.css`; else css.removeAttribute("href");
}
function applyMode() { document.documentElement.classList.toggle("dark", state.mode === "dark"); }

// Keyboard: 1–4 pick the page, 0 today's layout, N the new one (outside fields)
document.addEventListener("keydown", (e) => {
  if (e.ctrlKey || e.metaKey || e.altKey || e.target.closest("input, textarea, select, [contenteditable]")) return;
  const pages = { 1: "index", 2: "view", 3: "edit", 4: "create" };
  const layouts = { 0: "current", n: "new" };
  const k = e.key.toLowerCase();
  if (pages[k]) set("page", pages[k]);
  else if (layouts[k]) set("layout", layouts[k]);
});

const main = document.getElementById("proto-main");
main.addEventListener("click", (e) => {
  if (e.target.closest('[type="submit"], [data-slot="form-cancel"], .sa-breadcrumb-link, .sa-resource-index-list a')) e.preventDefault();
});
main.addEventListener("submit", (e) => e.preventDefault());

buildControls();
if (query.get("controls") === "hide") document.querySelector(".rp-proto-controls [data-collapse]").click();
applyTheme();
applyMode();
render();

// Fills page.template.html's placeholders with the markup the tag helpers emit and writes ../rich-text-editor.html.
import { readFileSync, writeFileSync } from "node:fs";

const icons = JSON.parse(readFileSync("../../../src/StellarAdmin.UI/Icons/Definitions/Lucide.json", "utf8").replace(/^﻿/, ""));

function icon(name, className = "") {
  const children = icons[name]
    .map(([tag, attrs]) => `<${tag} ${Object.entries(attrs).map(([k, v]) => `${k}="${v}"`).join(" ")}></${tag}>`)
    .join("");
  const cls = className ? `class="${className}" ` : "";
  return `<svg ${cls}xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">${children}</svg>`;
}

// Toolbar controls get a tooltip naming them, with the keyboard shortcut when there is one.
const tooltipId = (action) => `--rte-bio-tip-${action}`;

const kbd = (shortcut) =>
  shortcut ? ` <kbd data-slot="kbd-group" class="sa-kbd-group">${shortcut.split(" ").map((key) => `<kbd data-slot="kbd" class="sa-kbd">${key}</kbd>`).join("")}</kbd>` : "";

const tooltip = (action, label, shortcut) =>
  `<div id="${tooltipId(action)}" popover="hint" data-slot="tooltip-content" data-anchor-side="bottom" class="sa-tooltip-content anchored-bottom-center">${label}${kbd(shortcut)}</div>`;

const button = (action, label, iconName, shortcut) =>
  `<button type="button" aria-label="${label}" interestfor="${tooltipId(action)}" data-action="${action}" data-slot="button" class="sa-button sa-button-variant-ghost sa-button-size-icon-sm">${icon(iconName)}</button>${tooltip(action, label, shortcut)}`;

const toggle = (action, label, iconName, shortcut) =>
  `<label interestfor="${tooltipId(action)}" data-action="${action}" data-slot="toggle" data-variant="default" data-size="sm" class="sa-toggle sa-toggle-variant-default sa-toggle-size-sm"><input aria-label="${label}" type="checkbox" data-slot="toggle-input" class="sr-only">${icon(iconName)}</label>${tooltip(action, label, shortcut)}`;

const bubbleToggle = (action, label, iconName) =>
  `<label data-action="${action}" data-slot="toggle" data-variant="default" data-size="sm" class="sa-toggle sa-toggle-variant-default sa-toggle-size-sm"><input aria-label="${label}" type="checkbox" data-slot="toggle-input" class="sr-only">${icon(iconName)}</label>`;

const radio = (value, label) =>
  `<div role="menuitemradio" tabindex="-1" aria-checked="false" data-slot="dropdown-menu-radio-item" data-state="unchecked" data-value="${value}" data-radio-group="--rte-bio-block-radio" class="sa-dropdown-menu-radio-item"><span class="sa-dropdown-menu-item-indicator" data-slot="dropdown-menu-radio-item-indicator">${icon("check", "size-4")}</span>${label}</div>`;

// Slash menu items: the value is what the menu filters on; the shortcut is the Markdown-style input rule.
const slash = (name, label, iconName, value, shortcut) =>
  `<prosekit-autocomplete-item value="${value}" data-slash="${name}" class="sa-dropdown-menu-item">${icon(iconName)}${label}${shortcut ? `<span data-slot="dropdown-menu-shortcut" class="sa-dropdown-menu-shortcut">${shortcut}</span>` : ""}</prosekit-autocomplete-item>`;

// Table handle menu items: dropdown menu rows; the header items are checkbox rows with an indicator.
const tableItem = (action, label, iconName, variant) =>
  `<prosekit-menu-item data-table-action="${action}"${variant ? ` data-variant="${variant}"` : ""} data-slot="dropdown-menu-item" class="sa-dropdown-menu-item">${icon(iconName)}${label}</prosekit-menu-item>`;

const tableCheck = (action, label, iconName) =>
  `<prosekit-menu-item data-table-action="${action}" data-state="unchecked" data-slot="dropdown-menu-checkbox-item" class="sa-dropdown-menu-item sa-dropdown-menu-checkbox-item">${icon(iconName)}${label}<span class="sa-dropdown-menu-item-indicator" data-slot="dropdown-menu-checkbox-item-indicator">${icon("check", "size-4")}</span></prosekit-menu-item>`;

const menuSeparator = `<div role="separator" aria-orientation="horizontal" data-slot="dropdown-menu-separator" class="sa-dropdown-menu-separator"></div>`;

const separator = `<div role="separator" aria-orientation="vertical" data-slot="separator" data-orientation="vertical" class="sa-separator sa-separator-vertical"></div>`;

const html = readFileSync("page.template.html", "utf8")
  .replace(/{{icon:([a-z0-9-]+)}}/g, (_, n) => icon(n))
  .replace(/{{button:([^:]+):([^:]+):([^:]+):([^}]*)}}/g, (_, a, l, i, k) => button(a, l, i, k))
  .replace(/{{toggle:([^:]+):([^:]+):([^:]+):([^}]*)}}/g, (_, a, l, i, k) => toggle(a, l, i, k))
  .replace(/{{bubble-toggle:([^:]+):([^:]+):([^}]+)}}/g, (_, a, l, i) => bubbleToggle(a, l, i))
  .replace(/{{radio:([^:]+):([^}]+)}}/g, (_, v, l) => radio(v, l))
  .replace(/{{slash:([^:]+):([^:]+):([^:]+):([^:]+):([^}]*)}}/g, (_, n, l, i, v, k) => slash(n, l, i, v, k))
  .replace(/{{table-item:([^:]+):([^:]+):([^:}]+)(?::([^}]+))?}}/g, (_, a, l, i, v) => tableItem(a, l, i, v))
  .replace(/{{table-check:([^:]+):([^:]+):([^}]+)}}/g, (_, a, l, i) => tableCheck(a, l, i))
  .replace(/{{menu-separator}}/g, menuSeparator)
  .replace(/{{separator}}/g, separator);

writeFileSync("../rich-text-editor.html", html);

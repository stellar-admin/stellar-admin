import { LitElement } from "lit";
import { customElement } from "lit/decorators.js";

const TAB = '[role="tab"]';

/**
 * Tabs web component. Wrap it around a `[role="tablist"]` of tab buttons and the
 * `[role="tabpanel"]`s they control; each tab names its panel with `aria-controls`.
 * A tab selects itself with the Invoker Commands API: `command="--select-tab"`
 * and `commandfor` set to this element's id.
 *
 * All state lives in the DOM — `aria-selected`, `tabindex` and `data-active` on the
 * tabs, `hidden` on the panels — so the server picks the initial tab and tabs that
 * htmx adds need no re-initialization.
 *
 * The arrow keys move between the enabled tabs along the list's `aria-orientation`
 * (Home and End jump to the first and last) and select the tab they move to.
 *
 * Selecting a tab raises a bubbling `tab-hide` event on the panel it hides, then
 * `tab-show` on the panel it shows (detail: `{ tab, panel }`), after the change, so
 * the shown panel is visible when the event fires. The initial state raises no
 * events. Because the event fires on the panel, htmx attributes on the panel need
 * no `from:` modifier: `hx-get="…" hx-trigger="tab-show once"`.
 */
@customElement("sel-tabs")
export class Tabs extends LitElement {
  override createRenderRoot() {
    return this;
  }

  override connectedCallback() {
    super.connectedCallback();
    this.addEventListener("command", this.#onCommand);
    this.addEventListener("keydown", this.#onKeyDown);
  }

  override disconnectedCallback() {
    super.disconnectedCallback();
    this.removeEventListener("command", this.#onCommand);
    this.removeEventListener("keydown", this.#onKeyDown);
  }

  /** The tabs this element controls (excluding those of nested tabs). */
  get tabs(): HTMLElement[] {
    return Array.from(this.querySelectorAll<HTMLElement>(TAB)).filter((tab) => this.#owns(tab));
  }

  /** The selected tab. */
  get selectedTab(): HTMLElement | null {
    return this.tabs.find((tab) => tab.getAttribute("aria-selected") === "true") ?? null;
  }

  /** Selects a tab and shows its panel. */
  select(tab: HTMLElement) {
    if (!this.#owns(tab) || this.#isDisabled(tab)) return;

    const previous = this.selectedTab;
    if (previous === tab) return;

    for (const other of this.tabs) {
      const selected = other === tab;
      other.setAttribute("aria-selected", String(selected));
      other.tabIndex = selected ? 0 : -1;
      other.toggleAttribute("data-active", selected);
      const panel = this.#panelFor(other);
      if (panel) panel.hidden = !selected;
    }

    const previousPanel = previous && this.#panelFor(previous);
    if (previousPanel) this.#dispatch("tab-hide", previous, previousPanel);

    const panel = this.#panelFor(tab);
    if (panel) this.#dispatch("tab-show", tab, panel);
  }

  #owns(element: Element) {
    return element.closest("sel-tabs") === this;
  }

  #isDisabled(tab: HTMLElement) {
    return tab.matches(":disabled, [aria-disabled='true']");
  }

  #panelFor(tab: Element): HTMLElement | null {
    const controls = tab.getAttribute("aria-controls");
    return controls ? document.getElementById(controls) : null;
  }

  #dispatch(type: "tab-show" | "tab-hide", tab: HTMLElement, panel: HTMLElement) {
    panel.dispatchEvent(new CustomEvent(type, { bubbles: true, detail: { tab, panel } }));
  }

  #onCommand = (event: Event) => {
    const { command, source } = event as Event & { command: string; source: Element | null };
    if (command !== "--select-tab" || !(source instanceof HTMLElement) || !source.matches(TAB)) {
      return;
    }

    this.select(source);
  };

  #onKeyDown = (event: KeyboardEvent) => {
    const target = event.target;
    if (!(target instanceof HTMLElement) || !target.matches(TAB) || !this.#owns(target)) return;
    if (event.altKey || event.ctrlKey || event.metaKey) return;

    const list = target.closest('[role="tablist"]');
    const vertical = list?.getAttribute("aria-orientation") === "vertical";
    const rtl = getComputedStyle(target).direction === "rtl";
    const tabs = this.tabs.filter((tab) => tab.closest('[role="tablist"]') === list);
    const enabled = tabs.filter((tab) => !this.#isDisabled(tab));
    const index = enabled.indexOf(target);
    if (enabled.length === 0) return;

    let next: HTMLElement | undefined;
    switch (event.key) {
      case vertical ? "ArrowUp" : rtl ? "ArrowRight" : "ArrowLeft":
        next = enabled[(index - 1 + enabled.length) % enabled.length];
        break;
      case vertical ? "ArrowDown" : rtl ? "ArrowLeft" : "ArrowRight":
        next = enabled[(index + 1) % enabled.length];
        break;
      case "Home":
        next = enabled[0];
        break;
      case "End":
        next = enabled[enabled.length - 1];
        break;
      default:
        return;
    }

    event.preventDefault();
    if (!next) return;

    next.focus();
    this.select(next);
  };
}

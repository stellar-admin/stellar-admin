import { LitElement } from "lit";
import { customElement } from "lit/decorators.js";

const CHOICE = '[data-slot="questionnaire-choice"]';
const CHOICE_INPUT = '[data-slot="questionnaire-choice-input"]';
const ITEM = '[data-slot="questionnaire-item"]';

/**
 * Questionnaire keyboard shortcuts. Wrap it around one or more questionnaire
 * items whose choices carry a `data-shortcut` key; pressing that key selects
 * the choice.
 *
 * The listener sits on this element rather than on the document, so the keys
 * only apply once focus is somewhere inside the questionnaire. Items render
 * with `tabindex="-1"`, which makes clicking anywhere in a question - its
 * title, its description, its padding - focus the question and arm its keys.
 * Clicking away puts focus back on the document and the keys go quiet, so a
 * questionnaire never swallows keystrokes meant for the rest of the page.
 *
 * The keystroke applies to the question it came from. When the questionnaire
 * holds a single question, it applies to that question wherever focus sits
 * inside the element; when it holds several, focus has to be within one, since
 * every question assigns the same keys.
 *
 * Selection runs through `focus()` and `click()` on the choice's own radio or
 * checkbox, so the change event, the label pairing and validation all behave
 * as if the choice had been clicked. Nothing is stored here: the input remains
 * the only record of what is selected, and it posts as ordinary form data.
 * Without this script the questionnaire still works; only the keys go away.
 */
@customElement("sel-questionnaire")
export class Questionnaire extends LitElement {
  override createRenderRoot() {
    return this;
  }

  override connectedCallback() {
    super.connectedCallback();
    this.addEventListener("keydown", this.#onKeyDown);
  }

  override disconnectedCallback() {
    super.disconnectedCallback();
    this.removeEventListener("keydown", this.#onKeyDown);
  }

  get #items(): HTMLElement[] {
    return Array.from(this.querySelectorAll<HTMLElement>(ITEM));
  }

  /**
   * The question a keystroke applies to: the one it came from, or the only one
   * there is. With several questions rendered, a keystroke from outside all of
   * them - the actions row, the progress line - is ambiguous and does nothing.
   */
  #resolveItem(target: Element): HTMLElement | null {
    const item = target.closest<HTMLElement>(ITEM);
    if (item && this.contains(item)) {
      return item;
    }

    const items = this.#items;
    return items.length === 1 ? items[0] : null;
  }

  #findChoiceInput(item: HTMLElement, shortcut: string): HTMLInputElement | null {
    for (const choice of item.querySelectorAll<HTMLElement>(CHOICE)) {
      if (choice.dataset.shortcut?.toUpperCase() !== shortcut) {
        continue;
      }

      // The key is spoken for either way: a disabled choice does not fall
      // through to whatever else happens to share its key.
      const input = choice.querySelector<HTMLInputElement>(CHOICE_INPUT);
      return input && !input.disabled ? input : null;
    }

    return null;
  }

  #onKeyDown = (event: KeyboardEvent) => {
    if (event.defaultPrevented || event.isComposing || event.keyCode === 229) {
      return;
    }

    // Modified keystrokes belong to the browser and to the author's own bindings.
    if (event.altKey || event.ctrlKey || event.metaKey) {
      return;
    }

    if (!(event.target instanceof Element) || isEditable(event.target)) {
      return;
    }

    const shortcut = resolveShortcut(event.key);
    if (!shortcut) {
      return;
    }

    const item = this.#resolveItem(event.target);
    const input = item ? this.#findChoiceInput(item, shortcut) : null;
    if (!input) {
      return;
    }

    // Claimed before the repeat check, so holding the key neither retriggers
    // the choice nor lets the character through to the page.
    event.preventDefault();
    if (event.repeat) {
      return;
    }

    // Focus first: it moves the radio group's tab stop to the chosen answer and
    // shows the focus ring, so the keystroke is visibly acknowledged.
    input.focus();
    input.click();
  };
}

/** Whether the target takes typed characters of its own. */
function isEditable(target: Element): boolean {
  if (target instanceof HTMLTextAreaElement || target instanceof HTMLSelectElement) {
    return true;
  }

  if (target instanceof HTMLInputElement) {
    return !["button", "checkbox", "radio", "reset", "submit"].includes(target.type);
  }

  return target instanceof HTMLElement && target.isContentEditable;
}

/** The `data-shortcut` value a key press stands for, uppercased to match on. */
function resolveShortcut(key: string): string | null {
  return key.length === 1 && key !== " " ? key.toUpperCase() : null;
}

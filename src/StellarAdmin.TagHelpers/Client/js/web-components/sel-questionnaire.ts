import { LitElement } from "lit";
import { customElement } from "lit/decorators.js";

const CHOICE = '[data-slot="questionnaire-choice"]';
const CHOICE_INPUT = '[data-slot="questionnaire-choice-input"]';
const INPUT = '[data-slot="questionnaire-input"]';
const ITEM = '[data-slot="questionnaire-item"]';

/** Every control that records an answer: the choices and the free-text answer. */
const ANSWER = `${CHOICE_INPUT}, ${INPUT}`;

/** Input types whose value the arrow keys move a caret through. */
const TEXTUAL_TYPES = ["email", "password", "search", "tel", "text", "url"];

/**
 * Questionnaire keyboard behaviour: the shortcut keys the choices carry, the
 * arrow keys that move between answers, and the free-text answer replacing the
 * choice a single-answer question holds.
 *
 * The listeners sit on this element rather than on the document, so they only
 * apply once focus is somewhere inside the questionnaire. Items render with
 * `tabindex="-1"`, which makes clicking anywhere in a question - its title, its
 * description, its padding - focus the question and arm its keys. Clicking away
 * puts focus back on the document and the keys go quiet, so a questionnaire
 * never swallows keystrokes meant for the rest of the page.
 *
 * The keystroke applies to the question it came from. When the questionnaire
 * holds a single question, it applies to that question wherever focus sits
 * inside the element; when it holds several, focus has to be within one, since
 * every question assigns the same keys.
 *
 * Selection runs through `focus()` and `click()` on the choice's own radio or
 * checkbox, so the change event, the label pairing and validation all behave
 * as if the choice had been clicked. Nothing is stored here: the inputs remain
 * the only record of what is selected, and they post as ordinary form data.
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
    this.addEventListener("input", this.#onInput);
  }

  override disconnectedCallback() {
    super.disconnectedCallback();
    this.removeEventListener("keydown", this.#onKeyDown);
    this.removeEventListener("input", this.#onInput);
  }

  get #items(): HTMLElement[] {
    return Array.from(this.querySelectorAll<HTMLElement>(ITEM));
  }

  /** The question's answers in the order they are rendered, minus the disabled ones. */
  #answers(item: HTMLElement): HTMLInputElement[] {
    return Array.from(item.querySelectorAll<HTMLInputElement>(ANSWER)).filter(
      (answer) => !answer.disabled,
    );
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

  /**
   * Moves focus one answer along, wrapping at either end, and reports whether it
   * did. The browser only arrow-navigates a radio group, which leaves the
   * checkboxes of a multiple-answer question and the free-text answer at the end
   * of a list stranded; this walks every answer the question holds, in the order
   * they are rendered.
   */
  #moveAnswerFocus(item: HTMLElement, target: Element, forward: boolean): boolean {
    const answers = this.#answers(item);
    if (answers.length === 0) {
      return false;
    }

    const index = answers.indexOf(target as HTMLInputElement);

    // A free-text answer with something typed in it keeps the arrows for its own
    // caret. An empty one has nothing to move through, so they carry on past it.
    if (isEditable(target) && !isEmptyTextAnswer(answers[index])) {
      return false;
    }

    // From the question itself - its title, the space around the choices - the
    // arrows start somewhere sensible. From anywhere else outside the answers
    // they are not ours to take.
    if (index < 0 && target !== item) {
      return false;
    }

    const next =
      index < 0
        ? (answers.find(isAnswered) ?? answers[forward ? 0 : answers.length - 1])
        : answers[(index + (forward ? 1 : -1) + answers.length) % answers.length];

    // Between two radios the browser moves and wraps by itself, and taking the
    // keystroke here as well would skip a choice.
    if (next === target || (index >= 0 && isRadio(target) && isRadio(next))) {
      return false;
    }

    next.focus();

    // Arriving at a radio selects it, the way the browser's own arrow keys do.
    // A checkbox is left alone, since it does not answer the question on its own.
    if (isRadio(next)) {
      next.click();
    }

    return true;
  }

  /**
   * A question that takes one answer takes it from one place: typing an answer of
   * your own clears the choice that was selected. A question that takes several
   * keeps them, since the free text adds to the answer rather than replacing it.
   */
  #onInput = (event: Event) => {
    const input = event.target;
    if (!(input instanceof HTMLInputElement) || input.dataset.slot !== "questionnaire-input") {
      return;
    }

    if (input.value.trim().length === 0) {
      return;
    }

    const item = input.closest<HTMLElement>(ITEM);
    if (!item || !this.contains(item)) {
      return;
    }

    // Only a single-answer question renders radios, so clearing them is the whole
    // of the rule; the checkboxes of a multiple-answer question never match.
    for (const answer of this.#answers(item)) {
      if (isRadio(answer)) {
        answer.checked = false;
      }
    }
  };

  #onKeyDown = (event: KeyboardEvent) => {
    if (event.defaultPrevented || event.isComposing || event.keyCode === 229) {
      return;
    }

    // Modified keystrokes belong to the browser and to the author's own bindings.
    if (event.altKey || event.ctrlKey || event.metaKey) {
      return;
    }

    if (!(event.target instanceof Element)) {
      return;
    }

    const item = this.#resolveItem(event.target);
    if (!item) {
      return;
    }

    if (event.key === "ArrowDown" || event.key === "ArrowUp") {
      // Holding the key walks the answers, so this runs on the repeats too.
      if (this.#moveAnswerFocus(item, event.target, event.key === "ArrowDown")) {
        event.preventDefault();
      }

      return;
    }

    if (isEditable(event.target)) {
      return;
    }

    const shortcut = resolveShortcut(event.key);
    const input = shortcut ? this.#findChoiceInput(item, shortcut) : null;
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

/** Whether the answer is one the question already holds. */
function isAnswered(answer: HTMLInputElement): boolean {
  if (answer.type === "checkbox" || answer.type === "radio") {
    return answer.checked;
  }

  return answer.value.trim().length > 0;
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

/** Whether the answer is a text box with nothing typed into it yet. */
function isEmptyTextAnswer(answer: HTMLInputElement | undefined): boolean {
  return (
    answer !== undefined && TEXTUAL_TYPES.includes(answer.type) && answer.value.trim().length === 0
  );
}

/** Whether the answer is a choice in a question that takes one answer. */
function isRadio(answer: Element | undefined): boolean {
  return answer instanceof HTMLInputElement && answer.type === "radio";
}

/** The `data-shortcut` value a key press stands for, uppercased to match on. */
function resolveShortcut(key: string): string | null {
  return key.length === 1 && key !== " " ? key.toUpperCase() : null;
}

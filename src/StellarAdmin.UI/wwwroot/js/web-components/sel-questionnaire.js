const CHOICE = '[data-slot="questionnaire-choice"]';
const CHOICE_INPUT = '[data-slot="questionnaire-choice-input"]';
const INPUT = '[data-slot="questionnaire-input"]';
const ITEM = '[data-slot="questionnaire-item"]';

/** Every control that records an answer: the choices and the free-text answer. */
const ANSWER = `${CHOICE_INPUT}, ${INPUT}`;

/** A free-text answer that stands in for the question's choices rather than adding to them. */
const REPLACING_INPUT = `${INPUT}[data-replaces-choices]`;

/** Input types whose value the arrow keys move a caret through. */
const TEXTUAL_TYPES = ["email", "password", "search", "tel", "text", "url"];

/**
 * The form a free-text answer belongs to, remembered while it is out of that
 * form so it can go back to exactly what the author wrote - which is usually
 * no attribute at all, the input simply sitting inside the form.
 * @type {WeakMap<HTMLInputElement, string | null>}
 */
const formOwner = new WeakMap();

/**
 * Questionnaire keyboard behaviour: the shortcut keys the choices carry, the
 * arrow keys that move between answers, and the free-text answer standing in
 * for the choice a single-answer question holds.
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
 * Without this script the questionnaire still works; the keys go away, and a
 * question whose answers stand in for each other posts both.
 */
export class Questionnaire extends HTMLElement {
  connectedCallback() {
    this.addEventListener("change", this.#onChange);
    this.addEventListener("input", this.#onInput);
    this.addEventListener("keydown", this.#onKeyDown);

    // The element can upgrade while its own children are still being parsed, so
    // the first pass waits for a document that holds all of them.
    if (document.readyState === "loading") {
      document.addEventListener("DOMContentLoaded", this.#syncItems, { once: true });
    } else {
      this.#syncItems();
    }
  }

  disconnectedCallback() {
    this.removeEventListener("change", this.#onChange);
    this.removeEventListener("input", this.#onInput);
    this.removeEventListener("keydown", this.#onKeyDown);
    document.removeEventListener("DOMContentLoaded", this.#syncItems);
  }

  /** @returns {HTMLElement[]} */
  get #items() {
    return Array.from(/** @type {NodeListOf<HTMLElement>} */ (this.querySelectorAll(ITEM)));
  }

  /**
   * The question's answers in the order they are rendered, minus the disabled ones.
   * @param {HTMLElement} item
   * @returns {HTMLInputElement[]}
   */
  #answers(item) {
    return Array.from(
      /** @type {NodeListOf<HTMLInputElement>} */ (item.querySelectorAll(ANSWER)),
    ).filter((answer) => !answer.disabled);
  }

  /**
   * The question a keystroke applies to: the one it came from, or the only one
   * there is. With several questions rendered, a keystroke from outside all of
   * them - the actions row, the progress line - is ambiguous and does nothing.
   * @param {Element} target
   * @returns {HTMLElement | null}
   */
  #resolveItem(target) {
    const item = /** @type {HTMLElement | null} */ (target.closest(ITEM));
    if (item && this.contains(item)) {
      return item;
    }

    const items = this.#items;
    return items.length === 1 ? items[0] : null;
  }

  /**
   * @param {HTMLElement} item
   * @param {string} shortcut
   * @returns {HTMLInputElement | null}
   */
  #findChoiceInput(item, shortcut) {
    for (const choice of /** @type {NodeListOf<HTMLElement>} */ (item.querySelectorAll(CHOICE))) {
      if (choice.dataset.shortcut?.toUpperCase() !== shortcut) {
        continue;
      }

      // The key is spoken for either way: a disabled choice does not fall
      // through to whatever else happens to share its key.
      const input = /** @type {HTMLInputElement | null} */ (choice.querySelector(CHOICE_INPUT));
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
   * @param {HTMLElement} item
   * @param {Element} target
   * @param {boolean} forward
   * @returns {boolean}
   */
  #moveAnswerFocus(item, target, forward) {
    const answers = this.#answers(item);
    if (answers.length === 0) {
      return false;
    }

    const index = answers.indexOf(/** @type {HTMLInputElement} */ (target));

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
   * Keeps every free-text answer in the question that stands in for its choices
   * either in the form or out of it. It is in the form when it is the answer -
   * something typed into it, and no choice selected - and out of it otherwise,
   * so exactly one of the two answers ever posts. What was typed is never
   * thrown away: an answer out of the form still reads back on the page, ready
   * to be returned to.
   * @param {HTMLElement} item
   */
  #syncItem(item) {
    const chosen = Array.from(
      /** @type {NodeListOf<HTMLInputElement>} */ (item.querySelectorAll(CHOICE_INPUT)),
    ).some((choice) => choice.checked);

    for (const input of /** @type {NodeListOf<HTMLInputElement>} */ (
      item.querySelectorAll(REPLACING_INPUT)
    )) {
      if (!formOwner.has(input)) {
        formOwner.set(input, input.getAttribute("form"));
      }

      if (!chosen && input.value.trim().length > 0) {
        setAttribute(input, "form", formOwner.get(input));
        continue;
      }

      // A form attribute naming no form is how an element leaves the one it sits
      // in: it stops posting, and it drops out of that form's validation, while
      // keeping the name and the rules it was set up with for when it returns.
      input.setAttribute("form", "");
    }
  }

  #syncItems = () => {
    for (const item of this.#items) {
      this.#syncItem(item);
    }
  };

  /** @param {Event} event */
  #onChange = (event) => {
    const choice = event.target;
    if (
      !(choice instanceof HTMLInputElement) ||
      choice.dataset.slot !== "questionnaire-choice-input"
    ) {
      return;
    }

    const item = /** @type {HTMLElement | null} */ (choice.closest(ITEM));
    if (item && this.contains(item)) {
      this.#syncItem(item);
    }
  };

  /** @param {Event} event */
  #onInput = (event) => {
    const input = event.target;
    if (!(input instanceof HTMLInputElement) || !input.matches(REPLACING_INPUT)) {
      return;
    }

    const item = /** @type {HTMLElement | null} */ (input.closest(ITEM));
    if (!item || !this.contains(item)) {
      return;
    }

    // Typing an answer of your own is answering the question, so the choice that
    // was selected stands down.
    if (input.value.trim().length > 0) {
      for (const choice of /** @type {NodeListOf<HTMLInputElement>} */ (
        item.querySelectorAll(CHOICE_INPUT)
      )) {
        choice.checked = false;
      }
    }

    this.#syncItem(item);
  };

  /** @param {KeyboardEvent} event */
  #onKeyDown = (event) => {
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

if (!customElements.get("sel-questionnaire")) {
  customElements.define("sel-questionnaire", Questionnaire);
}

/**
 * Whether the answer is one the question already holds.
 * @param {HTMLInputElement} answer
 * @returns {boolean}
 */
function isAnswered(answer) {
  if (answer.type === "checkbox" || answer.type === "radio") {
    return answer.checked;
  }

  return answer.value.trim().length > 0;
}

/**
 * Whether the target takes typed characters of its own.
 * @param {Element} target
 * @returns {boolean}
 */
function isEditable(target) {
  if (target instanceof HTMLTextAreaElement || target instanceof HTMLSelectElement) {
    return true;
  }

  if (target instanceof HTMLInputElement) {
    return !["button", "checkbox", "radio", "reset", "submit"].includes(target.type);
  }

  return target instanceof HTMLElement && target.isContentEditable;
}

/**
 * Whether the answer is a text box with nothing typed into it yet.
 * @param {HTMLInputElement | undefined} answer
 * @returns {boolean}
 */
function isEmptyTextAnswer(answer) {
  return (
    answer !== undefined && TEXTUAL_TYPES.includes(answer.type) && answer.value.trim().length === 0
  );
}

/**
 * Restores an attribute to what it was, which for an absent one is absence.
 * @param {HTMLElement} element
 * @param {string} name
 * @param {string | null} value
 */
function setAttribute(element, name, value) {
  if (value === null) {
    element.removeAttribute(name);
  } else {
    element.setAttribute(name, value);
  }
}

/**
 * Whether the answer is a choice in a question that takes one answer.
 * @param {Element | undefined} answer
 * @returns {boolean}
 */
function isRadio(answer) {
  return answer instanceof HTMLInputElement && answer.type === "radio";
}

/**
 * The `data-shortcut` value a key press stands for, uppercased to match on.
 * @param {string} key
 * @returns {string | null}
 */
function resolveShortcut(key) {
  return key.length === 1 && key !== " " ? key.toUpperCase() : null;
}

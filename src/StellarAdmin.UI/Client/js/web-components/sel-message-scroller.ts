import { LitElement } from "lit";
import { customElement } from "lit/decorators.js";

/** How close to an edge still counts as being at it, in pixels. */
const EDGE_THRESHOLD = 24;

/** How long to give a smooth scroll to start moving before falling back to an instant one. */
const SMOOTH_SCROLL_GRACE_MS = 100;

/** How long after the reader's last scroll input their scrolling is still considered in progress. */
const USER_SCROLL_IDLE_MS = 300;

/** Keys that scroll the viewport, and so count as the reader taking over. */
const SCROLL_KEYS = new Set([
  "ArrowUp",
  "ArrowDown",
  "PageUp",
  "PageDown",
  "Home",
  "End",
  " ",
]);

/**
 * Message scroller web component. Wrap it around a viewport and a transcript - no marker
 * attributes needed beyond the `data-slot` values the tag helpers already emit.
 *
 * It does three things:
 *   - scrolls to the newest message when it first connects (`initial-position`)
 *   - keeps the newest message in view while the reader is already there (`auto-scroll`),
 *     releasing as soon as they scroll away and re-engaging when they come back
 *   - toggles `data-active` on each scroll button, so a button with nothing to scroll to
 *     fades out and stops taking clicks
 *
 * Buttons activate it through the native Invoker Commands API (`command="--scroll-to-end"` /
 * `"--scroll-to-start"` with `commandfor` set to this element's id).
 *
 * New content is detected from the DOM rather than from any transport, so it behaves the same
 * whether messages arrive by htmx swap, server-sent events, SignalR or a hand-written fetch.
 * The listeners and observers are bound to this element rather than to the viewport, so a swap
 * that replaces the transcript wholesale does not detach them.
 *
 * Without JavaScript the transcript still lays out and scrolls; it simply starts at the top and
 * the buttons stay inert.
 */
@customElement("sel-message-scroller")
export class MessageScroller extends LitElement {
  #following = false;
  #initialized = false;
  #mutationObserver: MutationObserver | undefined;
  #observedContent: Element | undefined;
  #resizeObserver: ResizeObserver | undefined;
  #userScrolling = false;
  #userScrollTimeout: ReturnType<typeof setTimeout> | undefined;

  override createRenderRoot() {
    return this;
  }

  override connectedCallback() {
    super.connectedCallback();

    // Scroll does not bubble, but a capturing listener on this element still sees it - which
    // keeps working when the viewport itself is replaced by a swap.
    this.addEventListener("scroll", this.#onScroll, { capture: true, passive: true });
    this.addEventListener("command", this.#onCommand);

    // Following is released by the reader's own input, never by a scroll event on its own:
    // the transcript's height swings as rows render, and the scroll positions the browser
    // clamps in response are indistinguishable from a scroll event alone.
    for (const type of ["wheel", "touchmove", "pointerdown", "keydown"]) {
      this.addEventListener(type, this.#onUserScrollInput, { capture: true, passive: true });
    }

    this.#mutationObserver = new MutationObserver(this.#onContentChange);
    this.#mutationObserver.observe(this, { childList: true, subtree: true, characterData: true });

    // Growth is not always a mutation: images decoding, fonts settling and disclosure widgets
    // opening all change the transcript's height without touching the DOM.
    this.#resizeObserver = new ResizeObserver(this.#onContentChange);
    this.#observeContent();

    this.#initialize();
  }

  override disconnectedCallback() {
    super.disconnectedCallback();

    this.removeEventListener("scroll", this.#onScroll, { capture: true });
    this.removeEventListener("command", this.#onCommand);

    for (const type of ["wheel", "touchmove", "pointerdown", "keydown"]) {
      this.removeEventListener(type, this.#onUserScrollInput, { capture: true });
    }

    this.#initialized = false;
    this.#mutationObserver?.disconnect();
    this.#mutationObserver = undefined;
    this.#resizeObserver?.disconnect();
    this.#resizeObserver = undefined;
    this.#observedContent = undefined;

    clearTimeout(this.#userScrollTimeout);
  }

  /** Whether the transcript follows new content while the reader is at the newest message. */
  get #autoScroll(): boolean {
    return this.getAttribute("auto-scroll") !== "false";
  }

  get #content(): HTMLElement | null {
    return this.querySelector<HTMLElement>('[data-slot="message-scroller-content"]');
  }

  get #initialPosition(): string {
    return this.getAttribute("initial-position") ?? "end";
  }

  get #viewport(): HTMLElement | null {
    return this.querySelector<HTMLElement>('[data-slot="message-scroller-viewport"]');
  }

  /** Whether the transcript is currently following new content. */
  get following(): boolean {
    return this.#following;
  }

  /** Scrolls the transcript to its newest message and resumes following. */
  scrollToEnd(behavior: ScrollBehavior = "smooth") {
    this.#following = this.#autoScroll;
    this.#scrollTo("end", behavior);
  }

  /** Scrolls the transcript to its oldest message and stops following. */
  scrollToStart(behavior: ScrollBehavior = "smooth") {
    this.#following = false;
    this.#scrollTo("start", behavior);
  }

  /**
   * Takes the initial scroll position. A custom element can upgrade before its children have
   * been parsed, so this is retried from the mutation observer until the viewport exists rather
   * than being given up on at connection time.
   */
  #initialize() {
    const viewport = this.#viewport;
    if (this.#initialized || !viewport) {
      return;
    }

    this.#initialized = true;

    if (this.#initialPosition !== "end") {
      this.#syncButtons();
      return;
    }

    // Hide the viewport for the duration of the positioning, so the reader is never shown the
    // top of a long transcript before it jumps to the bottom. This runs synchronously rather
    // than in an animation frame: a background tab runs no animation frames, and a transcript
    // that stayed hidden and unpositioned until the tab was looked at would be a worse bug
    // than the jump this avoids.
    viewport.dataset.pendingScroll = "";

    // Following is taken up before the scroll so that rows still settling - `content-visibility`
    // rows swap a placeholder height for their real one as they render - keep the transcript
    // pinned to the newest message as they go.
    this.#following = this.#autoScroll;
    this.#scrollTo("end", "auto");

    delete viewport.dataset.pendingScroll;
    this.#syncButtons();
  }

  #isAtEnd(): boolean {
    const viewport = this.#viewport;
    return (
      viewport !== null &&
      viewport.scrollHeight - viewport.scrollTop - viewport.clientHeight <= EDGE_THRESHOLD
    );
  }

  #observeContent() {
    const content = this.#content;
    if (content === this.#observedContent) {
      return;
    }

    if (this.#observedContent) {
      this.#resizeObserver?.unobserve(this.#observedContent);
    }

    this.#observedContent = content ?? undefined;
    if (content) {
      this.#resizeObserver?.observe(content);
    }
  }

  #scrollTo(edge: "start" | "end", behavior: ScrollBehavior) {
    const viewport = this.#viewport;
    if (!viewport) {
      return;
    }

    const top = edge === "end" ? viewport.scrollHeight : 0;
    const from = viewport.scrollTop;
    viewport.scrollTo({ top, behavior });

    // Some browsers and browser configurations drop a smooth scroll entirely rather than
    // falling back to an instant one, which would leave the buttons inert. If nothing has
    // moved shortly after asking, scroll instantly instead.
    if (behavior === "smooth") {
      setTimeout(() => {
        if (viewport.scrollTop === from) {
          viewport.scrollTo({ top, behavior: "auto" });
        }
      }, SMOOTH_SCROLL_GRACE_MS);
    }
  }

  #syncButtons() {
    const viewport = this.#viewport;
    if (!viewport) {
      return;
    }

    const atStart = viewport.scrollTop <= EDGE_THRESHOLD;
    const atEnd = this.#isAtEnd();

    for (const button of this.querySelectorAll<HTMLElement>(
      '[data-slot="message-scroller-button"]',
    )) {
      const active = button.dataset.direction === "start" ? !atStart : !atEnd;
      button.dataset.active = String(active);
    }
  }

  #onCommand = (event: Event) => {
    switch ((event as Event & { command: string }).command) {
      case "--scroll-to-end":
        this.scrollToEnd();
        break;
      case "--scroll-to-start":
        this.scrollToStart();
        break;
    }
  };

  #onContentChange = () => {
    this.#initialize();
    this.#observeContent();

    if (this.#following && this.#autoScroll) {
      this.#scrollTo("end", "auto");
    }

    this.#syncButtons();
  };

  #onScroll = (event: Event) => {
    if (event.target !== this.#viewport) {
      return;
    }

    // While the reader is the one scrolling, following tracks where they end up: away from the
    // newest message releases it, back at the newest message takes it up again.
    if (this.#userScrolling) {
      this.#following = this.#autoScroll && this.#isAtEnd();
    }

    this.#syncButtons();
  };

  #onUserScrollInput = (event: Event) => {
    if (event.type === "keydown" && !SCROLL_KEYS.has((event as KeyboardEvent).key)) {
      return;
    }

    this.#userScrolling = true;
    clearTimeout(this.#userScrollTimeout);
    this.#userScrollTimeout = setTimeout(() => {
      this.#userScrolling = false;
    }, USER_SCROLL_IDLE_MS);
  };
}

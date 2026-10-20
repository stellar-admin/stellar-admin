/** @typedef {"default" | "success" | "info" | "warning" | "error" | "loading"} ToastType */

/**
 * @typedef {object} ToastOptions
 * @property {string} [id]
 * @property {string} title
 * @property {string} [description]
 * @property {ToastType} [type]
 * @property {number} [duration] Milliseconds; 0 keeps the toast open until it is closed. Defaults to the toaster's duration.
 * @property {{ label: string; href: string }} [action]
 */

/** @typedef {"up" | "down" | "left" | "right"} SwipeDirection */

/**
 * @typedef {object} ToastState
 * @property {string} id
 * @property {HTMLElement} el
 * @property {ToastOptions} options
 * @property {number} height
 * @property {boolean} ending
 * @property {number} remaining
 * @property {number} startedAt
 * @property {number} timer
 * @property {Element[]} icons
 * @property {HTMLElement} iconSlot
 * @property {HTMLElement} description
 * @property {HTMLAnchorElement} action
 */

const SWIPE_THRESHOLD = 40;
let nextId = 1;

/**
 * Shows toasts from the server-rendered template. It reproduces Base UI's Toast state contract
 * (data-* attributes and --toast-* variables) so upstream's toast styles apply unchanged. The
 * newest toast is first, in the DOM and in #toasts.
 */
export class Toaster extends HTMLElement {
  /** @type {ToastState[]} */
  #toasts = [];
  /** @type {HTMLElement} */
  #viewport;
  /** @type {HTMLElement | null} */
  #announcer = null;
  /** @type {HTMLTemplateElement} */
  #template;
  #hovering = false;
  #focused = false;
  #windowFocused = true;
  #pendingLeave = false;
  /** @type {HTMLElement | null} */
  #previousFocus = null;
  /** @type {AbortController | null} */
  #abort = null;

  // Heights change when fonts load, the theme stylesheet swaps, or the width changes the
  // wrapping. The text column is never stretched, so it reports its natural size.
  #resizeObserver = new ResizeObserver((entries) => {
    for (const entry of entries) {
      const toast = this.#toasts.find((t) => t.el.contains(entry.target));
      if (toast && !toast.ending) toast.height = this.#measure(toast.el);
    }
    this.#sync();
  });

  connectedCallback() {
    const viewport = /** @type {HTMLElement | null} */ (
      this.querySelector('[data-slot="toast-viewport"]')
    );
    const template = /** @type {HTMLTemplateElement | null} */ (
      this.querySelector('template[data-slot="toast-template"]')
    );
    if (!viewport || !template) return;

    this.#viewport = viewport;
    this.#template = template;
    this.#announcer = viewport.querySelector('[data-slot="toast-announcer"]');
    this.#abort = new AbortController();

    // Keep the (empty, zero-height) popover open for the page's lifetime, so the live region
    // is in the accessibility tree before anything is announced.
    this.#syncHost();
    this.#listen(this.#abort.signal);

    const seed = /** @type {HTMLScriptElement | null} */ (
      this.querySelector(':scope > script[type="application/json"]')
    );
    if (seed?.textContent) {
      for (const toast of /** @type {ToastOptions[]} */ (JSON.parse(seed.textContent)))
        this.addToast(toast);
    }
  }

  disconnectedCallback() {
    this.#abort?.abort();
    this.#abort = null;
    for (const toast of this.#toasts) clearTimeout(toast.timer);
    this.#resizeObserver.disconnect();
    // Bring the viewport back if it was moved into a modal dialog.
    if (this.#viewport && this.#viewport.parentElement !== this) this.append(this.#viewport);
  }

  get #limit() {
    return Number(this.getAttribute("limit") ?? 3);
  }

  get #duration() {
    return Number(this.getAttribute("duration") ?? 5000);
  }

  get #expanded() {
    return this.#hovering || this.#focused;
  }

  get #paused() {
    return this.#hovering || this.#focused || document.hidden || !this.#windowFocused;
  }

  /**
   * Shows a toast and returns its id.
   * @param {ToastOptions} options
   * @returns {string}
   */
  addToast(options) {
    const id = options.id ?? `sa-toast-${nextId++}`;
    const el = /** @type {HTMLElement} */ (
      /** @type {HTMLElement} */ (this.#template.content.firstElementChild).cloneNode(true)
    );
    const iconSlot = /** @type {HTMLElement | null} */ (
      el.querySelector('[data-slot="toast-icon"]')
    );
    /** @type {ToastState} */
    const toast = {
      id,
      el,
      options,
      height: 0,
      ending: false,
      remaining: 0,
      startedAt: 0,
      timer: 0,
      iconSlot,
      icons: [...iconSlot.querySelectorAll("[data-toast-icon]")],
      description: el.querySelector('[data-slot="toast-description"]'),
      action: el.querySelector('[data-slot="toast-action"]'),
    };
    el.id = id;
    el.style.setProperty("--toast-swipe-movement-x", "0px");
    el.style.setProperty("--toast-swipe-movement-y", "0px");
    el.setAttribute("data-starting-style", "");
    this.#fill(toast, options);
    this.#wireSwipe(toast);

    this.#viewport.prepend(el);
    this.#toasts.unshift(toast);
    toast.height = this.#measure(el);
    this.#resizeObserver.observe(el.querySelector('[data-slot="toast-text"]'));
    this.#syncHost();
    this.#sync();

    requestAnimationFrame(() =>
      requestAnimationFrame(() => el.removeAttribute("data-starting-style")),
    );
    return id;
  }

  /**
   * Changes an open toast. Options not given keep their current values.
   * @param {string} id
   * @param {Partial<ToastOptions>} options
   */
  updateToast(id, options) {
    const toast = this.#toasts.find((t) => t.id === id);
    if (!toast || toast.ending) return;
    clearTimeout(toast.timer);
    toast.timer = 0;
    this.#fill(toast, { ...toast.options, ...options });
    toast.height = this.#measure(toast.el);
    this.#sync();
  }

  /**
   * Closes a toast, resolving once its exit animation has finished.
   * @param {string} id
   * @param {SwipeDirection} [swipeDirection]
   */
  async closeToast(id, swipeDirection) {
    const toast = this.#toasts.find((t) => t.id === id);
    if (!toast || toast.ending) return;
    clearTimeout(toast.timer);
    toast.ending = true;
    const hadFocus = toast.el.contains(document.activeElement);
    if (swipeDirection) toast.el.setAttribute("data-swipe-direction", swipeDirection);
    toast.el.setAttribute("data-ending-style", "");
    this.#sync();

    // getAnimations() flushes style, so the exit transitions have started by now. Infinite
    // animations, such as the loading spinner, never finish, so they are not waited for.
    await Promise.allSettled(
      toast.el
        .getAnimations({ subtree: true })
        .filter((a) => a.effect?.getComputedTiming().endTime !== Infinity)
        .map((a) => a.finished),
    );
    this.#resizeObserver.unobserve(toast.el.querySelector('[data-slot="toast-text"]'));
    toast.el.remove();
    this.#toasts = this.#toasts.filter((t) => t !== toast);
    if (hadFocus) this.#focusAfterClose();
    this.#sync();
    this.#flushMouseLeave();
  }

  /**
   * @param {ToastState} toast
   * @param {ToastOptions} options
   */
  #fill(toast, options) {
    const { el } = toast;
    const type = options.type ?? "default";
    toast.options = options;
    el.dataset.type = type;

    // The template carries an icon for every type; keep only this one.
    const content = el.querySelector('[data-slot="toast-content"]');
    toast.iconSlot.replaceChildren(
      ...toast.icons.filter((svg) => /** @type {HTMLElement} */ (svg).dataset.toastIcon === type),
    );
    if (toast.iconSlot.childElementCount === 0) toast.iconSlot.remove();
    else if (!toast.iconSlot.isConnected || toast.iconSlot.parentElement !== content)
      content.prepend(toast.iconSlot);

    const title = /** @type {HTMLElement | null} */ (el.querySelector('[data-slot="toast-title"]'));
    title.id = `${toast.id}-title`;
    title.textContent = options.title;
    el.setAttribute("aria-labelledby", title.id);

    if (options.description) {
      toast.description.id = `${toast.id}-description`;
      toast.description.textContent = options.description;
      if (toast.description.parentElement !== title.parentElement) title.after(toast.description);
      el.setAttribute("aria-describedby", toast.description.id);
    } else {
      toast.description.remove();
      el.removeAttribute("aria-describedby");
    }

    if (options.action) {
      toast.action.textContent = options.action.label;
      toast.action.href = options.action.href;
      if (toast.action.parentElement !== content)
        el.querySelector('[data-slot="toast-close"]').before(toast.action);
    } else {
      toast.action.remove();
    }

    // Error toasts are also announced assertively; the toast itself stays in the polite region.
    if (type === "error" && this.#announcer) {
      this.#announcer.textContent = [options.title, options.description].filter(Boolean).join(". ");
    }

    toast.remaining = options.duration ?? (type === "loading" ? 0 : this.#duration);
    if (!this.#paused) this.#startTimer(toast);
  }

  /** @param {ToastState} toast */
  #startTimer(toast) {
    if (toast.remaining <= 0 || toast.ending) return;
    toast.startedAt = performance.now();
    toast.timer = window.setTimeout(() => this.closeToast(toast.id), toast.remaining);
  }

  #pauseTimers() {
    for (const toast of this.#toasts) {
      if (!toast.timer) continue;
      clearTimeout(toast.timer);
      toast.timer = 0;
      toast.remaining = Math.max(0, toast.remaining - (performance.now() - toast.startedAt));
    }
  }

  #resumeTimers() {
    if (this.#paused) return;
    for (const toast of this.#toasts) if (!toast.timer) this.#startTimer(toast);
  }

  /** @param {HTMLElement} el */
  #measure(el) {
    const previous = el.style.height;
    el.style.height = "auto";
    const height = el.offsetHeight;
    el.style.height = previous;
    return height;
  }

  #sync() {
    let visibleIndex = 0;
    let offsetY = 0;
    this.#toasts.forEach((toast, domIndex) => {
      const { el } = toast;
      const limited = !toast.ending && visibleIndex >= this.#limit;
      el.style.setProperty("--toast-index", String(toast.ending ? domIndex : visibleIndex));
      el.style.setProperty("--toast-offset-y", `${offsetY}px`);
      el.style.setProperty("--toast-height", `${toast.height}px`);
      el.toggleAttribute("data-expanded", this.#expanded);
      el.toggleAttribute("data-limited", limited);
      el.inert = limited;
      const content = el.querySelector('[data-slot="toast-content"]');
      content.toggleAttribute("data-expanded", this.#expanded);
      content.toggleAttribute("data-behind", !toast.ending && visibleIndex > 0);
      offsetY += toast.height;
      if (!toast.ending) visibleIndex += 1;
    });
    const frontmost = this.#toasts[0];
    if (frontmost)
      this.#viewport.style.setProperty("--toast-frontmost-height", `${frontmost.height}px`);
    else this.#viewport.style.removeProperty("--toast-frontmost-height");
  }

  // A modal dialog makes everything outside it inert, top layer included, so toasts would show
  // above it but ignore clicks and focus. While a modal is open the viewport moves into it
  // (re-shown, so it is above the dialog in the top layer too) and moves back when it closes.
  #syncHost() {
    const host = [...document.querySelectorAll("dialog:modal")].at(-1) ?? this;
    // Removing an open popover from the DOM hides it, so a move always re-shows it.
    if (this.#viewport.parentElement !== host) host.append(this.#viewport);
    if (!this.#viewport.matches(":popover-open")) this.#viewport.showPopover();
  }

  #focusAfterClose() {
    const next = this.#toasts.find((t) => !t.ending && !t.el.inert);
    if (next) next.el.focus({ preventScroll: true });
    else this.#restoreFocus();
  }

  #restoreFocus() {
    const target = this.#previousFocus;
    this.#previousFocus = null;
    if (target?.isConnected) target.focus({ preventScroll: true });
  }

  /** @param {boolean} value */
  #setHovering(value) {
    if (this.#hovering === value) return;
    this.#hovering = value;
    if (value) this.#pauseTimers();
    else this.#resumeTimers();
    this.#sync();
  }

  /** @param {boolean} value */
  #setFocused(value) {
    if (this.#focused === value) return;
    this.#focused = value;
    if (value) this.#pauseTimers();
    else this.#resumeTimers();
    this.#sync();
  }

  // Base UI defers collapsing while a toast is animating out, so the stack doesn't reshuffle
  // under the pointer mid-exit.
  #flushMouseLeave() {
    if (!this.#pendingLeave || this.#toasts.some((t) => t.ending)) return;
    this.#pendingLeave = false;
    this.#setHovering(false);
  }

  /** @param {AbortSignal} signal */
  #listen(signal) {
    const vp = this.#viewport;
    const enter = () => {
      this.#pendingLeave = false;
      this.#setHovering(true);
    };
    vp.addEventListener("mouseenter", enter, { signal });
    vp.addEventListener("mousemove", enter, { signal });
    vp.addEventListener(
      "mouseleave",
      () => {
        this.#pendingLeave = true;
        this.#flushMouseLeave();
      },
      { signal },
    );

    vp.addEventListener(
      "focusin",
      (event) => {
        if (/** @type {Element} */ (event.target).matches(":focus-visible")) this.#setFocused(true);
      },
      { signal },
    );
    vp.addEventListener(
      "focusout",
      (event) => {
        if (!vp.contains(/** @type {Node | null} */ (event.relatedTarget))) this.#setFocused(false);
      },
      { signal },
    );
    vp.addEventListener(
      "keydown",
      (event) => {
        if (event.key === "Tab" && event.shiftKey && event.target === vp) {
          event.preventDefault();
          this.#restoreFocus();
        }
        if (event.key === "Escape") {
          const toast = this.#toasts.find((t) => t.el.contains(/** @type {Node} */ (event.target)));
          if (toast) {
            event.preventDefault();
            this.closeToast(toast.id);
          }
        }
      },
      { signal },
    );
    vp.addEventListener(
      "click",
      (event) => {
        const close = /** @type {Element} */ (event.target).closest('[data-slot="toast-close"]');
        const toast = close && this.#toasts.find((t) => t.el.contains(close));
        if (toast) this.closeToast(toast.id);
      },
      { signal },
    );

    document.addEventListener(
      "keydown",
      (event) => {
        if (event.key !== "F6" || event.target === vp || this.#toasts.length === 0) return;
        event.preventDefault();
        this.#previousFocus = /** @type {HTMLElement | null} */ (document.activeElement);
        vp.focus({ preventScroll: true });
        this.#setFocused(true);
      },
      { signal },
    );
    document.addEventListener(
      "visibilitychange",
      () => (document.hidden ? this.#pauseTimers() : this.#resumeTimers()),
      { signal },
    );
    window.addEventListener(
      "blur",
      () => {
        this.#windowFocused = false;
        this.#pauseTimers();
      },
      { signal },
    );
    window.addEventListener(
      "focus",
      () => {
        this.#windowFocused = true;
        this.#resumeTimers();
      },
      { signal },
    );

    // Dialogs fire toggle (open and close) and close; neither bubbles, so capture.
    /** @param {Event} event */
    const onDialog = (event) => {
      if (event.target instanceof HTMLDialogElement) queueMicrotask(() => this.#syncHost());
    };
    document.addEventListener("toggle", onDialog, { capture: true, signal });
    document.addEventListener("close", onDialog, { capture: true, signal });
  }

  /** @param {ToastState} toast */
  #wireSwipe(toast) {
    const { el } = toast;
    /** @type {{ x: number; y: number; id: number } | null} */
    let start = null;

    el.addEventListener("pointerdown", (event) => {
      if (event.button !== 0 || /** @type {Element} */ (event.target).closest("a, button")) return;
      start = { x: event.clientX, y: event.clientY, id: event.pointerId };
      el.setPointerCapture(event.pointerId);
    });
    el.addEventListener("pointermove", (event) => {
      if (!start || event.pointerId !== start.id) return;
      const { dx, dy } = this.#clampSwipe(event.clientX - start.x, event.clientY - start.y);
      if (!el.hasAttribute("data-swiping") && Math.hypot(dx, dy) < 4) return;
      el.setAttribute("data-swiping", "");
      el.style.transition = "none";
      el.style.setProperty("--toast-swipe-movement-x", `${dx}px`);
      el.style.setProperty("--toast-swipe-movement-y", `${dy}px`);
    });
    /** @param {PointerEvent} event */
    const end = (event) => {
      if (!start || event.pointerId !== start.id) return;
      const { dx, dy } = this.#clampSwipe(event.clientX - start.x, event.clientY - start.y);
      start = null;
      el.removeAttribute("data-swiping");
      el.style.transition = "";
      const direction = this.#swipeDirection(dx, dy);
      if (direction && event.type === "pointerup") {
        this.closeToast(toast.id, direction);
      } else {
        el.style.setProperty("--toast-swipe-movement-x", "0px");
        el.style.setProperty("--toast-swipe-movement-y", "0px");
      }
    };
    el.addEventListener("pointerup", end);
    el.addEventListener("pointercancel", end);
  }

  // Base UI's default is down and right for a bottom-right toaster; each position swipes towards
  // its own edges.
  /** @returns {SwipeDirection[]} */
  #allowedDirections() {
    const [vertical, horizontal] = (this.#viewport.dataset.position ?? "bottom-right").split("-");
    /** @type {SwipeDirection[]} */
    const directions = [vertical === "top" ? "up" : "down"];
    if (horizontal === "left" || horizontal === "right") directions.push(horizontal);
    return directions;
  }

  // Movement in a disallowed direction is damped, and only one axis moves at a time.
  /**
   * @param {number} dx
   * @param {number} dy
   */
  #clampSwipe(dx, dy) {
    const allowed = this.#allowedDirections();
    /** @param {number} d */
    const damp = (d) => Math.sign(d) * Math.sqrt(Math.abs(d)) * 2;
    const x =
      (dx > 0 && allowed.includes("right")) || (dx < 0 && allowed.includes("left")) ? dx : damp(dx);
    const y =
      (dy > 0 && allowed.includes("down")) || (dy < 0 && allowed.includes("up")) ? dy : damp(dy);
    return Math.abs(x) >= Math.abs(y) ? { dx: x, dy: 0 } : { dx: 0, dy: y };
  }

  /**
   * @param {number} dx
   * @param {number} dy
   */
  #swipeDirection(dx, dy) {
    /** @type {Record<SwipeDirection, number>} */
    const candidates = { right: dx, left: -dx, down: dy, up: -dy };
    return this.#allowedDirections().find((d) => candidates[d] > SWIPE_THRESHOLD);
  }
}

if (!customElements.get("sel-toaster")) customElements.define("sel-toaster", Toaster);

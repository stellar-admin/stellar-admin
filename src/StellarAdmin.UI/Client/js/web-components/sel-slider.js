/**
 * Slider web component. Rendered by the `sa-slider` tag helper.
 *
 * It renders in light DOM and operates directly on the server-rendered children: the
 * track, the filled range, one thumb per value, and one hidden <input> per value (so the
 * value posts back with the form). The server already positions everything via inline
 * styles for a correct first paint; this component takes over once hydrated to handle
 * pointer drag, track-click seeking, and keyboard interaction, keeping the thumb
 * positions, the range fill, the `aria-valuenow` attributes, and the hidden inputs in
 * sync.
 *
 * Supports both orientations (read from `data-orientation`), snaps to `step`, and enforces
 * `data-min-distance` between adjacent thumbs on a range slider. A disabled (`data-disabled`) or
 * read-only (`data-readonly`) slider ignores the pointer and the keys.
 *
 * It also fills the `<output data-slot="slider-value">` elements that show its value (those
 * without `for` in its field, and those whose `for` names its id), formats values with
 * `data-value-format` and `data-value-locale` like the server, keeps each thumb's
 * `aria-valuetext` and each mark's `data-state` current, and names unnamed thumbs after the
 * field label.
 */
let labelCount = 0;

/**
 * The number of decimal places in a number, including one written in exponent notation.
 * @param {number} value
 */
function countDecimals(value) {
  const [mantissa, exponent] = String(value).split("e");
  const fraction = mantissa.split(".")[1]?.length ?? 0;
  return Math.max(0, fraction - Number(exponent ?? 0));
}

export class Slider extends HTMLElement {
  #min = 0;
  #max = 100;
  #step = 1;
  #decimals = 0;
  #minDistance = 0;
  #vertical = false;
  #edgeAligned = false;
  /** @type {string | null} */
  #valueFormat = null;
  #numberFormat = new Intl.NumberFormat("en-US", { maximumFractionDigits: 0 });

  /** @type {HTMLElement | null} */
  #track = null;
  /** @type {HTMLElement | null} */
  #range = null;
  /** @type {HTMLElement[]} */
  #thumbs = [];
  /** @type {HTMLInputElement[]} */
  #inputs = [];
  /** @type {HTMLElement[]} */
  #marks = [];
  /** @type {HTMLOutputElement[]} */
  #outputs = [];
  /** @type {number[]} */
  #values = [];

  #dragIndex = -1;
  /** @type {ResizeObserver | null} */
  #thumbSizeObserver = null;

  connectedCallback() {
    this.#min = this.#numAttr("min", 0);
    this.#max = this.#numAttr("max", 100);
    const step = this.#numAttr("step", 1);
    this.#step = step > 0 ? step : 1;
    // Stepped values share the decimals of the minimum and step, like the server's formatting
    this.#decimals = Math.max(countDecimals(this.#min), countDecimals(this.#step));
    this.#minDistance = this.#numAttr("data-min-distance", 0);
    this.#vertical = this.getAttribute("data-orientation") === "vertical";
    this.#edgeAligned = this.getAttribute("data-thumb-alignment") === "edge";

    this.#track = /** @type {HTMLElement | null} */ (
      this.querySelector('[data-slot="slider-track"]')
    );
    this.#range = /** @type {HTMLElement | null} */ (
      this.querySelector('[data-slot="slider-range"]')
    );
    this.#thumbs = Array.from(
      /** @type {NodeListOf<HTMLElement>} */ (this.querySelectorAll('[data-slot="slider-thumb"]')),
    );
    this.#inputs = Array.from(
      /** @type {NodeListOf<HTMLInputElement>} */ (
        this.querySelectorAll('input[data-slot="slider-input"]')
      ),
    );
    this.#values = this.#thumbs.map((thumb) =>
      this.#clampToRange(Number(thumb.getAttribute("aria-valuenow") ?? this.#min)),
    );
    this.#marks = Array.from(
      /** @type {NodeListOf<HTMLElement>} */ (this.querySelectorAll('[data-slot="slider-mark"]')),
    );
    this.#valueFormat = this.getAttribute("data-value-format");
    try {
      this.#numberFormat = new Intl.NumberFormat(
        this.getAttribute("data-value-locale") ?? "en-US",
        { maximumFractionDigits: this.#decimals },
      );
    } catch {
      // An unknown locale falls back to the server's invariant culture
      this.#numberFormat = new Intl.NumberFormat("en-US", {
        maximumFractionDigits: this.#decimals,
      });
    }
    this.#outputs = this.#findOutputs();
    this.#nameThumbs();
    this.#observeThumbSize();
    this.#renderValues();

    this.addEventListener("pointerdown", this.#onPointerDown);
    this.addEventListener("keydown", this.#onKeyDown);
  }

  disconnectedCallback() {
    this.removeEventListener("pointerdown", this.#onPointerDown);
    this.removeEventListener("keydown", this.#onKeyDown);
    this.#thumbSizeObserver?.disconnect();
    this.#thumbSizeObserver = null;
    this.#endDrag();
  }

  get #disabled() {
    return this.hasAttribute("data-disabled");
  }

  get #readOnly() {
    return this.hasAttribute("data-readonly");
  }

  /**
   * @param {string} name
   * @param {number} fallback
   */
  #numAttr(name, fallback) {
    const value = Number(this.getAttribute(name));
    return Number.isFinite(value) ? value : fallback;
  }

  /** @param {PointerEvent} event */
  #onPointerDown = (event) => {
    if (this.#disabled || this.#readOnly || event.button !== 0) return;

    const target = /** @type {Element} */ (event.target);
    const thumb = /** @type {HTMLElement | null} */ (target.closest('[data-slot="slider-thumb"]'));
    if (thumb) {
      this.#startDrag(this.#thumbs.indexOf(thumb), event);
      return;
    }

    // Click anywhere on the track: jump the nearest thumb to that position, then drag it.
    if (this.#track && target.closest('[data-slot="slider-track"]')) {
      const value = this.#valueFromPointer(event);
      const index = this.#nearestThumb(value);
      this.#setThumbValue(index, value);
      this.#startDrag(index, event);
    }
  };

  /** @param {KeyboardEvent} event */
  #onKeyDown = (event) => {
    if (this.#disabled || this.#readOnly) return;

    const thumb = /** @type {HTMLElement | null} */ (
      /** @type {Element} */ (event.target).closest('[data-slot="slider-thumb"]')
    );
    if (!thumb) return;
    const index = this.#thumbs.indexOf(thumb);
    if (index < 0) return;

    const current = this.#values[index];
    /** @type {number | null} */
    let next = null;

    switch (event.key) {
      case "ArrowRight":
      case "ArrowUp":
        next = current + this.#step;
        break;
      case "ArrowLeft":
      case "ArrowDown":
        next = current - this.#step;
        break;
      case "PageUp":
        next = current + this.#step * 10;
        break;
      case "PageDown":
        next = current - this.#step * 10;
        break;
      case "Home":
        next = this.#min;
        break;
      case "End":
        next = this.#max;
        break;
    }

    if (next === null) return;
    event.preventDefault();
    this.#setThumbValue(index, this.#snap(next));
  };

  /**
   * @param {number} index
   * @param {PointerEvent} event
   */
  #startDrag(index, event) {
    if (index < 0) return;
    this.#dragIndex = index;
    this.dataset.dragging = "true";
    this.#thumbs[index].dataset.dragging = "true";
    this.#thumbs[index].focus();

    window.addEventListener("pointermove", this.#onPointerMove);
    window.addEventListener("pointerup", this.#onPointerUp);
    event.preventDefault();
  }

  /** @param {PointerEvent} event */
  #onPointerMove = (event) => {
    if (this.#dragIndex < 0) return;
    this.#setThumbValue(this.#dragIndex, this.#valueFromPointer(event));
  };

  #onPointerUp = () => {
    this.#endDrag();
  };

  #endDrag() {
    if (this.#dragIndex >= 0) {
      delete this.#thumbs[this.#dragIndex]?.dataset.dragging;
    }
    delete this.dataset.dragging;
    this.#dragIndex = -1;
    window.removeEventListener("pointermove", this.#onPointerMove);
    window.removeEventListener("pointerup", this.#onPointerUp);
  }

  /**
   * Map a pointer event to a snapped value along the track.
   * @param {PointerEvent} event
   */
  #valueFromPointer(event) {
    if (!this.#track) return this.#min;
    const rect = this.#track.getBoundingClientRect();
    const fraction = this.#vertical
      ? (rect.bottom - event.clientY) / rect.height
      : (event.clientX - rect.left) / rect.width;
    const raw = this.#min + this.#clamp(fraction, 0, 1) * (this.#max - this.#min);
    return this.#snap(raw);
  }

  /** @param {number} raw */
  #snap(raw) {
    const steps = Math.round((raw - this.#min) / this.#step);
    // Round away the floating-point noise of fractional steps, such as 0.1 + 0.2
    const value = Number((this.#min + steps * this.#step).toFixed(this.#decimals));
    return this.#clampToRange(value);
  }

  /** @param {number} value */
  #nearestThumb(value) {
    let nearest = 0;
    let smallest = Infinity;
    for (let i = 0; i < this.#values.length; i++) {
      const distance = Math.abs(this.#values[i] - value);
      if (distance < smallest) {
        smallest = distance;
        nearest = i;
      }
    }
    return nearest;
  }

  /**
   * @param {number} index
   * @param {number} value
   */
  #setThumbValue(index, value) {
    // Keep thumbs ordered and never closer than the minimum distance.
    const lowerBound = index > 0 ? this.#values[index - 1] + this.#minDistance : this.#min;
    const upperBound =
      index < this.#values.length - 1 ? this.#values[index + 1] - this.#minDistance : this.#max;
    const clamped = Number(this.#clamp(value, lowerBound, upperBound).toFixed(this.#decimals));

    if (clamped === this.#values[index]) return;
    this.#values[index] = clamped;
    this.#renderThumb(index);
    this.#renderRange();
    this.#renderValues();
    this.dispatchEvent(new Event("input", { bubbles: true }));
    this.dispatchEvent(new Event("change", { bubbles: true }));
  }

  /** @param {number} index */
  #renderThumb(index) {
    const thumb = this.#thumbs[index];
    const percent = this.#percent(this.#values[index]);
    // Edge alignment shifts by the value percentage so the thumb edge stays flush with the
    // track ends; center alignment shifts by a constant 50%. Keep the transform in sync since
    // the edge shift changes as the value moves.
    const shift = this.#edgeAligned ? percent : 50;
    if (this.#vertical) {
      thumb.style.bottom = `${percent}%`;
      thumb.style.transform = `translateY(${shift}%)`;
    } else {
      thumb.style.left = `${percent}%`;
      thumb.style.transform = `translateX(-${shift}%)`;
    }
    thumb.setAttribute("aria-valuenow", String(this.#values[index]));
    const input = this.#inputs[index];
    if (input) input.value = String(this.#values[index]);
  }

  #renderRange() {
    if (!this.#range) return;
    const percents = this.#values.map((value) => this.#percent(value));
    const low = percents.length > 1 ? Math.min(...percents) : 0;
    const high = Math.max(...percents);
    if (this.#vertical) {
      this.#range.style.bottom = `${low}%`;
      this.#range.style.top = `${100 - high}%`;
    } else {
      this.#range.style.left = `${low}%`;
      this.#range.style.right = `${100 - high}%`;
    }
  }

  /**
   * The outputs showing this slider's value: those without `for` in its field, which pair with
   * the field's slider, and those whose `for` names this slider's id anywhere in its root.
   */
  #findOutputs() {
    /** @type {Set<HTMLOutputElement>} */
    const outputs = new Set();
    const field = this.closest('[data-slot="field"]');
    /** @type {NodeListOf<HTMLOutputElement> | undefined} */ (
      field?.querySelectorAll('output[data-slot="slider-value"]:not([for])')
    )?.forEach((output) => outputs.add(output));
    if (this.id) {
      const root = /** @type {Document | ShadowRoot} */ (this.getRootNode());
      /** @type {NodeListOf<HTMLOutputElement>} */ (
        root.querySelectorAll(`output[data-slot="slider-value"][for="${CSS.escape(this.id)}"]`)
      ).forEach((output) => outputs.add(output));
    }
    return Array.from(outputs);
  }

  /**
   * In an explicit field the server cannot see the author's label from the slider, so thumbs
   * without a name of their own are labelled by the field's label, or a label targeting the host.
   */
  #nameThumbs() {
    const unnamed = this.#thumbs.filter(
      (thumb) => !thumb.hasAttribute("aria-label") && !thumb.hasAttribute("aria-labelledby"),
    );
    if (unnamed.length === 0) return;

    const root = /** @type {Document | ShadowRoot} */ (this.getRootNode());
    const label =
      /** @type {HTMLElement | null} */ (
        this.closest('[data-slot="field"]')?.querySelector('[data-slot="field-label"]')
      ) ??
      (this.id
        ? /** @type {HTMLElement | null} */ (
            root.querySelector(`label[for="${CSS.escape(this.id)}"]`)
          )
        : null);
    if (!label) return;

    if (!label.id) label.id = `sel-slider-label-${++labelCount}`;
    for (const thumb of unnamed) thumb.setAttribute("aria-labelledby", label.id);
  }

  /**
   * Marks sit at the thumb centres, which edge alignment moves inward by half a thumb at the
   * ends, so they need the thumb's size along the track. Observing the thumb measures it once
   * styles apply, and again when a hidden slider is shown.
   */
  #observeThumbSize() {
    const thumb = this.#thumbs[0];
    if (this.#marks.length === 0 || !thumb) return;
    this.#thumbSizeObserver = new ResizeObserver(() => {
      const size = this.#vertical ? thumb.offsetHeight : thumb.offsetWidth;
      if (size > 0) this.style.setProperty("--_slider-thumb-measured", `${size}px`);
    });
    this.#thumbSizeObserver.observe(thumb);
  }

  #renderValues() {
    this.#values.forEach((value, index) => {
      if (this.#valueFormat !== null) {
        this.#thumbs[index].setAttribute("aria-valuetext", this.#format(value));
      }
    });

    for (const output of this.#outputs) {
      const index = output.dataset.index;
      if (index !== undefined) {
        const value = this.#values[Number(index)];
        output.textContent = value === undefined ? "" : this.#format(value);
      } else {
        const texts = this.#values.map((value) => this.#format(value));
        output.textContent = texts.length === 2 ? `${texts[0]} – ${texts[1]}` : texts.join(", ");
      }
    }

    const low = this.#values.length > 1 ? Math.min(...this.#values) : -Infinity;
    const high = Math.max(...this.#values);
    for (const mark of this.#marks) {
      const value = Number(mark.dataset.value);
      mark.dataset.state = value >= low && value <= high ? "in-range" : "out-of-range";
    }
  }

  /** @param {number} value */
  #format(value) {
    const number = this.#numberFormat.format(value);
    return this.#valueFormat === null ? number : this.#valueFormat.replace("{0}", number);
  }

  /** @param {number} value */
  #percent(value) {
    return ((value - this.#min) / (this.#max - this.#min)) * 100;
  }

  /** @param {number} value */
  #clampToRange(value) {
    return this.#clamp(value, this.#min, this.#max);
  }

  /**
   * @param {number} value
   * @param {number} min
   * @param {number} max
   */
  #clamp(value, min, max) {
    return Math.min(Math.max(value, min), max);
  }
}

if (!customElements.get("sel-slider")) customElements.define("sel-slider", Slider);

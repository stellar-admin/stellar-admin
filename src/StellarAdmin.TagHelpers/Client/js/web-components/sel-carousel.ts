import { LitElement } from "lit";
import { customElement } from "lit/decorators.js";

const EPSILON = 1;

@customElement("sel-carousel")
export class Carousel extends LitElement {
  #resize?: ResizeObserver;
  #mutations?: MutationObserver;
  #frame = 0;
  #positions: { offset: number; item: HTMLElement }[] = [];
  #pending?: number;
  #settle?: ReturnType<typeof setTimeout>;

  override createRenderRoot() {
    return this;
  }

  override connectedCallback() {
    super.connectedCallback();

    this.addEventListener("command", this.#onCommand);
    this.addEventListener("scroll", this.#onScroll, { capture: true, passive: true });
    this.addEventListener("keydown", this.#onKeyDown);
    this.addEventListener("pointerdown", this.#cancelPending, { passive: true });
    this.addEventListener("wheel", this.#cancelPending, { passive: true });

    this.#resize = new ResizeObserver(this.#schedule);
    this.#mutations = new MutationObserver((records) => {
      // Ignore our generated indicators; observe server/htmx content and author sizing changes.
      if (
        records.some((r) => !(r.target as Element).closest?.('[data-slot="carousel-indicators"]'))
      ) {
        this.#observe();
        this.#schedule();
      }
    });

    this.#mutations.observe(this, {
      childList: true,
      subtree: true,
      characterData: true,
      attributes: true,
      attributeFilter: ["class", "style", "dir", "aria-label", "aria-labelledby", "hidden"],
    });

    this.#observe();
    this.#schedule();
  }

  override disconnectedCallback() {
    super.disconnectedCallback();

    this.removeEventListener("command", this.#onCommand);
    this.removeEventListener("scroll", this.#onScroll, true);
    this.removeEventListener("keydown", this.#onKeyDown);
    this.removeEventListener("pointerdown", this.#cancelPending);
    this.removeEventListener("wheel", this.#cancelPending);

    this.#resize?.disconnect();
    this.#mutations?.disconnect();

    cancelAnimationFrame(this.#frame);
    this.#frame = 0;
    this.#cancelPending();

    delete this.dataset.ready;
  }

  #owned<T extends HTMLElement>(selector: string): T[] {
    return [...this.querySelectorAll<T>(selector)].filter(
      (el) => el.closest("sel-carousel") === this,
    );
  }

  get #viewport() {
    return this.#owned<HTMLElement>('[data-slot="carousel-content"]')[0];
  }

  get #vertical() {
    return this.dataset.orientation === "vertical";
  }

  get #sign() {
    return !this.#vertical && this.#viewport && getComputedStyle(this.#viewport).direction === "rtl"
      ? -1
      : 1;
  }

  get #offset() {
    const viewport = this.#viewport;

    return viewport ? (this.#vertical ? viewport.scrollTop : viewport.scrollLeft * this.#sign) : 0;
  }

  #observe() {
    this.#resize?.disconnect();
    this.#resize?.observe(this);

    const viewport = this.#viewport;
    if (viewport) {
      this.#resize?.observe(viewport);

      for (const item of viewport.children) {
        this.#resize?.observe(item);
      }
    }
  }

  #schedule = () => {
    if (!this.#frame && this.isConnected) {
      this.#frame = requestAnimationFrame(() => {
        this.#frame = 0;
        this.#measure();
      });
    }
  };

  #measure() {
    const viewport = this.#viewport;
    if (!viewport) {
      return;
    }

    const bounds = viewport.getBoundingClientRect();
    const max = Math.max(
      0,
      this.#vertical
        ? viewport.scrollHeight - viewport.clientHeight
        : viewport.scrollWidth - viewport.clientWidth,
    );
    const offset = this.#offset;
    const positions: { offset: number; item: HTMLElement }[] = [];

    for (const item of this.#owned<HTMLElement>('[data-slot="carousel-item"]')) {
      if (item.parentElement !== viewport || !item.getClientRects().length) {
        continue;
      }

      const rect = item.getBoundingClientRect();
      const distance = this.#vertical
        ? rect.top - bounds.top - viewport.clientTop
        : this.#sign === -1
          ? bounds.left + viewport.clientLeft + viewport.clientWidth - rect.right
          : rect.left - bounds.left - viewport.clientLeft;
      const target = Math.min(max, Math.max(0, offset + distance));

      if (!positions.some((p) => Math.abs(p.offset - target) < EPSILON)) {
        positions.push({ offset: target, item });
      }
    }

    this.#positions = positions;
    this.#renderIndicators();
    this.#sync();

    // Browsers without invoker commands retain the native scroller, without dead controls.
    if ("commandForElement" in HTMLButtonElement.prototype) {
      this.dataset.ready = "";
    }
  }

  #renderIndicators() {
    for (const group of this.#owned<HTMLElement>('[data-slot="carousel-indicators"]')) {
      this.#positions.forEach((position, index) => {
        let button = group.children[index] as HTMLButtonElement | undefined;
        if (!button) {
          button = document.createElement("button");
          button.type = "button";
          button.className = "sa-carousel-indicator";
          button.setAttribute("data-slot", "carousel-indicator");
          group.append(button);
        }

        button.setAttribute("command", `--carousel-go-${index}`);
        button.setAttribute("commandfor", this.id);

        const labelledBy = position.item.getAttribute("aria-labelledby");
        if (labelledBy) {
          button.setAttribute("aria-labelledby", labelledBy);
          button.removeAttribute("aria-label");
        } else {
          button.removeAttribute("aria-labelledby");
          button.setAttribute(
            "aria-label",
            position.item.getAttribute("aria-label") || String(index + 1),
          );
        }
      });

      while (group.children.length > this.#positions.length) {
        group.lastElementChild?.remove();
      }
    }
  }

  #sync() {
    const offset = this.#offset;
    const last = this.#positions.at(-1)?.offset ?? 0;

    for (const button of this.#owned<HTMLButtonElement>('[data-slot="carousel-previous"]')) {
      button.disabled = offset <= EPSILON;
    }
    for (const button of this.#owned<HTMLButtonElement>('[data-slot="carousel-next"]')) {
      button.disabled = offset >= last - EPSILON;
    }

    let current = 0;
    this.#positions.forEach((p, i) => {
      if (Math.abs(p.offset - offset) < Math.abs(this.#positions[current].offset - offset)) {
        current = i;
      }
    });

    for (const group of this.#owned<HTMLElement>('[data-slot="carousel-indicators"]')) {
      [...group.children].forEach((button, index) => {
        if (index === current) {
          button.setAttribute("aria-current", "true");
        } else {
          button.removeAttribute("aria-current");
        }
      });
    }
  }

  #onScroll = (event: Event) => {
    if (event.target !== this.#viewport) {
      return;
    }

    this.#sync();

    clearTimeout(this.#settle);
    this.#settle = setTimeout(this.#cancelPending, 150);
  };

  #cancelPending = () => {
    this.#pending = undefined;
    clearTimeout(this.#settle);
  };

  #move(direction: number) {
    const offset = this.#pending ?? this.#offset;
    const position =
      direction > 0
        ? this.#positions.find((p) => p.offset > offset + EPSILON)
        : [...this.#positions].reverse().find((p) => p.offset < offset - EPSILON);

    if (position) {
      this.#go(position.offset);
    }
  }

  #go(offset: number) {
    this.#pending = offset;

    const behavior = matchMedia("(prefers-reduced-motion: reduce)").matches ? "instant" : "smooth";
    this.#viewport?.scrollTo(
      this.#vertical ? { top: offset, behavior } : { left: offset * this.#sign, behavior },
    );

    clearTimeout(this.#settle);
    this.#settle = setTimeout(this.#cancelPending, 500);
  }

  #onCommand = (event: Event) => {
    if (event.target !== this) {
      return;
    }

    const command = (event as Event & { command: string }).command;
    if (command === "--carousel-next") {
      this.#move(1);
    } else if (command === "--carousel-previous") {
      this.#move(-1);
    } else if (command.startsWith("--carousel-go-")) {
      const position = this.#positions[Number(command.slice("--carousel-go-".length))];

      if (position) {
        this.#go(position.offset);
      }
    }
  };

  #onKeyDown = (event: KeyboardEvent) => {
    // Never capture editing/navigation keys from slide links, forms, or nested widgets.
    if (
      event.target !== this.#viewport ||
      event.altKey ||
      event.ctrlKey ||
      event.metaKey ||
      event.shiftKey
    ) {
      return;
    }

    const next = this.#vertical ? "ArrowDown" : this.#sign === -1 ? "ArrowLeft" : "ArrowRight";
    const previous = this.#vertical ? "ArrowUp" : this.#sign === -1 ? "ArrowRight" : "ArrowLeft";

    if (event.key === next) {
      this.#move(1);
    } else if (event.key === previous) {
      this.#move(-1);
    } else if (event.key === "Home") {
      this.#go(0);
    } else if (event.key === "End") {
      this.#go(this.#positions.at(-1)?.offset ?? 0);
    } else {
      return;
    }

    event.preventDefault();
  };
}

/**
 * Interest invokers on any element.
 *
 * Browsers with native `interestfor` honour it only on buttons and links, and the polyfill stands
 * down there, so a tooltip on a toggle (a label around a checkbox), an input or a select never
 * opens. This handles `interestfor` on the elements the browser doesn't: it opens the target
 * popover on hover or keyboard focus and closes it when the pointer or focus leaves, using the
 * `interest-delay-start` and `interest-delay-end` delays, and fires `interest` and `loseinterest`
 * on the target as the browser does. Without native support the polyfill covers every element.
 *
 * A control inside a label, such as a toggle's hidden checkbox, takes its hover from the label and
 * its tooltip lines up with the label, since the control itself may not be visible.
 */

const nativeSupported = Object.prototype.hasOwnProperty.call(
  HTMLButtonElement.prototype,
  "interestForElement",
);

// The browser's defaults for interest-delay-start and interest-delay-end
const defaultStartDelay = 500;
const defaultEndDelay = 250;

/** @type {HTMLElement | null} */
let activeInvoker = null;
/** @type {HTMLElement | null} */
let activeTarget = null;
/** @type {ReturnType<typeof setTimeout> | undefined} */
let timer;

/**
 * @param {Element | null | undefined} element
 * @returns {element is HTMLElement}
 */
function isUnhandled(element) {
  return (
    element instanceof HTMLElement &&
    element.hasAttribute("interestfor") &&
    !("interestForElement" in element)
  );
}

/**
 * The invoker for an event target: an element with `interestfor` the browser doesn't handle
 * itself, or the label around one, which then takes the hover and anchors the tooltip.
 *
 * @param {EventTarget | null} node
 * @returns {HTMLElement | null}
 */
function invokerOf(node) {
  if (!(node instanceof Element)) return null;
  const element = node.closest("[interestfor]");
  if (isUnhandled(element)) return element.closest("label") ?? element;
  const label = node.closest("label");
  return label && isUnhandled(label.control) && label.contains(label.control) ? label : null;
}

/**
 * @param {HTMLElement} invoker
 * @returns {HTMLElement | null}
 */
function targetOf(invoker) {
  const element = invoker.hasAttribute("interestfor")
    ? invoker
    : invoker instanceof HTMLLabelElement
      ? invoker.control
      : null;
  const id = element?.getAttribute("interestfor");
  return id ? document.getElementById(id) : null;
}

/**
 * A delay in milliseconds from the invoker's computed style, such as `0.5s` or `300ms`.
 *
 * @param {HTMLElement} invoker
 * @param {"start" | "end"} edge
 */
function delayOf(invoker, edge) {
  const style = getComputedStyle(invoker);
  const value = (
    style.getPropertyValue(`interest-delay-${edge}`) ||
    style.getPropertyValue(`--interest-delay-${edge}`)
  ).trim();
  const match = /^(-?[\d.]+)(ms|s)$/.exec(value);
  if (!match) return edge === "start" ? defaultStartDelay : defaultEndDelay;
  const amount = Number(match[1]) * (match[2] === "s" ? 1000 : 1);
  return Number.isFinite(amount) && amount >= 0
    ? amount
    : edge === "start"
      ? defaultStartDelay
      : defaultEndDelay;
}

/** @param {HTMLElement} invoker */
function gainInterest(invoker) {
  clearTimeout(timer);
  if (activeInvoker === invoker) return;
  timer = setTimeout(
    () => {
      const target = targetOf(invoker);
      if (!target || !invoker.isConnected) return;
      if (activeInvoker) loseInterestNow();
      if (!target.dispatchEvent(new Event("interest", { cancelable: true }))) return;
      activeInvoker = invoker;
      activeTarget = target;
      if (target.popover !== null && !target.matches(":popover-open")) {
        /** @type {(options?: { source?: HTMLElement }) => void} */ (target.showPopover)({
          source: invoker,
        });
      }
    },
    delayOf(invoker, "start"),
  );
}

/** @param {HTMLElement} invoker */
function loseInterest(invoker) {
  clearTimeout(timer);
  if (activeInvoker !== invoker) return;
  timer = setTimeout(loseInterestNow, delayOf(invoker, "end"));
}

function loseInterestNow() {
  clearTimeout(timer);
  const target = activeTarget;
  activeInvoker = null;
  activeTarget = null;
  if (!target) return;
  target.dispatchEvent(new Event("loseinterest"));
  if (target.popover !== null && target.matches(":popover-open")) target.hidePopover();
}

if (nativeSupported) {
  document.addEventListener("pointerover", (event) => {
    if (event.pointerType === "touch") return;
    const invoker = invokerOf(event.target);
    if (invoker) {
      if (!invoker.contains(/** @type {Node | null} */ (event.relatedTarget)))
        gainInterest(invoker);
    } else if (activeTarget?.contains(/** @type {Node} */ (event.target))) {
      // Moving onto the open target keeps it open, as with native interest
      clearTimeout(timer);
    }
  });

  document.addEventListener("pointerout", (event) => {
    if (event.pointerType === "touch") return;
    const related = /** @type {Node | null} */ (event.relatedTarget);
    const invoker = invokerOf(event.target);
    if (invoker && !invoker.contains(related)) {
      if (activeInvoker === invoker && activeTarget?.contains(related)) return;
      if (activeInvoker === invoker) loseInterest(invoker);
      else clearTimeout(timer);
    } else if (
      activeInvoker &&
      activeTarget?.contains(/** @type {Node} */ (event.target)) &&
      !activeTarget.contains(related) &&
      !activeInvoker.contains(related)
    ) {
      loseInterest(activeInvoker);
    }
  });

  document.addEventListener("focusin", (event) => {
    const invoker = invokerOf(event.target);
    if (invoker?.matches(":focus-visible, :has(:focus-visible)")) gainInterest(invoker);
  });

  document.addEventListener("focusout", (event) => {
    const invoker = invokerOf(event.target);
    if (invoker && !invoker.contains(/** @type {Node | null} */ (event.relatedTarget))) {
      if (activeInvoker === invoker) loseInterest(invoker);
      else clearTimeout(timer);
    }
  });

  // A click acts on the control, so its tooltip gets out of the way
  document.addEventListener("pointerdown", (event) => {
    const invoker = invokerOf(event.target);
    if (invoker) {
      clearTimeout(timer);
      if (activeInvoker === invoker) loseInterestNow();
    }
  });
}

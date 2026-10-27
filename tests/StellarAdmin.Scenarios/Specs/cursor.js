// Draws a pointer that follows the mouse, since headless Chromium shows none, and a ripple on each press.
// It lives in the top layer and moves to the front whenever a dialog or popover opens, so it stays visible.
(() => {
  const cursor = document.createElement("div");
  cursor.setAttribute("popover", "manual");
  cursor.style.cssText =
    "position:fixed;inset:auto;left:0;top:0;margin:0;padding:0;border:0;background:none;overflow:visible;pointer-events:none;width:24px;height:24px;transform:translate(-100px,-100px)";
  cursor.innerHTML = `<svg width="24" height="24" viewBox="0 0 24 24" style="position:absolute;filter:drop-shadow(0 1px 2px rgba(0,0,0,.35))"><path d="M3 2 L3 19 L7.5 15 L10.5 21.5 L13.5 20 L10.5 13.8 L16.5 13.8 Z" fill="#111" stroke="#fff" stroke-width="1.5" stroke-linejoin="round"/></svg>`;

  function front() {
    if (!cursor.isConnected) {
      document.documentElement.append(cursor);
    }
    if (cursor.matches(":popover-open")) {
      cursor.hidePopover();
    }
    cursor.showPopover();
  }

  addEventListener(
    "mousemove",
    (event) => {
      // The tip of the arrow is at (3, 2)
      cursor.style.transform = `translate(${event.clientX - 3}px, ${event.clientY - 2}px)`;
      if (!cursor.isConnected || !cursor.matches(":popover-open")) {
        front();
      }
    },
    true,
  );

  addEventListener(
    "mousedown",
    () => {
      const ripple = document.createElement("div");
      ripple.style.cssText =
        "position:absolute;left:-11px;top:-12px;width:28px;height:28px;border-radius:999px;background:rgba(225,29,72,.3);border:2px solid rgba(225,29,72,.8);box-sizing:border-box";
      cursor.prepend(ripple);
      ripple.animate(
        [
          { transform: "scale(.3)", opacity: 1 },
          { transform: "scale(1.5)", opacity: 0 },
        ],
        { duration: 500, easing: "ease-out" },
      ).onfinish = () => ripple.remove();
    },
    true,
  );

  // A dialog or popover opened after the cursor would cover it
  addEventListener(
    "toggle",
    (event) => {
      if (event.target !== cursor && event.newState === "open") {
        front();
      }
    },
    true,
  );
})();

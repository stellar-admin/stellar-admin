// The in-page measurement, run in the browser by contrast.mjs. Returns the text elements below WCAG
// contrast, the parts that barely separate from what is behind them, and pill corners on tall boxes.
//
// Text: every element with its own visible text, against its composited background; 4.5:1, or 3:1
// for large text. Faded, hidden and disabled elements are skipped, as WCAG exempts them.
//
// Parts: every element with its own fill or border. Its separation is the larger luminance contrast
// of its fill or its border against what is behind it, and a real drop shadow counts as separation.
// A fill within 1.02 of its backdrop and no border draws no edge on purpose (cells, plain rows) and is
// skipped. The switch thumb is measured against its track, and a fill that fills a bordered parent
// shares that parent's edge.
//
// Pill corners: a drawn box (fill, border or image) over 1.5 times a control's height and wider than
// it is tall whose corner radius reaches 40% of its height, so it reads as an ellipse (a pill radius
// meant for one-line controls on a textarea, an alert, an image).

export function pageCheck(minSeparation) {
  const ctx = Object.assign(document.createElement("canvas"), { width: 1, height: 1 }).getContext(
    "2d",
    { willReadFrequently: true },
  );
  const rgba = (css) => {
    ctx.clearRect(0, 0, 1, 1);
    ctx.fillStyle = "#000";
    ctx.fillStyle = css;
    ctx.fillRect(0, 0, 1, 1);
    const d = ctx.getImageData(0, 0, 1, 1).data;
    return [d[0], d[1], d[2], d[3] / 255];
  };
  const over = (top, under) => top.map((v, i) => (i < 3 ? v * top[3] + under[i] * (1 - top[3]) : 1));
  const luminance = ([r, g, b]) =>
    [r, g, b]
      .map((v) => {
        v /= 255;
        return v <= 0.04045 ? v / 12.92 : ((v + 0.055) / 1.055) ** 2.4;
      })
      .reduce((sum, v, i) => sum + v * [0.2126, 0.7152, 0.0722][i], 0);
  const ratio = (a, b) => {
    const [high, low] = [luminance(a), luminance(b)].sort((p, q) => q - p);
    return (high + 0.05) / (low + 0.05);
  };
  const backgroundOf = (element) => {
    const layers = [];
    for (let e = element; e; e = e.parentElement) {
      const colour = rgba(getComputedStyle(e).backgroundColor);
      if (colour[3] > 0) layers.push(colour);
      if (colour[3] >= 1) break;
    }
    let base = [255, 255, 255, 1];
    if (!layers.length || layers.at(-1)[3] < 1) {
      const body = rgba(getComputedStyle(document.body).backgroundColor);
      if (body[3]) base = body;
    }
    for (let i = layers.length - 1; i >= 0; i--) base = over(layers[i], base);
    return base;
  };
  const exempt = (element, clipped) => {
    for (let e = element; e; e = e.parentElement) {
      const style = getComputedStyle(e);
      if (+style.opacity < 1 || style.visibility === "hidden") return true;
      if (clipped && style.clipPath === "inset(50%)") return true;
      if (e.matches(":disabled, [aria-disabled=true], [data-disabled]")) return true;
    }
    return false;
  };
  const label = (element) =>
    element.tagName.toLowerCase() +
    "." +
    [...element.classList]
      .filter((c) => !c.includes("/"))
      .slice(0, 2)
      .join(".");
  const sides = ["Top", "Right", "Bottom", "Left"];

  const failures = [];
  let texts = 0;
  for (const element of document.querySelectorAll("body *")) {
    if (![...element.childNodes].some((n) => n.nodeType === 3 && n.textContent.trim())) continue;
    const rect = element.getBoundingClientRect();
    if (!rect.width || !rect.height || exempt(element, true)) continue;
    const style = getComputedStyle(element);
    texts++;
    const background = backgroundOf(element);
    const contrast = ratio(over(rgba(style.color), background), background);
    const size = parseFloat(style.fontSize);
    const large = size >= 24 || (size >= 18.66 && +style.fontWeight >= 700);
    if (contrast < (large ? 3 : 4.5))
      failures.push({
        element: label(element),
        text: element.textContent.trim().slice(0, 24),
        contrast: +contrast.toFixed(2),
      });
  }

  const faint = [];
  let parts = 0;
  for (const element of document.querySelectorAll("body *")) {
    if (element.matches("thead tr, main") || element.closest("svg")) continue;
    // The prototype's own page chrome, apart from its toast stack.
    if ([...element.classList].some((c) => c.startsWith("demo-") && !c.startsWith("demo-toast")))
      continue;
    const rect = element.getBoundingClientRect();
    if (rect.width < 4 || rect.height < 4 || exempt(element, false)) continue;
    const style = getComputedStyle(element);
    const fill = rgba(style.backgroundColor);
    const bordered = (side) =>
      parseFloat(style[`border${side}Width`]) >= 1 && rgba(style[`border${side}Color`])[3] > 0;
    const hasFill = fill[3] > 0.02;
    const hasBorder = sides.some(bordered);
    if (!hasFill && !hasBorder) continue;

    const parent = element.parentElement;
    if (parent) {
      const parentRect = parent.getBoundingClientRect();
      const parentStyle = getComputedStyle(parent);
      if (
        Math.abs(parentRect.width - rect.width) <= 2 &&
        Math.abs(parentRect.height - rect.height) <= 2 &&
        sides.some((side) => parseFloat(parentStyle[`border${side}Width`]) >= 1)
      )
        continue;
    }
    const track = element.matches(".sa-switch-thumb")
      ? parent.querySelector(".sa-switch")
      : null;
    const backdrop = track
      ? over(rgba(getComputedStyle(track).backgroundColor), backgroundOf(parent))
      : parent
        ? backgroundOf(parent)
        : [255, 255, 255, 1];
    const shadowed =
      !style.boxShadow.includes("inset") &&
      [
        ...style.boxShadow.matchAll(
          /(rgba?\([^)]*\)|oklch\([^)]*\)|oklab\([^)]*\)|color\([^)]*\))\s+(-?[\d.]+)px\s+(-?[\d.]+)px\s+([\d.]+)px/g,
        ),
      ].some((m) => rgba(m[1])[3] >= 0.06 && +m[4] >= 2);
    const fillContrast = hasFill ? ratio(over(fill, backdrop), backdrop) : 1;
    const borderContrast = Math.max(
      1,
      ...sides
        .filter(bordered)
        .map((side) => ratio(over(rgba(style[`border${side}Color`]), backdrop), backdrop)),
    );
    if (!hasBorder && fillContrast < 1.02) continue;
    parts++;
    const separation = shadowed ? Infinity : Math.max(fillContrast, borderContrast);
    if (separation < minSeparation)
      faint.push({
        element: label(element),
        text: element.textContent.trim().slice(0, 18),
        separation: +separation.toFixed(3),
      });
  }

  const probe = document.body.appendChild(document.createElement("div"));
  probe.style.cssText = "position:absolute;visibility:hidden;height:var(--sa-control-h)";
  const controlHeight = probe.getBoundingClientRect().height || 32;
  probe.remove();
  const pills = [];
  for (const element of document.querySelectorAll("body *")) {
    if (element.closest("svg")) continue;
    const rect = element.getBoundingClientRect();
    if (rect.height <= 1.5 * controlHeight || rect.width < rect.height * 1.2) continue;
    if (exempt(element, false)) continue;
    const style = getComputedStyle(element);
    const drawn =
      element.matches("img, video") ||
      rgba(style.backgroundColor)[3] > 0.02 ||
      sides.some((side) => parseFloat(style[`border${side}Width`]) >= 1);
    const radius = Math.min(rect.height / 2, parseFloat(style.borderTopLeftRadius) || 0);
    if (drawn && radius >= 0.4 * rect.height)
      pills.push({
        element: label(element),
        text: element.textContent.trim().slice(0, 18),
        height: Math.round(rect.height),
      });
  }

  return { texts, failures, parts, faint, pills };
}

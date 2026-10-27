// Installs window.specAnnotations once per page: marks, badges and labels drawn over the page for screenshots.
// Everything it adds carries data-spec-annotation, so clearAnnotations removes it.
() => {
  if (window.specAnnotations) {
    return;
  }

  const color = "#e11d48";
  const font = "600 13px/1.3 system-ui, sans-serif";
  const z = 2147483647;
  const ns = "http://www.w3.org/2000/svg";

  // One SVG the size of the document per layer; the spotlight sits just below the marks
  function layer(name, zIndex) {
    const existing = document.querySelector(`[data-spec-annotation="${name}"]`);
    if (existing) {
      return existing;
    }

    const doc = document.documentElement;
    const svg = document.createElementNS(ns, "svg");
    svg.dataset.specAnnotation = name;
    svg.setAttribute("width", doc.scrollWidth);
    svg.setAttribute("height", doc.scrollHeight);
    svg.style.cssText = `position:absolute;left:0;top:0;pointer-events:none;z-index:${zIndex};overflow:visible`;
    svg.innerHTML = `<defs><marker id="spec-arrow" viewBox="0 0 10 10" refX="8" refY="5" markerWidth="7" markerHeight="7" orient="auto-start-reverse"><path d="M0 0 L10 5 L0 10 z" fill="${color}"/></marker></defs>`;
    document.body.append(svg);

    return svg;
  }

  // Document coordinates; a block that only holds text (a heading, a title span) is boxed around its
  // text rather than its full width
  function box(element, pad) {
    let rect = element.getBoundingClientRect();
    const textOnly = [...element.childNodes].every(
      (node) => node.nodeType === Node.TEXT_NODE || (node.nodeType === Node.ELEMENT_NODE && getComputedStyle(node).display.startsWith("inline")),
    );
    if (getComputedStyle(element).display === "block" && textOnly) {
      const range = document.createRange();
      range.selectNodeContents(element);
      rect = range.getBoundingClientRect();
    }

    return { x: rect.left + scrollX - pad, y: rect.top + scrollY - pad, w: rect.width + pad * 2, h: rect.height + pad * 2 };
  }

  function draw(tag, attributes) {
    const shape = document.createElementNS(ns, tag);
    for (const [name, value] of Object.entries(attributes)) {
      shape.setAttribute(name, value);
    }
    layer("marks", z).append(shape);
  }

  function text(css, content, marker) {
    const element = document.createElement("div");
    element.dataset.specAnnotation = marker ?? "";
    element.textContent = content;
    element.style.cssText = `position:absolute;pointer-events:none;z-index:${z};font:${font};${css}`;
    document.body.append(element);

    return element;
  }

  // Seeded jitter, so the hand-drawn circle comes out the same on every run
  function jitter(seed) {
    return () => (seed = (seed * 16807) % 2147483647) / 2147483647 - 0.5;
  }

  function circle(b) {
    const cx = b.x + b.w / 2;
    const cy = b.y + b.h / 2;
    const random = jitter(Math.round(b.x + b.y) || 1);
    // A little over one turn, so the ends overlap like a pen stroke
    const points = [];
    for (let i = 0; i <= 26; i++) {
      const angle = -2.2 + (i / 24) * Math.PI * 2;
      const wobble = 1 + random() * 0.06;
      points.push(`${(cx + Math.cos(angle) * (b.w / 2 + 10) * wobble).toFixed(1)} ${(cy + Math.sin(angle) * (b.h / 2 + 8) * wobble).toFixed(1)}`);
    }
    draw("path", { d: `M${points.join(" L")}`, fill: "none", stroke: color, "stroke-width": 2.5, "stroke-linecap": "round", "stroke-linejoin": "round" });
  }

  function mark(b, style) {
    switch (style) {
      case "circle":
        return circle(b);
      case "highlight":
        return draw("rect", { x: b.x, y: b.y, width: b.w, height: b.h, rx: 6, fill: color, "fill-opacity": 0.14, stroke: color, "stroke-opacity": 0.5 });
      case "underline":
        return draw("path", { d: `M${b.x} ${b.y + b.h + 2} q${b.w / 2} 4 ${b.w} 0`, fill: "none", stroke: color, "stroke-width": 3, "stroke-linecap": "round" });
      case "box":
        return draw("rect", { x: b.x, y: b.y, width: b.w, height: b.h, rx: 6, fill: "none", stroke: color, "stroke-width": 2 });
    }
  }

  // auto continues from the badges already on the page
  function badge(b, value) {
    const content = value === "auto" ? String(document.querySelectorAll('[data-spec-annotation="badge"]').length + 1) : value;
    text(
      `left:${Math.max(b.x - 11, scrollX + 2)}px;top:${Math.max(b.y - 11, scrollY + 2)}px;width:22px;height:22px;border-radius:999px;background:${color};color:#fff;display:grid;place-items:center;box-shadow:0 0 0 2px #fff`,
      content,
      "badge",
    );
  }

  // Places the label beside the target; with an arrow it sits gap pixels away, off-centre, and a curve
  // runs from its near edge to the target's
  function label(b, content, placement, gap, arrow) {
    const element = text(`background:${color};color:#fff;padding:4px 8px;border-radius:6px;white-space:nowrap`, content);
    const w = element.offsetWidth;
    const h = element.offsetHeight;
    const g = arrow ? gap : 6;
    // Without a placement the label goes below, or above when there is no room below
    const side = placement ?? (b.y + b.h + g + h <= scrollY + innerHeight ? "bottom" : "top");
    const vertical = side === "top" || side === "bottom";
    let x = { top: b.x + b.w / 2 - w / 2, bottom: b.x + b.w / 2 - w / 2, left: b.x - g - w, right: b.x + b.w + g }[side];
    let y = { top: b.y - g - h, bottom: b.y + b.h + g, left: b.y + b.h / 2 - h / 2, right: b.y + b.h / 2 - h / 2 }[side];
    if (!arrow && vertical) {
      x = b.x;
    } else if (arrow && vertical) {
      x += w / 2 + 12;
    } else if (arrow) {
      y += h + 6;
    }
    x = Math.max(scrollX + 8, Math.min(x, scrollX + innerWidth - w - 8));
    y = Math.max(scrollY + 8, y);
    element.style.left = `${x}px`;
    element.style.top = `${y}px`;
    if (!arrow) {
      return;
    }

    const from = { top: [x + 14, y + h], bottom: [x + 14, y], left: [x + w, y + h / 2], right: [x, y + h / 2] }[side];
    const to = {
      top: [b.x + b.w / 2, b.y - 3],
      bottom: [b.x + b.w / 2, b.y + b.h + 3],
      left: [b.x - 3, b.y + b.h / 2],
      right: [b.x + b.w + 3, b.y + b.h / 2],
    }[side];
    // Leaves the label and meets the target square-on, bending in between
    const dx = (to[0] - from[0]) / 2;
    const dy = (to[1] - from[1]) / 2;
    const c1 = vertical ? [from[0], from[1] + dy] : [from[0] + dx, from[1]];
    const c2 = vertical ? [to[0], to[1] - dy] : [to[0] - dx, to[1]];
    draw("path", {
      d: `M${from[0]} ${from[1]} C${c1[0]} ${c1[1]} ${c2[0]} ${c2[1]} ${to[0]} ${to[1]}`,
      fill: "none",
      stroke: color,
      "stroke-width": 2,
      "stroke-linecap": "round",
      "marker-end": "url(#spec-arrow)",
    });
  }

  window.specAnnotations = {
    annotate(element, { style, badge: badgeValue, label: labelText, placement, arrow, gap }) {
      const b = box(element, 4);
      mark(b, style ?? "box");
      if (badgeValue) {
        badge(b, badgeValue);
      }
      if (labelText) {
        label(b, labelText, placement, gap ?? 56, arrow);
      }
    },

    // Dims the page except for the elements, each cut out with rounded corners
    spotlight(elements) {
      const doc = document.documentElement;
      const holes = elements.map((element) => {
        const b = box(element, 6);
        return `M${b.x + 8} ${b.y} h${b.w - 16} a8 8 0 0 1 8 8 v${b.h - 16} a8 8 0 0 1 -8 8 h${16 - b.w} a8 8 0 0 1 -8 -8 v${16 - b.h} a8 8 0 0 1 8 -8 z`;
      });
      const dim = document.createElementNS(ns, "path");
      dim.setAttribute("d", `M0 0 H${doc.scrollWidth} V${doc.scrollHeight} H0 Z ${holes.join(" ")}`);
      dim.setAttribute("fill-rule", "evenodd");
      dim.setAttribute("fill", "rgba(15, 23, 42, 0.55)");
      layer("spotlight", z - 1).append(dim);
    },
  };
};

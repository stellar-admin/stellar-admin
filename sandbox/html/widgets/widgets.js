// Widgets prototype: draws the charts and heatmaps, and runs the control panel (sandbox only).
// Charts read their data from attributes:
//   data-chart="spark|area|line|bar|stacked|donut"
//   data-series='[[…],[…]]'  data-labels='[…]'  data-names='[…]'  data-format="int|money|pct"

const SVG = "http://www.w3.org/2000/svg";
const fmt = {
  int: (v) => Math.round(v).toLocaleString("en-US"),
  money: (v) => "$" + Math.round(v).toLocaleString("en-US"),
  pct: (v) => v.toFixed(1) + "%",
};

function el(name, attrs = {}, parent) {
  const node = document.createElementNS(SVG, name);
  for (const [k, v] of Object.entries(attrs)) node.setAttribute(k, v);
  parent?.appendChild(node);
  return node;
}

function niceMax(v) {
  const p = Math.pow(10, Math.floor(Math.log10(v)));
  for (const m of [1, 2, 4, 5, 8, 10]) if (m * p >= v) return m * p;
  return 10 * p;
}

function read(host) {
  const series = JSON.parse(host.dataset.series);
  return {
    series: Array.isArray(series[0]) ? series : [series],
    labels: host.dataset.labels ? JSON.parse(host.dataset.labels) : null,
    names: host.dataset.names ? JSON.parse(host.dataset.names) : null,
    format: fmt[host.dataset.format ?? "int"],
  };
}

function tooltip(host) {
  let tip = host.querySelector(".sa-chart-tooltip");
  if (!tip) {
    tip = document.createElement("div");
    tip.className = "sa-chart-tooltip";
    tip.hidden = true;
    host.appendChild(tip);
  }
  return tip;
}

function drawSpark(host) {
  const { series } = read(host);
  const w = host.clientWidth || 120, h = host.clientHeight || 40;
  const data = series[0];
  const min = Math.min(...data), max = Math.max(...data);
  const x = (i) => (i / (data.length - 1)) * w;
  const y = (v) => h - 2 - ((v - min) / (max - min || 1)) * (h - 4);
  const svg = el("svg", { viewBox: `0 0 ${w} ${h}`, "aria-hidden": "true" });
  const d = data.map((v, i) => `${i ? "L" : "M"}${x(i).toFixed(1)},${y(v).toFixed(1)}`).join("");
  el("path", { d: `${d}L${w},${h}L0,${h}Z`, class: "area f1" }, svg);
  el("path", { d, class: "line s1", "stroke-width": "1.5" }, svg);
  el("circle", { cx: x(data.length - 1), cy: y(data.at(-1)), r: 2.5, class: "f1" }, svg);
  host.replaceChildren(svg);
}

function drawCartesian(host, kind) {
  const { series, labels, names, format } = read(host);
  const w = host.clientWidth, h = host.clientHeight;
  if (!w || !h) return;
  const pad = { l: 44, r: 8, t: 8, b: 22 };
  const iw = w - pad.l - pad.r, ih = h - pad.t - pad.b;
  const n = series[0].length;
  const stacked = kind === "stacked";
  const totals = series[0].map((_, i) => series.reduce((s, ser) => s + ser[i], 0));
  const max = niceMax(stacked ? Math.max(...totals) : Math.max(...series.flat()));
  const svg = el("svg", { viewBox: `0 0 ${w} ${h}`, role: "img" });
  const y = (v) => pad.t + ih - (v / max) * ih;

  for (let t = 0; t <= 4; t++) {
    const v = (max / 4) * t;
    el("line", { x1: pad.l, x2: w - pad.r, y1: y(v), y2: y(v), class: "grid" }, svg);
    const tx = el("text", { x: pad.l - 8, y: y(v) + 4, "text-anchor": "end", class: "axis" }, svg);
    tx.textContent = format(v).replace(/,000$/, "k").replace(/\$(\d+),(\d)\d\d$/, "$$$1.$2k");
  }

  const band = iw / n;
  const cx = (i) => (kind === "area" || kind === "line" ? pad.l + (i / (n - 1)) * iw : pad.l + band * i + band / 2);
  const every = Math.ceil(n / Math.max(2, Math.floor(iw / 72)));
  labels?.forEach((label, i) => {
    if (i % every && i !== n - 1) return;
    if (i === n - 1 && i % every && i % every < every * 0.85) return;
    const tx = el("text", { x: cx(i), y: h - 6, "text-anchor": i === 0 && kind !== "bar" && kind !== "stacked" ? "start" : i === n - 1 && (kind === "area" || kind === "line") ? "end" : "middle", class: "axis" }, svg);
    tx.textContent = label;
  });

  if (kind === "area" || kind === "line") {
    series.forEach((ser, s) => {
      const d = ser.map((v, i) => `${i ? "L" : "M"}${cx(i).toFixed(1)},${y(v).toFixed(1)}`).join("");
      if (kind === "area" && s === 0) el("path", { d: `${d}L${cx(n - 1)},${y(0)}L${cx(0)},${y(0)}Z`, class: `area f${s + 1}` }, svg);
      el("path", { d, class: `line s${s + 1}`, ...(s > 0 ? { "stroke-dasharray": "4 4" } : {}) }, svg);
    });
  } else {
    const bw = Math.max(2, Math.min(28, band * (stacked ? 0.62 : 0.62)));
    series[0].forEach((_, i) => {
      let base = 0;
      series.forEach((ser, s) => {
        const v = ser[i];
        const top = stacked ? base + v : v;
        el("rect", { x: cx(i) - bw / 2, y: y(top), width: bw, height: Math.max(0, y(stacked ? base : 0) - y(top)), rx: s === series.length - 1 || !stacked ? 2 : 0, class: `f${s + 1}` }, svg);
        base = top;
      });
    });
  }

  const guide = el("line", { y1: pad.t, y2: pad.t + ih, class: "guide", visibility: "hidden" }, svg);
  const dots = series.map((_, s) => el("circle", { r: 4, class: `dot f${s + 1}`, visibility: "hidden" }, svg));
  host.replaceChildren(svg);
  const tip = tooltip(host);

  svg.addEventListener("pointermove", (e) => {
    const r = svg.getBoundingClientRect();
    const px = e.clientX - r.left;
    const i = Math.max(0, Math.min(n - 1, Math.round(kind === "area" || kind === "line" ? ((px - pad.l) / iw) * (n - 1) : (px - pad.l - band / 2) / band)));
    guide.setAttribute("x1", cx(i)); guide.setAttribute("x2", cx(i)); guide.setAttribute("visibility", "visible");
    let acc = 0;
    dots.forEach((dot, s) => {
      const v = stacked ? (acc += series[s][i]) : series[s][i];
      dot.setAttribute("cx", cx(i)); dot.setAttribute("cy", y(v));
      dot.setAttribute("visibility", kind === "area" || kind === "line" ? "visible" : "hidden");
    });
    tip.innerHTML = `<div class="k">${labels?.[i] ?? ""}</div>` + series.map((ser, s) => `<div>${names ? `<span class="k">${names[s]}</span> ` : ""}<b>${format(ser[i])}</b></div>`).join("");
    tip.hidden = false;
    const left = Math.min(cx(i) + 12, w - tip.offsetWidth - 4);
    tip.style.left = `${left}px`;
    tip.style.top = `${pad.t}px`;
  });
  svg.addEventListener("pointerleave", () => {
    tip.hidden = true;
    guide.setAttribute("visibility", "hidden");
    dots.forEach((dot) => dot.setAttribute("visibility", "hidden"));
  });
}

function drawDonut(host) {
  const { series, names, format } = read(host);
  const data = series[0];
  const total = data.reduce((a, b) => a + b, 0);
  const size = host.clientHeight || 160, r = size / 2 - 4, inner = r * 0.64, c = size / 2;
  const svg = el("svg", { viewBox: `0 0 ${size} ${size}`, role: "img", style: `width:${size}px;height:${size}px` });
  let a0 = -Math.PI / 2;
  data.forEach((v, i) => {
    const a1 = a0 + (v / total) * Math.PI * 2 - 0.02;
    const large = a1 - a0 > Math.PI ? 1 : 0;
    const p = (rad, a) => `${c + rad * Math.cos(a)},${c + rad * Math.sin(a)}`;
    el("path", { d: `M${p(r, a0)}A${r},${r} 0 ${large} 1 ${p(r, a1)}L${p(inner, a1)}A${inner},${inner} 0 ${large} 0 ${p(inner, a0)}Z`, class: `f${i + 1}` }, svg);
    a0 = a1 + 0.02;
  });
  const t = el("text", { x: c, y: c - 2, "text-anchor": "middle", style: "font-size:20px;font-weight:600;fill:var(--sa-color-ink)" }, svg);
  t.textContent = format(total);
  const s = el("text", { x: c, y: c + 16, "text-anchor": "middle", class: "axis" }, svg);
  s.textContent = host.dataset.caption ?? "total";
  host.replaceChildren(svg);
  const legend = host.parentElement.querySelector(".sa-chart-legend[data-for-donut]");
  if (legend && names) {
    legend.innerHTML = names.map((nm, i) => `<span style="--_c:var(--sa-chart-${i + 1})"><i></i>${nm} <b style="color:var(--sa-color-ink);font-weight:500">${Math.round((data[i] / total) * 100)}%</b></span>`).join("");
  }
}

function drawHeatmap(host) {
  // Deterministic activity for the last 53 weeks ending today.
  let seed = Number(host.dataset.seed ?? 7);
  const rand = () => ((seed = (seed * 16807) % 2147483647) / 2147483647);
  const weeks = Number(host.dataset.weeks ?? 53);
  const busy = Number(host.dataset.busy ?? 0.55);
  const unit = host.dataset.unit ?? "logins";
  const today = new Date(2026, 9, 30);
  const start = new Date(today);
  start.setDate(start.getDate() - (weeks - 1) * 7 - today.getDay());
  const cells = [], months = [];
  let total = 0, lastMonth = -1;
  for (let w = 0; w < weeks; w++) {
    for (let d = 0; d < 7; d++) {
      const date = new Date(start);
      date.setDate(start.getDate() + w * 7 + d);
      if (d === 0 && date.getDate() <= 7 && date.getMonth() !== lastMonth) {
        months[w] = date.toLocaleString("en-US", { month: "short" });
        lastMonth = date.getMonth();
      }
      if (date > today) { cells.push(`<i data-empty></i>`); continue; }
      const season = 0.8 + 0.2 * Math.sin((w / weeks) * Math.PI * 2 + 1);
      const weekday = d === 0 || d === 6 ? 0.6 : 1;
      const active = rand() < busy * season * weekday;
      const count = active ? 1 + Math.floor(rand() * rand() * 9) : 0;
      total += count;
      const level = count === 0 ? 0 : count < 3 ? 1 : count < 5 ? 2 : count < 7 ? 3 : 4;
      const label = `${count || "No"} ${count === 1 ? unit.replace(/s$/, "") : unit} on ${date.toLocaleDateString("en-US", { weekday: "short", month: "short", day: "numeric" })}`;
      cells.push(`<i data-l="${level}" title="${label}"></i>`);
    }
  }
  const monthRow = Array.from({ length: weeks }, (_, w) => `<span>${months[w] ?? ""}</span>`).join("");
  host.style.setProperty("--_weeks", weeks);
  host.innerHTML = `
    <div class="sa-heatmap-scroll"><div class="sa-heatmap-body">
      <div class="sa-heatmap-months">${monthRow}</div>
      <div class="sa-heatmap-days"><span></span><span>Mon</span><span></span><span>Wed</span><span></span><span>Fri</span><span></span></div>
      <div class="sa-heatmap-cells">${cells.join("")}</div>
    </div></div>
    <div class="sa-heatmap-footer">
      <span><b style="color:var(--sa-color-ink)">${total.toLocaleString("en-US")}</b> ${unit} in the last year</span>
      <span class="sa-heatmap-legend">Less <i style="background:var(--sa-color-tint)"></i><i style="background:color-mix(in oklab,var(--sa-color-accent) 28%,var(--sa-color-surface))"></i><i style="background:color-mix(in oklab,var(--sa-color-accent) 52%,var(--sa-color-surface))"></i><i style="background:color-mix(in oklab,var(--sa-color-accent) 76%,var(--sa-color-surface))"></i><i style="background:var(--sa-color-accent)"></i> More</span>
    </div>`;
  // Show the most recent weeks first on narrow widgets.
  const scroll = host.querySelector(".sa-heatmap-scroll");
  scroll.scrollLeft = scroll.scrollWidth;
}

function drawAll(root = document) {
  root.querySelectorAll("[data-chart]").forEach((host) => {
    if (host.closest("[hidden], [data-proto-variant]:not([data-proto-active])")) return;
    const kind = host.dataset.chart;
    if (kind === "spark") drawSpark(host);
    else if (kind === "donut") drawDonut(host);
    else drawCartesian(host, kind);
  });
  root.querySelectorAll(".sa-heatmap[data-seed]").forEach((host) => {
    if (!host.querySelector(".sa-heatmap-cells")) drawHeatmap(host);
  });
}

// --------------------------------------------------------------------------------------------
// Control panel
// --------------------------------------------------------------------------------------------
const THEMES = { Default: "", Ops: "ops", Soft: "soft", Ledger: "ledger" };
const PAGES = [
  ["Home", "widgets-home.html"],
  ["Index", "widgets-index.html"],
  ["View", "widgets-view.html"],
  ["Edit", "widgets-edit.html"],
  ["Gallery", "widgets-gallery.html"],
];
const store = {
  get: (k) => { try { return localStorage.getItem("proto-widgets:" + k); } catch { return null; } },
  set: (k, v) => { try { localStorage.setItem("proto-widgets:" + k, v); } catch {} },
};

function group(name, options, current, onPick, stack = false) {
  const div = document.createElement("div");
  div.className = "group" + (stack ? " stack" : "");
  div.dataset.group = name;
  for (const [label, value] of options) {
    const b = document.createElement("button");
    b.type = "button";
    b.textContent = label;
    b.setAttribute("aria-pressed", String(value === current));
    b.addEventListener("click", () => {
      div.querySelectorAll("button").forEach((x) => x.setAttribute("aria-pressed", "false"));
      b.setAttribute("aria-pressed", "true");
      onPick(value);
    });
    div.appendChild(b);
  }
  return div;
}

function setVariant(id) {
  document.querySelectorAll("[data-proto-variant]").forEach((n) => n.toggleAttribute("data-proto-active", n.dataset.protoVariant === id));
  const variant = window.protoVariants?.find((v) => v.id === id);
  const note = document.querySelector(".proto-controls .note[data-variant-note]");
  if (note && variant) note.innerHTML = variant.note;
  store.set(location.pathname + ":variant", id);
  requestAnimationFrame(() => drawAll());
}

function simulateLoading() {
  const widgets = [...document.querySelectorAll("[data-proto-active] .sa-widget, .proto-gallery .sa-widget[data-lazy]")].filter((w) => !w.closest("[data-static]"));
  widgets.forEach((w, i) => {
    const body = w.querySelector(":scope > .sa-card-content, :scope.sa-stat");
    if (!body) return;
    const original = body.innerHTML;
    const isStat = w.classList.contains("sa-stat");
    body.innerHTML = isStat
      ? `<div class="sa-widget-skeleton" style="grid-column:1/-1"><div class="sa-skeleton" style="width:40%" data-slot="skeleton"></div><div class="sa-skeleton" data-h="value" data-slot="skeleton"></div><div class="sa-skeleton" style="width:60%" data-slot="skeleton"></div></div>`
      : `<div class="sa-widget-skeleton"><div class="sa-skeleton" data-h="chart" data-slot="skeleton"></div><div class="sa-skeleton" style="width:70%" data-slot="skeleton"></div></div>`;
    setTimeout(() => { body.innerHTML = original; drawAll(w); }, 500 + i * 140 + Math.random() * 500);
  });
}

function buildControls() {
  const panel = document.createElement("aside");
  panel.className = "proto-controls";
  const page = location.pathname.split("/").pop();
  panel.innerHTML = `<header><span>Widgets prototype</span><button type="button" data-collapse>Hide</button></header>
    <h3>Page</h3><nav>${PAGES.map(([l, h]) => `<a href="${h}"${h === page ? ' aria-current="page"' : ""}>${l}</a>`).join("")}</nav>`;
  if (window.protoVariants?.length) {
    const h = document.createElement("h3");
    h.textContent = "Placement";
    panel.appendChild(h);
    const wrap = document.createElement("div");
    wrap.style.padding = "4px 12px 6px";
    const saved = store.get(location.pathname + ":variant");
    const initial = window.protoVariants.some((v) => v.id === saved) ? saved : window.protoVariants[0].id;
    wrap.appendChild(group("variant", window.protoVariants.map((v) => [v.label, v.id]), initial, setVariant, true));
    panel.appendChild(wrap);
    const note = document.createElement("p");
    note.className = "note";
    note.dataset.variantNote = "";
    panel.appendChild(note);
    setTimeout(() => setVariant(initial));
  }
  const h = document.createElement("h3");
  h.textContent = "Display";
  panel.appendChild(h);
  const rows = document.createElement("div");
  rows.className = "rows";
  const add = (label, node) => { const s = document.createElement("span"); s.textContent = label; rows.append(s, node); };

  const themeLink = document.getElementById("theme-css");
  const applyTheme = (t) => { themeLink.href = t ? `../../util/theme-check/presets/${t}.css` : ""; if (!t) themeLink.removeAttribute("href"); store.set("theme", t); setTimeout(() => drawAll(), 150); };
  const theme = store.get("theme") ?? "";
  applyTheme(theme);
  add("Theme", group("theme", Object.entries(THEMES).map(([l, v]) => [l, v]), theme, applyTheme));

  const mode = store.get("mode") ?? "light";
  const applyMode = (m) => { document.documentElement.classList.toggle("dark", m === "dark"); store.set("mode", m); };
  applyMode(mode);
  add("Mode", group("mode", [["Light", "light"], ["Dark", "dark"]], mode, applyMode));

  const density = store.get("density") ?? "";
  const applyDensity = (d) => { const m = document.querySelector("main"); d ? m.setAttribute("data-sa-density", d) : m.removeAttribute("data-sa-density"); store.set("density", d); setTimeout(() => drawAll(), 50); };
  applyDensity(density);
  add("Density", group("density", [["Compact", "compact"], ["Default", ""], ["Roomy", "comfortable"]], density, applyDensity));

  const slots = store.get("slots") === "on";
  document.documentElement.classList.toggle("proto-slot-labels", slots);
  add("Slots", group("slots", [["Hide", "off"], ["Show", "on"]], slots ? "on" : "off", (v) => { document.documentElement.classList.toggle("proto-slot-labels", v === "on"); store.set("slots", v); }));

  const load = document.createElement("div");
  load.className = "group";
  const lb = document.createElement("button");
  lb.type = "button";
  lb.textContent = "Simulate lazy load";
  lb.addEventListener("click", simulateLoading);
  load.appendChild(lb);
  add("Loading", load);
  panel.appendChild(rows);

  panel.querySelector("[data-collapse]").addEventListener("click", (e) => {
    const collapsed = panel.toggleAttribute("data-collapsed");
    e.target.textContent = collapsed ? "Show" : "Hide";
    store.set("collapsed", collapsed ? "1" : "");
  });
  if (store.get("collapsed")) { panel.setAttribute("data-collapsed", ""); panel.querySelector("[data-collapse]").textContent = "Show"; }
  document.body.appendChild(panel);
}

// Segmented period pickers in widget headers redraw their widget with a different slice.
document.addEventListener("change", (e) => {
  const input = e.target.closest("[data-period-for] input");
  if (!input) return;
  const id = input.closest("[data-period-for]").dataset.periodFor;
  const host = document.getElementById(id);
  const all = JSON.parse(host.dataset.full ?? host.dataset.series);
  if (!host.dataset.full) { host.dataset.full = host.dataset.series; host.dataset.fullLabels = host.dataset.labels; }
  const n = Number(input.value);
  const series = (Array.isArray(all[0]) ? all : [all]).map((s) => s.slice(-n));
  host.dataset.series = JSON.stringify(series);
  host.dataset.labels = JSON.stringify(JSON.parse(host.dataset.fullLabels).slice(-n));
  drawAll(host.parentElement);
});

document.addEventListener("toggle", (e) => { if (e.target.matches?.("details")) drawAll(e.target); }, true);

let resizeTimer;
window.addEventListener("resize", () => { clearTimeout(resizeTimer); resizeTimer = setTimeout(() => drawAll(), 120); });

buildControls();
document.fonts?.ready.then(() => drawAll());
drawAll();

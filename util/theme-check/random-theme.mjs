// Seeded random knob sets for the theme checks. Every third theme is extreme: each ranged knob
// takes one end of a wider range, to find formulas that only hold for moderate values.

export function randomThemes(count, seed) {
  const random = () => (seed = (seed * 16807) % 2147483647) / 2147483647;
  const pick = (values) => values[Math.floor(random() * values.length)];
  const between = (low, high) => +(low + random() * (high - low)).toFixed(2);

  return Array.from({ length: count }, (_, index) => {
    const extreme = index % 3 === 2;
    const ranged = (low, high, extremeLow, extremeHigh) =>
      extreme ? pick([extremeLow, extremeHigh]) : between(low, high);
    const hue = Math.round(random() * 360);
    const knobs = {
      "--sa-accent": `oklch(${ranged(0.45, 0.7, 0.3, 0.88)} ${ranged(0.08, 0.22, 0.02, 0.3)} ${hue})`,
      "--sa-danger": `oklch(${ranged(0.5, 0.65, 0.4, 0.75)} ${between(0.15, 0.24)} ${Math.round(between(15, 35))})`,
      "--sa-neutral-hue": String(random() < 0.6 ? hue : Math.round(random() * 360)),
      "--sa-neutral-chroma": String(ranged(0, 0.025, 0, 0.04)),
      "--sa-accent-tint": String(ranged(0, 0.8, 0, 1)),
      "--sa-surface-depth": String(ranged(0, 1, 0, 1)),
      "--sa-nav-tint": String(ranged(0, 1, 0, 1)),
      "--sa-line-strength": String(ranged(0.75, 1.5, 0.75, 2)),
      "--sa-radius": pick(["0px", "4px", "6px", "10px", "9999px"]),
      "--sa-pills": pick(["0", "1"]),
      "--sa-outlines": pick(["0", "1"]),
      "--sa-relief": pick(["0", "1", "2"]),
      "--sa-elevation": String(ranged(0, 2, 0, 3)),
      "--sa-density": String(ranged(0.78, 1.2, 0.7, 1.4)),
      "--sa-current-page-fill": pick(["0", "0.15", "1"]),
      "--sa-solid-destructive": pick(["0", "1"]),
      "--sa-filled-secondary": pick(["0", "1"]),
      "--sa-column-head-label": pick(["0", "1"]),
      "--sa-column-head-fill": pick(["0", "1"]),
    };
    return { name: `random${index}${extreme ? "-extreme" : ""}`, knobs };
  });
}

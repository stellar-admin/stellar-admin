// Shared bits for the lookup-editor.html and lookup-list.html prototypes:
// airport data, lucide icons, the bottom-right control bar and match highlighting.

export const airports = [
  ["AKL", "Auckland", "New Zealand"], ["AMS", "Amsterdam", "Netherlands"], ["ATH", "Athens", "Greece"],
  ["BCN", "Barcelona", "Spain"], ["BKK", "Bangkok", "Thailand"], ["BOG", "Bogotá", "Colombia"],
  ["CAI", "Cairo", "Egypt"], ["CDG", "Paris", "France"], ["CPT", "Cape Town", "South Africa"],
  ["DEL", "Delhi", "India"], ["DXB", "Dubai", "United Arab Emirates"], ["EZE", "Buenos Aires", "Argentina"],
  ["FCO", "Rome", "Italy"], ["GRU", "São Paulo", "Brazil"], ["HKG", "Hong Kong", "China"],
  ["HND", "Tokyo Haneda", "Japan"], ["IST", "Istanbul", "Türkiye"], ["JFK", "New York", "United States"],
  ["JNB", "Johannesburg", "South Africa"], ["KEF", "Reykjavík", "Iceland"], ["LAX", "Los Angeles", "United States"],
  ["LHR", "London", "United Kingdom"], ["LIS", "Lisbon", "Portugal"], ["MAD", "Madrid", "Spain"],
  ["MEX", "Mexico City", "Mexico"], ["NBO", "Nairobi", "Kenya"], ["NRT", "Tokyo Narita", "Japan"],
  ["ORD", "Chicago", "United States"], ["SIN", "Singapore", "Singapore"], ["SYD", "Sydney", "Australia"],
  ["YVR", "Vancouver", "Canada"], ["ZRH", "Zurich", "Switzerland"],
]
  .map(([code, city, country]) => ({ code, city, country }))
  .sort((a, b) => a.city.localeCompare(b.city));

export function search(term) {
  const t = (term ?? "").trim().toLowerCase();
  if (!t) return airports;
  return airports.filter((a) => [a.city, a.code, a.country].some((v) => v.toLowerCase().includes(t)));
}

// Wraps the first case-insensitive match of term in a <mark>; styling comes from the [data-match] rule on each page
export function highlight(text, term) {
  const t = (term ?? "").trim();
  const i = t ? text.toLowerCase().indexOf(t.toLowerCase()) : -1;
  if (i < 0) return text;
  return `${text.slice(0, i)}<mark data-match>${text.slice(i, i + t.length)}</mark>${text.slice(i + t.length)}`;
}

const svg = (body, cls = "") =>
  `<svg xmlns="http://www.w3.org/2000/svg" class="${cls}" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">${body}</svg>`;

export const icon = {
  search: (c) => svg('<path d="m21 21-4.34-4.34"/><circle cx="11" cy="11" r="8"/>', c),
  x: (c) => svg('<path d="M18 6 6 18"/><path d="m6 6 12 12"/>', c),
  chevrons: (c) => svg('<path d="m7 15 5 5 5-5"/><path d="m7 9 5-5 5 5"/>', c),
  check: (c) => svg('<path d="M20 6 9 17l-5-5"/>', c),
  plane: (c) => svg('<path d="M17.8 19.2 16 11l3.5-3.5C21 6 21.5 4 21 3c-1-.5-3 0-4.5 1.5L13 8 4.8 6.2c-.5-.1-.9.1-1.1.5l-.3.5c-.2.5-.1 1 .3 1.3L9 12l-2 3H4l-1 1 3 2 2 3 1-1v-3l3-2 3.5 5.3c.3.4.8.5 1.3.3l.5-.2c.4-.3.6-.7.5-1.2z"/>', c),
  plus: (c) => svg('<path d="M5 12h14"/><path d="M12 5v14"/>', c),
  arrowRight: (c) => svg('<path d="M5 12h14"/><path d="m12 5 7 7-7 7"/>', c),
  searchX: (c) => svg('<path d="m13.5 8.5-5 5"/><path d="m8.5 8.5 5 5"/><circle cx="11" cy="11" r="8"/><path d="m21 21-4.3-4.3"/>', c),
  loader: (c) => svg('<path d="M21 12a9 9 0 1 1-6.219-8.56"/>', c),
  pencil: (c) => svg('<path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"/>', c),
};

const themes = ["parallax", "shadcn.nova", "shadcn.vega", "shadcn.lyra", "ledger", "aurora"];

// Control bar: theme bundle swap + dark mode; extra buttons can be appended by the page
export function controlBar(extra = "") {
  const bar = document.createElement("div");
  bar.className =
    "fixed right-4 bottom-4 z-50 flex flex-wrap items-center gap-1 rounded-lg border bg-background/95 p-2 text-xs shadow-lg backdrop-blur";
  bar.innerHTML =
    themes
      .map(
        (t) =>
          `<button type="button" data-set-theme="${t}" class="rounded-md px-2 py-1 hover:bg-muted data-[active]:bg-primary data-[active]:text-primary-foreground">${t}</button>`,
      )
      .join("") +
    `<button type="button" data-toggle-dark class="ml-2 rounded-md border px-2 py-1">dark</button>${extra}`;
  document.body.append(bar);

  const link = document.getElementById("theme-css");
  const mark = (theme) =>
    bar.querySelectorAll("[data-set-theme]").forEach((b) => b.toggleAttribute("data-active", b.dataset.setTheme === theme));
  mark(link.href.match(/stellar-admin\.(.+)\.css/)[1]);
  bar.addEventListener("click", (e) => {
    const btn = e.target.closest("button");
    if (btn?.dataset.setTheme) {
      link.href = `../../src/StellarAdmin.TagHelpers/wwwroot/stellar-admin.${btn.dataset.setTheme}.css`;
      mark(btn.dataset.setTheme);
    } else if (btn?.hasAttribute("data-toggle-dark")) {
      document.documentElement.classList.toggle("dark");
    }
  });
  return bar;
}

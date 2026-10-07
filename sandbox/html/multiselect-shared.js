// Shared bits for the multiselect-*.html prototypes: Voyager Travel data sets, the item parts
// (code chip, avatar), a tiny "server" with latency and paging, and re-exports from lookup-shared.js.

export { airports, search, highlight, icon, controlBar } from "./lookup-shared.js";
import { airports, icon as baseIcon } from "./lookup-shared.js";

const svg = (body, cls = "") =>
  `<svg xmlns="http://www.w3.org/2000/svg" class="${cls}" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">${body}</svg>`;

// Extra lucide icons the multiselect pages need
export const icons = {
  ...baseIcon,
  chevronDown: (c) => svg('<path d="m6 9 6 6 6-6"/>', c),
  chevronRight: (c) => svg('<path d="m9 18 6-6-6-6"/>', c),
  chevronLeft: (c) => svg('<path d="m15 18-6-6 6-6"/>', c),
  chevronsRight: (c) => svg('<path d="m6 17 5-5-5-5"/><path d="m13 17 5-5-5-5"/>', c),
  chevronsLeft: (c) => svg('<path d="m11 17-5-5 5-5"/><path d="m18 17-5-5 5-5"/>', c),
  grip: (c) => svg('<circle cx="9" cy="12" r="1"/><circle cx="9" cy="5" r="1"/><circle cx="9" cy="19" r="1"/><circle cx="15" cy="12" r="1"/><circle cx="15" cy="5" r="1"/><circle cx="15" cy="19" r="1"/>', c),
  trash: (c) => svg('<path d="M3 6h18"/><path d="M19 6v14c0 1-1 2-2 2H7c-1 0-2-1-2-2V6"/><path d="M8 6V4c0-1 1-2 2-2h4c1 0 2 1 2 2v2"/>', c),
  circlePlus: (c) => svg('<circle cx="12" cy="12" r="10"/><path d="M8 12h8"/><path d="M12 8v8"/>', c),
  listChecks: (c) => svg('<path d="m3 17 2 2 4-4"/><path d="m3 7 2 2 4-4"/><path d="M13 6h8"/><path d="M13 12h8"/><path d="M13 18h8"/>', c),
  users: (c) => svg('<path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/>', c),
  mapPin: (c) => svg('<path d="M20 10c0 4.993-5.539 10.193-7.399 11.799a1 1 0 0 1-1.202 0C9.539 20.193 4 14.993 4 10a8 8 0 0 1 16 0"/><circle cx="12" cy="10" r="3"/>', c),
  tag: (c) => svg('<path d="M12.586 2.586A2 2 0 0 0 11.172 2H4a2 2 0 0 0-2 2v7.172a2 2 0 0 0 .586 1.414l8.704 8.704a2.426 2.426 0 0 0 3.42 0l6.58-6.58a2.426 2.426 0 0 0 0-3.42z"/><circle cx="7.5" cy="7.5" r=".5" fill="currentColor"/>', c),
  clipboard: (c) => svg('<rect width="8" height="4" x="8" y="2" rx="1" ry="1"/><path d="M16 4h2a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h2"/>', c),
  minus: (c) => svg('<path d="M5 12h14"/>', c),
  undo: (c) => svg('<path d="M9 14 4 9l5-5"/><path d="M4 9h10.5a5.5 5.5 0 0 1 5.5 5.5a5.5 5.5 0 0 1-5.5 5.5H11"/>', c),
};

// Tour guides (people with avatars) — many-to-many "Guides" on a tour
export const guides = [
  ["amara", "Amara Okafor", "Lagos · English, Yoruba"],
  ["bruno", "Bruno Costa", "Lisbon · Portuguese, Spanish"],
  ["chen", "Chen Wei", "Hong Kong · Cantonese, Mandarin"],
  ["dana", "Dana Levi", "Tel Aviv · Hebrew, English"],
  ["elif", "Elif Yılmaz", "Istanbul · Turkish, German"],
  ["finn", "Finn Andersen", "Copenhagen · Danish, English"],
  ["gabriela", "Gabriela Ruiz", "Mexico City · Spanish"],
  ["hiro", "Hiro Tanaka", "Kyoto · Japanese, English"],
  ["isla", "Isla MacLeod", "Edinburgh · English, Gaelic"],
  ["jonas", "Jonas Weber", "Zurich · German, French"],
  ["kavya", "Kavya Rao", "Bengaluru · Kannada, Hindi"],
  ["lucas", "Lucas Moreau", "Lyon · French"],
  ["mei", "Mei Lin", "Singapore · English, Malay"],
  ["noah", "Noah Mensah", "Accra · English, Twi"],
  ["olivia", "Olivia Brown", "Cape Town · English, Afrikaans"],
  ["pablo", "Pablo Díaz", "Buenos Aires · Spanish"],
].map(([id, name, desc]) => ({ id, name, desc }));

// Trip amenities — a small, fixed set (toggle chips / checklist territory)
export const amenities = [
  ["wifi", "Wi-Fi"], ["breakfast", "Breakfast included"], ["airport", "Airport transfer"], ["pool", "Pool"],
  ["spa", "Spa"], ["gym", "Gym"], ["pets", "Pet friendly"], ["parking", "Free parking"],
  ["kids", "Kids club"], ["accessible", "Step-free access"], ["ev", "EV charging"], ["laundry", "Laundry"],
].map(([id, name]) => ({ id, name }));

// Free-form-ish tags with an existing vocabulary (creatable)
export const tags = ["adventure", "beach", "city break", "culture", "family", "food & wine", "hiking", "honeymoon", "luxury", "nightlife", "road trip", "safari", "skiing", "wellness", "wildlife"];

// Airports grouped by region, for grouped / tree selection
const regions = {
  Africa: ["CAI", "CPT", "JNB", "NBO"],
  Americas: ["BOG", "EZE", "GRU", "JFK", "LAX", "MEX", "ORD", "YVR"],
  "Asia & Pacific": ["AKL", "BKK", "DEL", "DXB", "HKG", "HND", "NRT", "SIN", "SYD"],
  Europe: ["AMS", "ATH", "BCN", "CDG", "FCO", "IST", "KEF", "LHR", "LIS", "MAD", "ZRH"],
};
export const airportsByRegion = Object.entries(regions).map(([region, codes]) => ({
  region,
  airports: codes.map((c) => airports.find((a) => a.code === c)).sort((a, b) => a.city.localeCompare(b.city)),
}));
export const airport = (code) => airports.find((a) => a.code === code);
export const guide = (id) => guides.find((g) => g.id === id);

// A fake server: filters, pages and waits, so remote-search behavior (spinners, Load more) is visible
export function fakeFetch(list, term, { fields, page = 0, pageSize = 10, delay = 350 } = {}) {
  const t = (term ?? "").trim().toLowerCase();
  const hits = t ? list.filter((x) => fields.some((f) => String(x[f]).toLowerCase().includes(t))) : list;
  return new Promise((resolve) =>
    setTimeout(() => resolve({ items: hits.slice(page * pageSize, (page + 1) * pageSize), total: hits.length, hasMore: hits.length > (page + 1) * pageSize }), delay),
  );
}

// ---------------------------------------------------------------- Item parts, matching the single lookup prototypes
export const initials = (name) => {
  const w = name.split(/\s+/);
  return (w.length > 1 ? w[0][0] + w[1][0] : name.slice(0, 2)).toUpperCase();
};
const photos = {
  LIS: "../../docs/DocsSamples/wwwroot/cities/lisbon.jpg",
  CPT: "../../docs/DocsSamples/wwwroot/cities/cape-town.jpg",
};
// size: "sm" | "default" | "lg"
export const avatar = (name, size = "sm", src = null) => `
  <span data-slot="avatar" data-size="${size}" class="sa-avatar group/avatar">
    ${src ? `<img data-slot="avatar-image" class="sa-avatar-image" src="${src}" alt="" />` : `<span data-slot="avatar-fallback" class="sa-avatar-fallback">${initials(name)}</span>`}
  </span>`;
export const airportAvatar = (a, size = "sm") => avatar(a.city, size, photos[a.code]);
// Large = the size-7 square used with a description; small = the inline h-5 chip
export const codeChip = (code, large = false) =>
  `<span class="bg-muted text-foreground flex shrink-0 items-center justify-center rounded-md font-mono font-medium ${large ? "size-7 text-[0.65rem]" : "h-5 px-1.5 text-[0.65rem]"}">${code}</span>`;

export const button = (variant, size, body, attrs = "", cls = "") =>
  `<button type="button" ${attrs} class="sa-button group/button sa-button-variant-${variant} sa-button-size-${size} ${cls}" data-slot="button">${body}</button>`;

export const badge = (variant, body, cls = "") =>
  `<span data-slot="badge" class="sa-badge sa-badge-variant-${variant} ${cls}">${body}</span>`;

// The markup <sa-input type="checkbox"> emits; set .indeterminate on the input from script for a partial group
export const checkbox = (checked, attrs = "") => `
  <span class="sa-input-control-wrapper">
    <input type="checkbox" ${checked ? "checked" : ""} ${attrs} data-slot="input" class="sa-checkbox peer" />
    <span class="sa-checkbox-indicator">${svg('<path d="M20 6 9 17l-5-5"/>', "sa-checkbox-indicator-icon")}</span>
  </span>`;

// A vertical form field the way the dashboard renders it: label, control, description
export const field = (label, control, description = "", attrs = "") => `
  <div data-slot="field" data-orientation="vertical" class="sa-field group/field min-w-0" ${attrs}>
    <label data-slot="field-label" class="sa-label sa-field-label">${label}</label>
    ${control}
    ${description ? `<p data-slot="field-description" class="sa-field-description">${description}</p>` : ""}
  </div>`;

// ---------------------------------------------------------------- Build effort
// How much of an example already ships. Levels 1–4 fill a meter; "vendor" is the Tom Select route.
export const effortLevels = {
  1: ["Exists today", "Ships now; configuration or a new editor option at most."],
  2: ["Small", "Assembles shipped components; a new editor template plus light script."],
  3: ["Medium", "Shipped components plus new client behaviour (a new web component or a substantial script)."],
  4: ["Large", "A new component with its own interaction model, built mostly from scratch."],
  vendor: ["Third-party", "Tom Select does the behaviour; our work is the styling shim and the editor around it."],
};
const meter = (level) =>
  level === "vendor"
    ? `<span class="border-foreground/60 inline-flex h-4 items-center rounded-sm border border-dashed px-1 font-mono text-[0.6rem]">npm</span>`
    : `<span class="inline-flex gap-0.5" aria-hidden="true">${[1, 2, 3, 4].map((i) => `<span class="h-3 w-1.5 rounded-[1px] ${i <= level ? "bg-foreground" : "bg-foreground/15"}"></span>`).join("")}</span>`;

// data: "choices" = a fixed list posted as a scalar collection (binding exists);
//       "entity"  = a collection navigation such as Tour.Guides (needs the shared backend work)
const dataNotes = {
  choices: "Fixed choices bound to a scalar collection: the binding already exists (CheckboxGroup editor).",
  entity: "Entity collection: also needs the shared backend work for many-to-many fields (see the note at the top).",
  both: "Fixed choices work today; an entity collection also needs the shared backend work (see the note at the top).",
};

export const effort = ({ level, uses = [], needs = [], data, note }) => {
  const [name, desc] = effortLevels[level];
  return `
  <div data-effort="${level}" class="bg-muted/40 @container space-y-3 rounded-lg border p-4 text-sm">
    <div class="flex flex-wrap items-center gap-x-3 gap-y-1">
      <span class="text-muted-foreground text-[0.65rem] font-medium tracking-wide uppercase">Build effort</span>
      ${meter(level)}
      <span class="font-semibold">${name}</span>
      <span class="text-muted-foreground text-xs">${desc}</span>
    </div>
    <div class="grid gap-4 @md:grid-cols-2">
      <div class="space-y-1.5">
        <p class="text-muted-foreground text-xs font-medium">Already there</p>
        <ul class="space-y-1">${uses.map((u) => `<li class="flex gap-2">${baseIcon.check("text-muted-foreground mt-0.5 size-3.5 shrink-0")}<span>${u}</span></li>`).join("") || `<li class="text-muted-foreground">Nothing specific</li>`}</ul>
      </div>
      <div class="space-y-1.5">
        <p class="text-muted-foreground text-xs font-medium">New work</p>
        <ul class="space-y-1">${needs.map((n) => `<li class="flex gap-2"><span class="text-muted-foreground mt-px w-3.5 shrink-0 text-center font-mono">+</span><span>${n}</span></li>`).join("") || `<li class="text-muted-foreground">None beyond wiring it into an editor</li>`}</ul>
      </div>
    </div>
    ${data || note ? `<p class="text-muted-foreground border-t pt-2 text-xs">${[data ? dataNotes[data] : "", note ?? ""].filter(Boolean).join(" ")}</p>` : ""}
  </div>`;
};

// Once per page: what any entity-backed multiselect needs regardless of the editor chosen, plus the meter legend
export const backendNote = () => `
  <aside class="bg-muted/40 max-w-4xl space-y-3 rounded-lg border p-4 text-sm">
    <p class="font-semibold">Reading the build-effort boxes</p>
    <ul class="grid gap-x-6 gap-y-1 sm:grid-cols-2">
      ${Object.entries(effortLevels).map(([k, [n, d]]) => `<li class="flex items-start gap-2"><span class="mt-0.5 shrink-0">${meter(k === "vendor" ? k : Number(k))}</span><span><strong>${n}</strong> · <span class="text-muted-foreground">${d}</span></span></li>`).join("")}
    </ul>
    <p><strong>Shared backend work, whichever editor wins.</strong> <span class="text-muted-foreground">Today's Lookup editor only handles a single reference (a non-collection navigation), and nothing in the Dashboard renders a collection value. Any multiselect over entities (destinations, guides) also needs: collection navigations recognised as form fields, repeated values bound on post, the EF data source loading the current set and syncing the join on save, and a grid/detail display. That's a fixed cost on top of every “Entity collection” example below. Fixed-choice lists (amenities, weekdays) already bind through the CheckboxGroup editor.</span></p>
  </aside>`;

// A labeled exploration panel: title, rationale, the live demo, the build effort, then pros / cons
export const panel = ({ id, title, blurb, body, build, pros = [], cons = [], wide = false }) => `
  <section id="${id}" class="space-y-3 ${wide ? "lg:col-span-2" : ""}">
    <div class="space-y-1">
      <h2 class="text-lg font-semibold">${title}</h2>
      <p class="text-muted-foreground max-w-3xl text-sm">${blurb}</p>
    </div>
    <div class="bg-background rounded-xl border p-6 shadow-sm">${body}</div>
    ${build ? effort(build) : ""}
    ${pros.length || cons.length ? `
    <div class="grid gap-4 text-sm sm:grid-cols-2">
      <ul class="space-y-1">${pros.map((p) => `<li class="flex gap-2"><span class="text-emerald-600">+</span><span>${p}</span></li>`).join("")}</ul>
      <ul class="space-y-1">${cons.map((c) => `<li class="flex gap-2"><span class="text-destructive">−</span><span>${c}</span></li>`).join("")}</ul>
    </div>` : ""}
  </section>`;

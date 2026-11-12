// Deterministic checks of /admin: access for anonymous, non-Staff and Staff users, and the seeded bookings.
// Usage: node checks.mjs <base-url>    Prints JSON.
const base = process.argv[2];
const origin = new URL(base).origin;

async function signIn(roles) {
  const response = await fetch(`${base}/dev-login?name=sam&roles=${roles}`, { redirect: "manual" });
  return response.headers.getSetCookie().map((cookie) => cookie.split(";")[0]).join("; ");
}

// Follows redirects within the app; returns the final status, path and body.
async function get(path, cookie) {
  let url = new URL(path, base);
  for (let hop = 0; hop < 10; hop++) {
    const response = await fetch(url, { redirect: "manual", headers: cookie ? { cookie } : {} });
    const location = response.headers.get("location");
    if (response.status >= 300 && response.status < 400 && location) {
      url = new URL(location, url);
      if (url.origin !== origin) return { status: response.status, path: url.href, body: "" };
      continue;
    }
    return { status: response.status, path: url.pathname, body: await response.text() };
  }
  return { status: 0, path: url.pathname, body: "" };
}

const isAdmin = (result) => result.status === 200 && result.path.toLowerCase().startsWith("/admin");
const refused = (result) => !isAdmin(result);

// Pages linked from /admin, two levels deep, skipping anything that might change data on GET
async function crawl(cookie) {
  const seen = new Map();
  let frontier = ["/admin"];
  for (let depth = 0; depth < 3 && frontier.length; depth++) {
    const next = [];
    for (const path of frontier) {
      if (seen.has(path) || seen.size >= 60) continue;
      const result = await get(path, cookie);
      seen.set(path, result);
      if (!isAdmin(result)) continue;
      for (const [, href] of result.body.matchAll(/href="([^"#]+)"/g)) {
        const url = new URL(href.replaceAll("&amp;", "&"), new URL(path, base));
        const link = url.pathname + url.search;
        if (url.origin === origin && /^\/admin(\/|$|\?)/i.test(link) && !/delete|remove|logout|signout/i.test(link)) next.push(link);
      }
    }
    frontier = next;
  }
  return [...seen].filter(([, result]) => isAdmin(result)).map(([path, result]) => ({ path, body: result.body }));
}

const checks = [];
const check = (id, title, pass, note) => checks.push({ id, title, result: pass ? "pass" : "fail", note });

const staff = await signIn("Staff");
const sales = await signIn("Sales");
const home = await get("/admin", staff);
check("D1", "Staff can open /admin", isAdmin(home), `${home.status} at ${home.path}`);
const anonymous = await get("/admin");
check("D2", "Anonymous users are refused at /admin", refused(anonymous), `${anonymous.status} at ${anonymous.path}`);
const other = await get("/admin", sales);
check("D3", "Signed-in users without Staff are refused at /admin", refused(other), `${other.status} at ${other.path}`);

const pages = isAdmin(home) ? await crawl(staff) : [];
const seeded = pages.find((page) => /VT-10\d\d/.test(page.body));
check("D4", "The seeded bookings appear in the admin area", Boolean(seeded), seeded ? `on ${seeded.path}` : `not on any of ${pages.length} pages`);
const open = [];
for (const page of pages) {
  if (!refused(await get(page.path))) open.push(`${page.path} (anonymous)`);
  if (!refused(await get(page.path, sales))) open.push(`${page.path} (without Staff)`);
}
check("D5", "Every admin page found is refused to anonymous users and users without Staff", pages.length > 0 && open.length === 0, open.length ? open.join(", ") : `${pages.length} pages checked`);

console.log(JSON.stringify({ build: true, checks, pages: pages.map((page) => page.path) }, null, 2));

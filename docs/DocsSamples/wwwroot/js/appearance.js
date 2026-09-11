(() => {
  const stylesheet = document.getElementById("docs-sample-theme");
  const themes = stylesheet.dataset.themes.split(",");
  const query = new URLSearchParams(location.search);
  const keys = { theme: "docsSamples.theme", mode: "docsSamples.mode" };

  function readPreference(name) {
    try {
      return localStorage.getItem(keys[name]);
    } catch {
      return null;
    }
  }

  function resolve(name, values, fallback) {
    const override = query.get(name);
    const saved = readPreference(name);
    return values.includes(override) ? override : values.includes(saved) ? saved : fallback;
  }

  let theme = resolve("theme", themes, "ledger");
  let mode = resolve("mode", ["light", "dark"], "light");

  function apply() {
    const url = new URL(stylesheet.href);
    const pathname = `/_content/StellarAdmin.TagHelpers/stellar-admin.${theme}.css`;
    if (url.pathname !== pathname) {
      url.pathname = pathname;
      url.search = "";
      stylesheet.href = url.href;
    }
    document.documentElement.classList.toggle("dark", mode === "dark");
    document.documentElement.style.colorScheme = mode;
  }

  function save(name, value) {
    try {
      localStorage.setItem(keys[name], value);
    } catch {
      // Controls still work when browser storage is unavailable.
    }
    const url = new URL(location.href);
    url.searchParams.delete(name);
    history.replaceState(null, "", url);
    apply();
  }

  // Run in the head so stored preferences apply before the samples render.
  apply();

  document.addEventListener("DOMContentLoaded", () => {
    const themeSelect = document.getElementById("docs-sample-theme-select");
    const darkSwitch = document.getElementById("docs-sample-dark-mode");
    if (!themeSelect || !darkSwitch) return;

    for (const value of themes) {
      themeSelect.add(new Option(value[0].toUpperCase() + value.slice(1), value));
    }
    themeSelect.value = theme;
    darkSwitch.checked = mode === "dark";

    themeSelect.addEventListener("change", () => {
      theme = themeSelect.value;
      save("theme", theme);
    });
    darkSwitch.addEventListener("change", () => {
      mode = darkSwitch.checked ? "dark" : "light";
      save("mode", mode);
    });
  });
})();

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

  let theme = resolve("theme", themes, "default");
  let mode = resolve("mode", ["system", "light", "dark"], "system");
  const systemDark = window.matchMedia("(prefers-color-scheme: dark)");

  function apply() {
    window.saLoadThemeFonts(theme);
    // The theme is a preset stylesheet linked after stellar-admin.css; the default theme has none.
    if (theme === "default") {
      stylesheet.removeAttribute("href");
    } else {
      const pathname = `/_content/StellarAdmin.TagHelpers/presets/${theme}.css`;
      if (!stylesheet.getAttribute("href") || new URL(stylesheet.href).pathname !== pathname)
        stylesheet.href = pathname;
    }
    const dark = mode === "dark" || (mode === "system" && systemDark.matches);
    document.documentElement.classList.toggle("dark", dark);
    document.documentElement.style.colorScheme = dark ? "dark" : "light";
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

  systemDark.addEventListener("change", () => {
    if (mode === "system") apply();
  });

  document.addEventListener("DOMContentLoaded", () => {
    const themeOptions = document.getElementById("docs-sample-theme-options");
    const themeLabel = document.getElementById("docs-sample-theme-label");
    const modeControl = document.getElementById("docs-sample-mode");

    if (themeOptions && themeLabel) {
      function syncTheme() {
        for (const item of themeOptions.querySelectorAll('[role="menuitemradio"]')) {
          const checked = item.dataset.value === theme;
          item.setAttribute("aria-checked", String(checked));
          item.dataset.state = checked ? "checked" : "unchecked";
          if (checked) themeLabel.textContent = item.textContent.trim();
        }
      }

      syncTheme();
      themeOptions.addEventListener("valuechange", event => {
        if (!themes.includes(event.detail.value)) return;
        theme = event.detail.value;
        syncTheme();
        save("theme", theme);
      });
    }

    if (modeControl) {
      for (const input of modeControl.querySelectorAll('input[type="radio"]')) {
        input.checked = input.value === mode;
      }
      modeControl.addEventListener("change", event => {
        if (!event.target.matches('input[type="radio"]')) return;
        mode = event.target.value;
        save("mode", mode);
      });
    }
  });
})();

// Shared by DocsSamples, DocsSamplesPro, and the exported website demos.
// Font loading belongs to the demo application; the theme files work without it.
(() => {
  // Each theme's Google Fonts family, written by the layout from the theme fixtures; a theme without
  // a web font has none.
  const families = window.saThemeFonts ?? {};

  window.saLoadThemeFonts = (theme) => {
    const family = theme in families ? families[theme] : families.default;
    if (!family) return document.getElementById("docs-theme-fonts")?.remove();
    const href = `https://fonts.googleapis.com/css2?family=${family}&display=swap`;
    let link = document.getElementById("docs-theme-fonts");
    if (!link) {
      link = document.createElement("link");
      link.id = "docs-theme-fonts";
      link.rel = "stylesheet";
      link.href = href;
      document.head.append(link);
    } else if (link.href !== href) {
      link.href = href;
    }
  };
})();

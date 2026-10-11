// Shared by DocsSamples, DocsSamplesPro, and the exported website demos.
// Font loading belongs to the demo application; the theme files work without it.
(() => {
  const families = {
    default: "Inter:wght@400..700",
    ledger: "Lexend:wght@300..700",
    ops: "IBM+Plex+Sans:wght@400;500;600;700",
    soft: "Figtree:wght@400..700",
  };

  window.saLoadThemeFonts = (theme) => {
    const family = families[theme] ?? families.default;
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

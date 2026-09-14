// Shared by DocsSamples, DocsSamplesPro, and the exported website demos.
// Font loading belongs to the demo application; theme bundles work without it.
(() => {
  const families = {
    aurora: "Archivo:wght@400;500;600&family=IBM+Plex+Mono:wght@400;500",
    concourse: "Source+Sans+3:wght@400;500;600;700&family=IBM+Plex+Mono:wght@400;500",
    ice: "IBM+Plex+Sans:wght@400;500;600&family=JetBrains+Mono:wght@400;500;700",
    ledger: "Lexend:wght@300..700&family=JetBrains+Mono:wght@400;500",
    meridian: "Instrument+Sans:wght@600&family=Work+Sans:wght@400;500;600&family=JetBrains+Mono:wght@400;500",
    observatory: "IBM+Plex+Sans:wght@400;500;600&family=IBM+Plex+Mono:wght@400;500",
    parallax: "Space+Grotesk:wght@400;500;600&family=JetBrains+Mono:wght@400;500",
  };

  window.saLoadThemeFonts = (theme) => {
    const family = families[theme] ?? "Geist:wght@100..900";
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

(() => {
  const dialog = document.getElementById("playground-command");
  const shortcut = document.getElementById("playground-command-shortcut");
  if (!dialog) return;

  const platform = navigator.userAgentData?.platform ?? navigator.platform;
  const mac = /mac|iphone|ipad/i.test(platform);
  if (mac && shortcut) {
    shortcut.querySelector("kbd").textContent = "⌘";
  }

  // ⌘K on macOS, Ctrl+K elsewhere, toggles the palette. Escape closes it natively.
  // Demos that handle the shortcut themselves mark the event as handled first.
  document.addEventListener("keydown", event => {
    if (event.defaultPrevented) return;
    if (event.key.toLowerCase() === "k" && (mac ? event.metaKey : event.ctrlKey) && !event.altKey && !event.shiftKey) {
      event.preventDefault();
      dialog.open ? dialog.close() : dialog.showModal();
    }
  });

  dialog.addEventListener("itemselect", () => dialog.close());
})();

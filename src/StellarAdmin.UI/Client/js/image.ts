/**
 * Behaviour of `sa-image`, wherever it is on the page: the loading, loaded and broken states, the
 * lightbox and the hover preview. Load and error events don't bubble, so they are caught on the way
 * down; images that finished before this script ran are settled when it starts.
 */

const imageOf = (target: EventTarget | null) =>
  target instanceof HTMLImageElement ? target.closest<HTMLElement>(".sa-image") : null;

function settle(image: HTMLElement, img: HTMLImageElement) {
  if (img.naturalWidth > 0) {
    image.dataset.state = "loaded";
    return;
  }

  image.dataset.state = "error";
  if (image instanceof HTMLButtonElement) {
    image.disabled = true;
    image.setAttribute("aria-label", "Couldn't load the image");
  }
}

document.addEventListener(
  "load",
  (event) => {
    const image = imageOf(event.target);
    if (image) settle(image, event.target as HTMLImageElement);
  },
  true,
);
document.addEventListener(
  "error",
  (event) => {
    const image = imageOf(event.target);
    if (image) settle(image, event.target as HTMLImageElement);
  },
  true,
);

function settleAll() {
  document
    .querySelectorAll<HTMLImageElement>('.sa-image[data-state="loading"] > img')
    .forEach((img) => img.complete && settle(img.parentElement!, img));
}
if (document.readyState === "loading") {
  document.addEventListener("DOMContentLoaded", settleAll, { once: true });
} else {
  settleAll();
}

let lightbox: HTMLDialogElement | null = null;

/** Opens the shared lightbox on an image. */
export function openLightbox(src: string, label = "Image") {
  if (!lightbox) {
    const template = document.querySelector<HTMLTemplateElement>(
      'template[data-slot="image-lightbox-template"]',
    );
    if (!template) return;
    document.body.append(template.content.cloneNode(true));
    lightbox = document.querySelector<HTMLDialogElement>("#sa-image-lightbox");
    if (!lightbox) return;
  }

  const img = lightbox.querySelector<HTMLImageElement>('[data-slot="image-lightbox-image"]')!;
  img.src = src;
  img.alt = label;
  lightbox.setAttribute("aria-label", label);
  lightbox.showModal();
}

document.addEventListener("click", (event) => {
  const image = (event.target as Element).closest?.<HTMLElement>(
    '.sa-image[data-interaction="lightbox"]',
  );
  const img = image?.querySelector("img");
  if (!image || !img || image.dataset.state === "error") return;
  openLightbox(img.currentSrc || img.src, image.getAttribute("aria-label") ?? undefined);
});

let preview: HTMLElement | null = null;

document.addEventListener("mouseover", (event) => {
  const image = (event.target as Element).closest?.<HTMLElement>(
    '.sa-image[data-interaction="preview"]',
  );
  const img = image?.querySelector("img");
  if (!image || !img || image.dataset.state !== "loaded") return;

  if (!preview) {
    preview = document.createElement("div");
    preview.className = "sa-image-preview";
    preview.setAttribute("popover", "manual");
    preview.setAttribute("aria-hidden", "true");
    preview.append(document.createElement("img"));
    document.body.append(preview);
  }

  preview.querySelector("img")!.src = img.currentSrc || img.src;
  preview.showPopover();

  // Beside the thumbnail, on whichever side has room, and kept inside the viewport.
  const rect = image.getBoundingClientRect();
  const { offsetWidth: width, offsetHeight: height } = preview;
  const gap = 8;
  const left =
    rect.right + gap + width <= window.innerWidth ? rect.right + gap : rect.left - gap - width;
  const top = Math.min(Math.max(gap, rect.top - gap), window.innerHeight - height - gap);
  preview.style.left = `${Math.max(gap, left)}px`;
  preview.style.top = `${top}px`;

  image.addEventListener("mouseleave", () => preview?.hidePopover(), { once: true });
});

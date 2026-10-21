import { defineNodeView } from "prosekit/core";
import { UploadTask } from "prosekit/extensions/file";

/**
 * Upload settings from the editor element's data attributes.
 *
 * @typedef {object} UploadOptions
 * @property {string} url
 * @property {string[]} accept
 * @property {number} maxFileSize
 * @property {() => string | undefined} token
 */

/** @param {number} bytes */
const formatSize = (bytes) =>
  bytes >= 1024 * 1024
    ? `${Math.round((bytes / 1024 / 1024) * 10) / 10} MB`
    : `${Math.max(1, Math.round(bytes / 1024))} KB`;

/**
 * Why a file can't be uploaded, or null when it can.
 *
 * @param {File} file
 * @param {UploadOptions} options
 */
export function rejectReason(file, options) {
  const type = file.type.toLowerCase();
  const typeMatches = options.accept.some((accept) =>
    accept.endsWith("/*") ? type.startsWith(accept.slice(0, -1)) : accept === type,
  );
  if (!typeMatches) return `${file.name} isn't a supported image.`;
  if (file.size > options.maxFileSize) {
    return `${file.name} is larger than the ${formatSize(options.maxFileSize)} limit.`;
  }
  return null;
}

/**
 * Posts the file as `file`, with the form's antiforgery token, and resolves with the `url` the
 * endpoint returns. An `error` in the response rejects with that message.
 *
 * @param {UploadOptions} options
 * @returns {import("prosekit/extensions/file").Uploader<string>}
 */
export function createUploader(options) {
  return ({ file, onProgress }) =>
    new Promise((resolve, reject) => {
      const body = new FormData();
      body.append("file", file);
      const token = options.token();
      if (token) body.append("__RequestVerificationToken", token);

      const request = new XMLHttpRequest();
      request.open("POST", options.url);
      request.responseType = "json";
      if (token) request.setRequestHeader("RequestVerificationToken", token);
      request.upload.addEventListener("progress", (event) => {
        if (event.lengthComputable) onProgress({ loaded: event.loaded, total: event.total });
      });
      request.addEventListener("load", () => {
        const response = /** @type {{ url?: string; error?: string } | null} */ (request.response);
        if (request.status >= 200 && request.status < 300 && response?.url) resolve(response.url);
        else reject(new Error(response?.error ?? `${file.name} couldn't be uploaded.`));
      });
      request.addEventListener("error", () =>
        reject(new Error(`${file.name} couldn't be uploaded.`)),
      );
      request.send(body);
    });
}

/**
 * The image node view: the image in ProseKit's resizable element with a handle to drag, and the
 * upload's progress while the image still has its temporary URL.
 */
export function defineImageView() {
  return defineNodeView({
    name: "image",
    constructor: (node, view, getPos) => {
      const root =
        /** @type {HTMLElement & { width: number | null; height: number | null; aspectRatio: number | null }} */ (
          document.createElement("prosekit-resizable-root")
        );
      root.className = "sa-rich-text-editor-image";
      root.dataset.slot = "rich-text-editor-image";

      const img = document.createElement("img");
      img.draggable = false;
      img.alt = "";
      const handle = document.createElement("prosekit-resizable-handle");
      handle.setAttribute("position", "bottom-right");
      handle.className = "sa-rich-text-editor-image-handle";
      const progress = document.createElement("span");
      progress.className = "sa-rich-text-editor-image-progress";
      progress.hidden = true;
      root.append(img, handle, progress);

      /** @type {VoidFunction | undefined} */
      let unsubscribe;
      /** @param {import("prosekit/extensions/image").ImageAttrs} attrs */
      const render = (attrs) => {
        if (img.getAttribute("src") !== attrs.src) {
          img.src = attrs.src ?? "";
          unsubscribe?.();
          unsubscribe = undefined;
          const task = attrs.src ? UploadTask.get(attrs.src) : undefined;
          progress.hidden = !task;
          if (task) {
            progress.textContent = "Uploading…";
            unsubscribe = task.subscribeProgress(({ loaded, total }) => {
              progress.textContent = `Uploading ${Math.round((loaded / total) * 100)}%`;
            });
            task.finished.finally(() => (progress.hidden = true)).catch(() => {});
          }
        }
        root.width = attrs.width ?? null;
        root.height = attrs.height ?? null;
      };
      render(/** @type {import("prosekit/extensions/image").ImageAttrs} */ (node.attrs));
      img.addEventListener("load", () => {
        if (!root.aspectRatio && img.naturalWidth) {
          root.aspectRatio = img.naturalWidth / img.naturalHeight;
        }
      });

      root.addEventListener("resizeEnd", (event) => {
        const { width, height } = /** @type {CustomEvent<{ width: number; height: number }>} */ (
          event
        ).detail;
        const pos = getPos();
        if (pos == null) return;
        view.dispatch(
          view.state.tr.setNodeMarkup(pos, undefined, {
            ...node.attrs,
            width: Math.round(width),
            height: Math.round(height),
          }),
        );
      });

      return {
        dom: root,
        update: (updated) => {
          if (updated.type !== node.type) return false;
          node = updated;
          render(/** @type {import("prosekit/extensions/image").ImageAttrs} */ (updated.attrs));
          return true;
        },
        selectNode: () => root.setAttribute("data-selected", ""),
        deselectNode: () => root.removeAttribute("data-selected"),
        ignoreMutation: () => true,
        stopEvent: (event) =>
          /** @type {Element | null} */ (event.target)?.closest?.("prosekit-resizable-handle") !=
          null,
        destroy: () => unsubscribe?.(),
      };
    },
  });
}

import { openLightbox } from "../image.js";

/** @typedef {"empty" | "filled" | "uploading" | "removed"} State */
/** @typedef {{ value: string; src: string; name: string; size: number }} Image */

/** @param {number} bytes */
const formatSize = (bytes) =>
  bytes >= 1024 * 1024
    ? `${Math.round((bytes / 1024 / 1024) * 10) / 10} MB`
    : `${Math.max(1, Math.round(bytes / 1024))} KB`;

/**
 * Image upload web component. Rendered by the `sa-image-upload` tag helper.
 *
 * It renders in light DOM and works on the server-rendered parts. A picked or dropped file is
 * checked against `data-accept` and `data-max-file-size`, then posted to `data-upload-url` straight
 * away, with progress. The value the endpoint returns goes in the hidden input, which is all the form
 * posts. Replace and remove are staged until the form saves; undo puts the original value back. Parts
 * that belong to some states only name them in `data-when`, and are shown and hidden here.
 */
export class ImageUpload extends HTMLElement {
  /** @type {HTMLInputElement} */
  #file;
  /** @type {HTMLInputElement} */
  #hidden;
  #original = { value: "", src: "", name: "" };
  /** @type {Image} */
  #current = { value: "", src: "", name: "", size: 0 };
  #removed = false;
  /** @type {XMLHttpRequest | null} */
  #request = null;
  /** @type {Image | null} */
  #previous = null;
  #progress = 0;
  /** @type {string | null} */
  #error = null;
  /** @type {string | null} */
  #objectUrl = null;

  connectedCallback() {
    this.#file = /** @type {HTMLInputElement | null} */ (
      this.querySelector('[data-slot="image-upload-file"]')
    );
    this.#hidden = /** @type {HTMLInputElement | null} */ (
      this.querySelector('[data-slot="image-upload-value"]')
    );
    const img = /** @type {HTMLImageElement | null} */ (this.querySelector(".sa-image > img"));
    this.#original = {
      value: this.#hidden.value,
      src: img?.getAttribute("src") ?? "",
      name: this.dataset.fileName ?? "",
    };
    this.#current = { ...this.#original, size: 0 };

    this.addEventListener("click", this.#onClick);
    this.addEventListener("keydown", this.#onKeyDown);
    this.addEventListener("dragover", this.#onDragOver);
    this.addEventListener("dragleave", this.#onDragLeave);
    this.addEventListener("drop", this.#onDrop);
    this.#file.addEventListener("change", this.#onFileChange);
    this.#hidden.form?.addEventListener("submit", this.#onSubmit, true);
  }

  disconnectedCallback() {
    this.#request?.abort();
    this.#hidden.form?.removeEventListener("submit", this.#onSubmit, true);
    this.#revokeObjectUrl();
  }

  /** Whether a file is being uploaded. A form doesn't submit until it finishes. */
  get uploading() {
    return this.#request !== null;
  }

  get #disabled() {
    return this.hasAttribute("data-disabled");
  }

  get #layout() {
    return this.dataset.layout ?? "dropzone";
  }

  /** @returns {State} */
  get #state() {
    if (this.#request) return "uploading";
    if (this.#removed) return "removed";
    return this.#current.value ? "filled" : "empty";
  }

  /** @param {MouseEvent} event */
  #onClick = (event) => {
    const target = /** @type {Element} */ (event.target);
    const action = /** @type {HTMLElement | null} */ (target.closest("[data-action]"))?.dataset
      .action;
    if (this.#disabled) return;

    switch (action) {
      case "pick":
        this.#file.click();
        return;
      case "remove":
        this.#remove();
        return;
      case "cancel":
        this.#cancel();
        return;
      case "undo":
        this.#undo();
        return;
    }

    // The image opens in the lightbox, except in a cell, where clicking it picks a file
    const image = /** @type {HTMLElement | null} */ (target.closest(".sa-image"));
    const img = image?.querySelector("img");
    if (
      img &&
      image.closest('[data-slot="image-upload-media"]') &&
      this.#state === "filled" &&
      image.dataset.state === "loaded"
    ) {
      openLightbox(img.currentSrc || img.src);
    }
  };

  /** @param {KeyboardEvent} event */
  #onKeyDown = (event) => {
    const drop = /** @type {Element} */ (event.target).closest('[data-slot="image-upload-drop"]');
    if (!drop || this.#disabled || (event.key !== "Enter" && event.key !== " ")) return;
    event.preventDefault();
    this.#file.click();
  };

  /** @param {DragEvent} event */
  #onDragOver = (event) => {
    if (this.#disabled || !event.dataTransfer?.types.includes("Files")) return;
    event.preventDefault();
    this.toggleAttribute("data-dragover", true);
  };

  /** @param {DragEvent} event */
  #onDragLeave = (event) => {
    if (!this.contains(/** @type {Node} */ (event.relatedTarget)))
      this.removeAttribute("data-dragover");
  };

  /** @param {DragEvent} event */
  #onDrop = (event) => {
    this.removeAttribute("data-dragover");
    const file = event.dataTransfer?.files[0];
    if (this.#disabled || !file) return;
    event.preventDefault();
    this.#accept(file);
  };

  #onFileChange = () => {
    const file = this.#file.files?.[0];
    this.#file.value = "";
    if (file) this.#accept(file);
  };

  /** @param {SubmitEvent} event */
  #onSubmit = (event) => {
    if (!this.#request) return;
    event.preventDefault();
    event.stopImmediatePropagation();
    this.#error = "Wait for the upload to finish, then save again.";
    this.#sync();
  };

  /** @param {File} file */
  #accept(file) {
    const accepted = (this.dataset.accept ?? "image/*")
      .split(",")
      .map((type) => type.trim().toLowerCase());
    const type = file.type.toLowerCase();
    const typeMatches = accepted.some((accept) =>
      accept.endsWith("/*") ? type.startsWith(accept.slice(0, -1)) : accept === type,
    );
    const maxSize = Number(this.dataset.maxFileSize ?? 0);

    if (!typeMatches) {
      this.#error = `${file.name} can't be used. Choose a ${this.dataset.acceptText ?? "supported"} image.`;
    } else if (maxSize > 0 && file.size > maxSize) {
      this.#error = `${file.name} is ${formatSize(file.size)}. The limit is ${formatSize(maxSize)}.`;
    } else {
      this.#upload(file);
      return;
    }

    this.#sync();
  }

  /** @param {File} file */
  #upload(file) {
    this.#request?.abort();
    this.#previous ??= { ...this.#current };
    this.#error = null;
    this.#removed = false;
    this.#progress = 0;

    // The picked file shows straight away, while it uploads
    this.#revokeObjectUrl();
    this.#objectUrl = URL.createObjectURL(file);
    this.#current = {
      value: this.#current.value,
      src: this.#objectUrl,
      name: file.name,
      size: file.size,
    };
    this.#setImage(this.#objectUrl);

    const body = new FormData();
    body.append("file", file);
    const token = this.#antiforgeryToken();
    if (token) body.append("__RequestVerificationToken", token);

    const request = new XMLHttpRequest();
    this.#request = request;
    request.open("POST", this.dataset.uploadUrl ?? "");
    request.responseType = "json";
    if (token) request.setRequestHeader("RequestVerificationToken", token);
    request.upload.addEventListener("progress", (event) => {
      if (!event.lengthComputable) return;
      this.#progress = Math.round((event.loaded / event.total) * 100);
      this.#syncProgress();
    });
    request.addEventListener("load", () => {
      if (this.#request !== request) return;
      this.#request = null;
      const response = /** @type {{ value?: string; url?: string; error?: string } | null} */ (
        request.response
      );
      if (request.status >= 200 && request.status < 300 && response?.value) {
        this.#current.value = response.value;
        this.#previous = null;
        this.#setValue(response.value);
      } else {
        this.#restorePrevious();
        this.#error = response?.error ?? `${file.name} couldn't be uploaded.`;
      }
      this.#sync();
    });
    request.addEventListener("error", () => {
      if (this.#request !== request) return;
      this.#request = null;
      this.#restorePrevious();
      this.#error = `${file.name} couldn't be uploaded.`;
      this.#sync();
    });
    request.send(body);

    this.#sync();
  }

  #cancel() {
    this.#request?.abort();
    this.#request = null;
    this.#restorePrevious();
    this.#sync();
  }

  #remove() {
    this.#error = null;
    // In a cell the row's Cancel is the undo, so the image just goes
    if (this.#layout === "cell" || this.#current.value !== this.#original.value) {
      this.#current = { value: "", src: "", name: "", size: 0 };
      this.#setImage("");
      this.#setValue("");
      this.#removed = this.#layout !== "cell" && this.#original.value !== "";
      if (this.#removed) this.#showOriginalImage();
    } else {
      this.#removed = true;
      this.#setValue("");
    }
    this.#sync();
  }

  #undo() {
    this.#request?.abort();
    this.#request = null;
    this.#previous = null;
    this.#removed = false;
    this.#error = null;
    this.#current = { ...this.#original, size: 0 };
    this.#showOriginalImage();
    this.#setValue(this.#original.value);
    this.#sync();
  }

  #restorePrevious() {
    if (!this.#previous) return;
    this.#current = this.#previous;
    this.#previous = null;
    this.#setImage(this.#current.src);
  }

  #showOriginalImage() {
    this.#setImage(this.#original.src);
  }

  // The form's token, or outside a form (an editing table row) the nearest one around the control
  /** @returns {string | undefined} */
  #antiforgeryToken() {
    const selector = 'input[name="__RequestVerificationToken"]';
    const form = this.#hidden.form;
    if (form) return /** @type {HTMLInputElement | null} */ (form.querySelector(selector))?.value;
    for (let element = this.parentElement; element; element = element.parentElement) {
      const token = /** @type {HTMLInputElement | null} */ (element.querySelector(selector));
      if (token) return token.value;
    }
    return undefined;
  }

  /** @param {string} value */
  #setValue(value) {
    if (this.#hidden.value === value) return;
    this.#hidden.value = value;
    this.#hidden.dispatchEvent(new Event("change", { bubbles: true }));
  }

  // Points every image of the control at a new source, or empties it
  /** @param {string} src */
  #setImage(src) {
    /** @type {NodeListOf<HTMLElement>} */ (this.querySelectorAll(".sa-image")).forEach((image) => {
      let img = image.querySelector("img");
      if (!src) {
        img?.remove();
        image.dataset.state = "empty";
        return;
      }
      if (!img) {
        img = document.createElement("img");
        img.alt = "";
        image.prepend(img);
      }
      if (img.getAttribute("src") !== src) {
        image.dataset.state = "loading";
        img.src = src;
      }
    });
  }

  #revokeObjectUrl() {
    if (this.#objectUrl && this.#objectUrl !== this.#current.src) {
      URL.revokeObjectURL(this.#objectUrl);
      this.#objectUrl = null;
    }
  }

  #sync() {
    const state = this.#state;
    this.dataset.state = state;
    this.toggleAttribute("data-changed", this.#hidden.value !== this.#original.value);
    this.toggleAttribute("data-error", this.#error !== null);
    this.toggleAttribute("aria-busy", state === "uploading");

    /** @type {NodeListOf<HTMLElement>} */ (this.querySelectorAll("[data-when]")).forEach(
      (part) => {
        part.hidden = !part.dataset.when.split(" ").includes(state);
      },
    );

    const trigger = /** @type {HTMLElement | null} */ (
      this.querySelector('[data-slot="image-upload-trigger"]')
    );
    trigger?.setAttribute("aria-label", this.#current.value ? "Replace image" : "Upload image");

    this.#syncAttachment(state);
    this.#syncNote(state);
    this.#syncProgress();
  }

  /** @param {State} state */
  #syncAttachment(state) {
    const attachment = /** @type {HTMLElement | null} */ (
      this.querySelector('[data-slot="attachment"]')
    );
    if (!attachment) return;

    attachment.dataset.state = state === "uploading" ? "uploading" : "done";
    const title = /** @type {HTMLElement | null} */ (
      attachment.querySelector('[data-slot="attachment-title"]')
    );
    const description = /** @type {HTMLElement | null} */ (
      attachment.querySelector('[data-slot="attachment-description"]')
    );
    const shown = state === "removed" ? this.#original : this.#current;
    title.textContent = shown.name;

    const text =
      state === "uploading"
        ? `Uploading · ${this.#progress}%`
        : state === "removed"
          ? "Removed when you save"
          : this.#current.size > 0
            ? formatSize(this.#current.size)
            : "";
    description.textContent = text;
    description.hidden = text === "";
  }

  /** @param {State} state */
  #syncNote(state) {
    const note = /** @type {HTMLElement | null} */ (
      this.querySelector('[data-slot="image-upload-note"]')
    );
    if (!note) return;

    note.removeAttribute("data-tone");
    note.textContent = "";
    if (this.#error) {
      note.dataset.tone = "danger";
      note.textContent = this.#error;
      note.hidden = false;
      return;
    }

    // In a cell only errors show; elsewhere the note explains a staged change, or gives the hint
    if (this.#layout === "cell") {
      note.hidden = true;
      return;
    }

    const undo = () => {
      const button = document.createElement("button");
      button.type = "button";
      button.dataset.action = "undo";
      button.textContent = "Undo";
      return button;
    };

    if (state === "uploading") {
      note.textContent = `Uploading ${this.#current.name} · ${this.#progress}%`;
    } else if (state === "removed") {
      note.dataset.tone = "warning";
      note.append("The image is removed when you save. ", undo());
    } else if (this.#hidden.value !== this.#original.value && this.#original.value) {
      note.append("Replaces the current image when you save. ", undo());
    } else {
      note.textContent = note.dataset.hint ?? "";
    }

    // The attachment row says the rest itself, except how to undo a replacement
    note.hidden =
      this.#layout === "attachment" &&
      state !== "empty" &&
      !(state === "filled" && this.#hidden.value !== this.#original.value && this.#original.value);
  }

  #syncProgress() {
    const bar = /** @type {HTMLElement | null} */ (
      this.querySelector('[data-slot="image-upload-bar"]')
    );
    if (bar) bar.style.width = `${this.#progress}%`;
    if (this.#state !== "uploading") return;

    const note = /** @type {HTMLElement | null} */ (
      this.querySelector('[data-slot="image-upload-note"]')
    );
    if (note && this.#layout !== "cell" && !this.#error) {
      note.textContent = `Uploading ${this.#current.name} · ${this.#progress}%`;
    }
    const description = /** @type {HTMLElement | null} */ (
      this.querySelector('[data-slot="attachment-description"]')
    );
    if (description) description.textContent = `Uploading · ${this.#progress}%`;
  }
}

if (!customElements.get("sel-image-upload")) customElements.define("sel-image-upload", ImageUpload);

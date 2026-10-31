import "./web-components/sel-carousel.js";
import "./web-components/sel-collapsible.js";
import "./web-components/sel-collection-editor.js";
import "./web-components/sel-command.js";
import "./web-components/sel-dialog.js";
import "./web-components/sel-dropdown-menu.js";
import "./web-components/sel-icon-picker.js";
import "./web-components/sel-image-upload.js";
import "./web-components/sel-input-otp.js";
import "./web-components/sel-message-scroller.js";
import "./web-components/sel-questionnaire.js";
import "./web-components/sel-sidebar.js";
import "./web-components/sel-slider.js";
import "./web-components/sel-tab-list.js";
import "./web-components/sel-table.js";
import "./web-components/sel-tabs.js";
import "./web-components/sel-toaster.js";

import "./image.js";

import { alertDialog } from "./wrappers/alert-dialog.js";
import { dialog } from "./wrappers/dialog.js";
import { toast } from "./wrappers/toast.js";

import "interestfor";
import "./interest-fallback.js";

const stellarAdmin = { alertDialog, dialog, toast };

const page = /** @type {Window & { stellarAdmin?: typeof stellarAdmin }} */ (window);

if (page.stellarAdmin) {
  console.warn("stellar-admin-ui: window.stellarAdmin is already defined and will be overwritten.");
}

page.stellarAdmin = stellarAdmin;

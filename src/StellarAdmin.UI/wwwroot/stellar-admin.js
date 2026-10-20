import "./js/web-components/sel-carousel.js";
import "./js/web-components/sel-collapsible.js";
import "./js/web-components/sel-command.js";
import "./js/web-components/sel-dialog.js";
import "./js/web-components/sel-dropdown-menu.js";
import "./js/web-components/sel-icon-picker.js";
import "./js/web-components/sel-image-upload.js";
import "./js/web-components/sel-input-otp.js";
import "./js/web-components/sel-message-scroller.js";
import "./js/web-components/sel-questionnaire.js";
import "./js/web-components/sel-sidebar.js";
import "./js/web-components/sel-slider.js";
import "./js/web-components/sel-table.js";
import "./js/web-components/sel-tabs.js";
import "./js/web-components/sel-toaster.js";

import "./js/image.js";

import { alertDialog } from "./js/wrappers/alert-dialog.js";
import { dialog } from "./js/wrappers/dialog.js";
import { toast } from "./js/wrappers/toast.js";

import "./vendor/interestfor.js";

const stellarAdmin = { alertDialog, dialog, toast };

const page = /** @type {Window & { stellarAdmin?: typeof stellarAdmin }} */ (window);

if (page.stellarAdmin) {
  console.warn("stellar-admin-ui: window.stellarAdmin is already defined and will be overwritten.");
}

page.stellarAdmin = stellarAdmin;

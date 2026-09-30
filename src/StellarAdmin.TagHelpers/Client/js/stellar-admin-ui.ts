import "./web-components/sel-carousel";
import "./web-components/sel-collapsible";
import "./web-components/sel-command";
import "./web-components/sel-dialog";
import "./web-components/sel-dropdown-menu";
import "./web-components/sel-input-otp";
import "./web-components/sel-message-scroller";
import "./web-components/sel-questionnaire";
import "./web-components/sel-sidebar";
import "./web-components/sel-slider";
import "./web-components/sel-table-selection";
import "./web-components/sel-toaster";

import { alertDialog } from "./wrappers/alert-dialog";
import { dialog } from "./wrappers/dialog";
import { toast } from "./wrappers/toast";

import "interestfor";

const stellarAdmin = { alertDialog, dialog, toast };

// augment the Window type so TS knows about window.stellarAdmin
declare global {
  interface Window {
    stellarAdmin: typeof stellarAdmin;
  }
}

if (window.stellarAdmin) {
  console.warn("stellar-admin-ui: window.stellarAdmin is already defined and will be overwritten.");
}

window.stellarAdmin = stellarAdmin;

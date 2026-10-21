namespace StellarAdmin.Diagnostics;

// How much of what the app configured a problem cost, which sets its colour in the sheet and its log level
internal enum ConfigurationProblemLevel
{
    // The whole resource is unavailable: it is hidden from the sidebar, and its pages return 404
    Error,

    // Something the app configured doesn't happen
    Warning,

    // What the app configured happens, and the panel adjusted something next to it
    Info,
}

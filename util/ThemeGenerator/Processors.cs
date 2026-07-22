using System.Text.RegularExpressions;

namespace ThemeGenerator;

public static partial class Processors
{
    [GeneratedRegex(@"(\s)(?<dark>dark:)?(aria\-invalid:)(?<class>\S+)")]
    public static partial Regex AriaInvalidClassRegex();

    [GeneratedRegex(@"\saria-invalid:ring-(\[)?[1-9](px\])?")]
    public static partial Regex AriaInvalidRingRegex();

    [GeneratedRegex(@"duration-(\w+)\s?")]
    public static partial Regex DurationRegex();

    [GeneratedRegex(@"(data-\[slot=checkbox-group\]:)(?<class>\S+)")]
    public static partial Regex FieldGroupCheckboxGroupRegex();

    [GeneratedRegex(@"(\w+-)?flex(-\w+)?\s?")]
    public static partial Regex FlexRegex();

    [GeneratedRegex(@"(?<!group-)data-\[size=(?<size>default|sm)\]:")]
    public static partial Regex SwitchTrackSizeRegex();

    [GeneratedRegex(@"grid(\s)?")]
    public static partial Regex GridRegex();

    [GeneratedRegex(@"data-((open)|(closed)|(\[state=delayed-open])|(\[side=\w+])):\S+\s?")]
    public static partial Regex PopoverContentDataAnimationClasses();

    [GeneratedRegex(@"\-aria$")]
    public static partial Regex ReactAriaStyle();

    [GeneratedRegex(@"data-((open)|(closed)|(\[state=delayed-open])|(\[side=\w+])):\S+\s?")]
    public static partial Regex TooltipContentDataAnimationClasses();

    extension(Dictionary<string, string> input)
    {
        /// <summary>
        ///     sa-field-group contains a rule for data-slot=checkbox-group which tightens the gap between checkboxes.
        ///     We want to duplicate this rule for data-slot=radio-group so radio group spacing is also tightened up.
        /// </summary>
        public Dictionary<string, string> AddFieldRadioGroupSupport()
        {
            var tokens = new Dictionary<string, string>(input);

            if (tokens.TryGetValue("sa-field-group", out var fieldGroupClass))
            {
                foreach (Match match in FieldGroupCheckboxGroupRegex().Matches(fieldGroupClass))
                {
                    fieldGroupClass += $" data-[slot=radio-group]:{match.Groups["class"].Value}";
                }

                // Store the updated
                tokens["sa-field-group"] = fieldGroupClass;
            }

            return tokens;
        }

        /// <summary>
        ///     Shadcn using the aria-invalid attribute to indicate errors. We want to make copies of all those classes
        ///     and create a version that depends on whether the .input-validation-error class is added (which is what
        ///     is added by ASP.NET Core validation).
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> CreateInputValidationErrorClassesFromAriaInvalid()
        {
            var tokens = new Dictionary<string, string>();

            foreach (var (key, value) in input)
            {
                var newValue = value;

                foreach (Match match in AriaInvalidClassRegex().Matches(value))
                {
                    newValue +=
                        $" {match.Groups["dark"].Value}[&.input-validation-error]:{match.Groups["class"].Value}";
                }

                tokens.Add(key, newValue);
            }

            return tokens;
        }

        /// <summary>
        ///     Creates sa-radiobutton* styles based on the existing radio-group-item* styles that exists
        ///     in shadcn
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> CreateRadioButtonStyles()
        {
            var output = new Dictionary<string, string>(input);

            if (output.TryGetValue("sa-radio-group-item", out var radioGroupItem))
            {
                output["sa-radiobutton"] = radioGroupItem;
            }

            if (output.TryGetValue("sa-radio-group-indicator", out var radioGroupIndicator))
            {
                output["sa-radiobutton-indicator"] = radioGroupIndicator;
            }

            if (
                output.TryGetValue("sa-radio-group-indicator-icon", out var radioGroupIndicatorIcon)
            )
            {
                output["sa-radiobutton-indicator-icon"] = radioGroupIndicatorIcon;
            }

            return output;
        }

        public Dictionary<string, string> DropReactAriaStyles()
        {
            var tokens = new Dictionary<string, string>();

            foreach (var (key, value) in input)
            {
                if (!ReactAriaStyle().IsMatch(key))
                {
                    tokens.Add(key, AriaInvalidRingRegex().Replace(value, string.Empty));
                }
            }

            return tokens;
        }

        /// <summary>
        ///     Remove the ring around inputs that are in an error state. The ring is confusing as it is the same
        ///     ring used when an input has focus, so the presence of this ring makes it difficult to see when an
        ///     input that is in error state has input focus
        /// </summary>
        public Dictionary<string, string> RemoveAriaInvalidRing()
        {
            var tokens = new Dictionary<string, string>();

            foreach (var (key, value) in input)
            {
                tokens.Add(key, AriaInvalidRingRegex().Replace(value, string.Empty));
            }

            return tokens;
        }

        /// <summary>
        ///     Emits the tokens that back StellarAdmin.UI's per-instance menu color / appearance / accent
        ///     settings. Only <c>sa-menu-translucent</c> exists as a <c>.cn-*</c> block upstream
        ///     (extracted verbatim); the inverted and bold-accent treatments are TS transforms in
        ///     the shadcn CLI, so we synthesise them here:
        ///     <list type="bullet">
        ///         <item>
        ///             <c>sa-menu-inverted</c> — shadcn rewrites its <c>cn-menu-target</c> marker
        ///             to the <c>dark</c> class; putting <c>dark</c> on the surface makes it (and
        ///             its items) resolve dark color variables.
        ///         </item>
        ///         <item>
        ///             <c>sa-menu-accent-bold</c> — shadcn remaps <c>--accent</c> to
        ///             <c>--primary</c> for the whole theme; we scope the same var override to the
        ///             menu surface so <c>focus:bg-accent</c> items highlight in the primary color.
        ///         </item>
        ///     </list>
        ///     The Default / Solid / Subtle states need no token (they are the absence of one).
        /// </summary>
        public Dictionary<string, string> CreateMenuSurfaceStyles()
        {
            var output = new Dictionary<string, string>(input)
            {
                ["sa-menu-inverted"] = "dark",
                ["sa-menu-accent-bold"] =
                    "[--accent:var(--primary)] [--accent-foreground:var(--primary-foreground)]",
            };

            return output;
        }

        public Dictionary<string, string> CleanDialogClasses()
        {
            var output = new Dictionary<string, string>(input);

            if (output.TryGetValue("sa-dialog-content", out var classes))
            {
                // The native dialog element controls it visibility by itself. The grid utility will force it to be
                // always visible, so we add it only when the data-open attribute is present (i.e. the dialog is open)
                classes = classes.Replace("grid", "data-open:grid");

                output["sa-dialog-content"] = classes;
            }

            return output;
        }

        /// <summary>
        ///     Shadcn popover animations were specified using a number of data classes. Since we are using the native
        ///     popover APIs, animations are declared differently and these are not required anymore.
        /// </summary>
        public Dictionary<string, string> CleanPopoverClasses()
        {
            var output = new Dictionary<string, string>(input);

            if (output.TryGetValue("sa-popover-content", out var classes))
            {
                classes = PopoverContentDataAnimationClasses().Replace(classes, string.Empty);
                classes = FlexRegex().Replace(classes, string.Empty);
                classes = DurationRegex().Replace(classes, string.Empty);

                output["sa-popover-content"] = classes;
            }

            return output;
        }

        public Dictionary<string, string> CleanSheetClasses()
        {
            var output = new Dictionary<string, string>(input);

            if (output.TryGetValue("sa-sheet-content", out var classes))
            {
                classes = classes.Replace("z-50 flex flex-col ", string.Empty);

                output["sa-sheet-content"] = classes;
            }

            return output;
        }

        /// <summary>
        ///     Shadcn tooltip animations were specified using a number of data classes. Since we are using the native
        ///     popover APIs, animations are declared differently and these are not required anymore.
        /// </summary>
        public Dictionary<string, string> CleanTooltipClasses()
        {
            var output = new Dictionary<string, string>(input);

            if (output.TryGetValue("sa-tooltip-content", out var classes))
            {
                classes = TooltipContentDataAnimationClasses().Replace(classes, string.Empty);
                classes = FlexRegex().Replace(classes, string.Empty);
                classes = DurationRegex().Replace(classes, string.Empty);

                output["sa-tooltip-content"] = classes;
            }

            return output;
        }

        /// <summary>
        ///     sa-checkbox uses the data-checked attribute to style the checked state since that is what is
        ///     being used by BaseUI. but since we use the native checkbox we should use the standard checked
        ///     pseudo class instead.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> ReplaceDuiCheckboxDataChecked()
        {
            var tokens = new Dictionary<string, string>(input);

            if (tokens.TryGetValue("sa-checkbox", out var classes))
            {
                tokens["sa-checkbox"] = classes.Replace("data-checked:", "checked:");
            }

            return tokens;
        }

        /// <summary>
        ///     sa-checkbox uses the data-checked attribute to style the checked state since that is what is
        ///     being used by BaseUI. but since we use the native checkbox we should use the standard checked
        ///     pseudo class instead.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> ReplaceDuiRadioGroupItemDataChecked()
        {
            var tokens = new Dictionary<string, string>(input);

            if (tokens.TryGetValue("sa-radio-group-item", out var classes))
            {
                tokens["sa-radio-group-item"] = classes.Replace("data-checked:", "checked:");
            }

            return tokens;
        }

        /// <summary>
        ///     sa-switch uses the data-checked/data-unchecked attributes to style the on/off state since that is
        ///     what is being used by BaseUI. Since we render a native checkbox as the switch track (so the value
        ///     posts back), we drive the state purely from CSS instead:
        ///     <list type="bullet">
        ///         <item>
        ///             The track (sa-switch) is the &lt;input&gt; itself, so it reacts to its own
        ///             <c>checked</c> pseudo class. The unchecked styles become the default (the prefix is dropped).
        ///             It also sizes off the wrapper's <c>data-size</c> (group/switch) since the input does not
        ///             carry the size attribute.
        ///         </item>
        ///         <item>
        ///             The thumb (sa-switch-thumb) is rendered as a sibling after the peer &lt;input&gt;, so it
        ///             reacts via <c>peer-checked</c>.
        ///         </item>
        ///     </list>
        /// </summary>
        public Dictionary<string, string> CleanSwitchClasses()
        {
            var tokens = new Dictionary<string, string>(input);

            if (tokens.TryGetValue("sa-switch", out var trackClasses))
            {
                trackClasses = trackClasses.Replace("data-unchecked:", string.Empty);
                trackClasses = trackClasses.Replace("data-checked:", "checked:");
                trackClasses = SwitchTrackSizeRegex()
                    .Replace(trackClasses, "group-data-[size=${size}]/switch:");

                tokens["sa-switch"] = trackClasses;
            }

            if (tokens.TryGetValue("sa-switch-thumb", out var thumbClasses))
            {
                thumbClasses = thumbClasses.Replace("data-unchecked:", string.Empty);
                thumbClasses = thumbClasses.Replace("data-checked:", "peer-checked:");

                tokens["sa-switch-thumb"] = thumbClasses;
            }

            return tokens;
        }

        /// <summary>
        ///     Shadcn drives the toggle on/off state from the element itself (<c>aria-pressed</c> for a
        ///     single toggle, <c>data-[state=on]</c> for a toggle group item) and expects focus/validation
        ///     styles on that same element. StellarAdmin.UI renders a toggle as a &lt;label&gt; wrapping an
        ///     <c>sr-only</c> native &lt;input&gt; (so the value posts back with no JavaScript), which means
        ///     the checked/focus/validation state lives on a descendant. We rewrite those prefixes to the
        ///     <c>has-*</c> forms so the wrapping label reacts to its inner input.
        /// </summary>
        public Dictionary<string, string> CleanToggleClasses()
        {
            var tokens = new Dictionary<string, string>(input);

            static string Adapt(string classes) =>
                classes
                    .Replace("aria-pressed:", "has-[:checked]:")
                    .Replace("data-[state=on]:", "has-[:checked]:")
                    .Replace("focus-visible:", "has-[:focus-visible]:")
                    .Replace("[&.input-validation-error]:", "has-[.input-validation-error]:")
                    .Replace("aria-invalid:", "has-[[aria-invalid]]:");

            if (tokens.TryGetValue("sa-toggle", out var toggleClasses))
            {
                tokens["sa-toggle"] = Adapt(toggleClasses);
            }

            if (tokens.TryGetValue("sa-toggle-group-item", out var itemClasses))
            {
                tokens["sa-toggle-group-item"] = Adapt(itemClasses);
            }

            return tokens;
        }
    }
}

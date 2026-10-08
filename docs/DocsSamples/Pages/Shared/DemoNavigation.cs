namespace DocsSamples.Pages.Shared;

// The demo pages, shared by the sidebar navigation and the command palette.
internal static class DemoNavigation
{
    public static readonly DemoGroup[] Groups =
    [
        new DemoGroup(
            "Showcases",
            [
                new DemoItem("Masonry", "/Showcase/Masonry"),
                new DemoItem("Theme Showcase", "/Showcase/ThemeShowcase"),
            ]
        ),
        new DemoGroup(
            "Layout",
            [
                new DemoItem("Card", "/Card/Index"),
                new DemoItem("Group", "/Group/Index"),
                new DemoItem("Page Container", "/PageContainer/Index"),
                new DemoItem("Separator", "/Separator/Index"),
                new DemoItem("Stack", "/Stack/Index"),
            ]
        ),
        new DemoGroup(
            "Form Layout",
            [
                new DemoItem("Form Row", "/FormRow/Index"),
                new DemoItem("Form Section", "/FormSection/Index"),
            ]
        ),
        new DemoGroup(
            "Forms",
            [
                new DemoItem("Checkbox", "/Checkbox/Index"),
                new DemoItem("Checkbox Group", "/CheckboxGroup/Index"),
                new DemoItem("Field", "/Field/Index"),
                new DemoItem("Input", "/Input/Index"),
                new DemoItem("Input Group", "/InputGroup/Index"),
                new DemoItem("Input OTP", "/InputOtp/Index"),
                new DemoItem("Segmented Control", "/SegmentedControl/Index"),
                new DemoItem("Label", "/Label/Index"),
                new DemoItem("Questionnaire", "/Questionnaire/Index"),
                new DemoItem("Radio", "/Radio/Index"),
                new DemoItem("Radio Group", "/RadioGroup/Index"),
                new DemoItem("Select", "/Select/Index"),
                new DemoItem("Slider", "/Slider/Index"),
                new DemoItem("Switch", "/Switch/Index"),
                new DemoItem("Textarea", "/Textarea/Index"),
                new DemoItem("Toggle", "/Toggle/Index"),
                new DemoItem("Toggle Group", "/ToggleGroup/Index"),
            ]
        ),
        new DemoGroup(
            "Actions",
            [
                new DemoItem("Button", "/Button/Index"),
                new DemoItem("Button Group", "/ButtonGroup/Index"),
                new DemoItem("Link Button", "/LinkButton/Index"),
            ]
        ),
        new DemoGroup(
            "Data Display",
            [
                new DemoItem("Avatar", "/Avatar/Index"),
                new DemoItem("Badge", "/Badge/Index"),
                new DemoItem("Carousel", "/Carousel/Index"),
                new DemoItem("Chip", "/Chip/Index"),
                new DemoItem("Data Grid", "/DataGrid/Index"),
                new DemoItem("Empty", "/Empty/Index"),
                new DemoItem("Icon", "/Icon/Index"),
                new DemoItem("Item", "/Item/Index"),
                new DemoItem("Kbd", "/Kbd/Index"),
                new DemoItem("Table", "/Table/Index"),
            ]
        ),
        new DemoGroup(
            "Chat Interfaces",
            [
                new DemoItem("Attachment", "/Attachment/Index"),
                new DemoItem("Bubble", "/Bubble/Index"),
                new DemoItem("Marker", "/Marker/Index"),
                new DemoItem("Message", "/Message/Index"),
                new DemoItem("Message Scroller", "/MessageScroller/Index"),
            ]
        ),
        new DemoGroup(
            "Feedback",
            [
                new DemoItem("Alert", "/Alert/Index"),
                new DemoItem("Progress", "/Progress/Index"),
                new DemoItem("Skeleton", "/Skeleton/Index"),
                new DemoItem("Spinner", "/Spinner/Index"),
            ]
        ),
        new DemoGroup(
            "Overlays",
            [
                new DemoItem("Alert Dialog", "/AlertDialog/Index"),
                new DemoItem("Dialog", "/Dialog/Index"),
                new DemoItem("Dropdown Menu", "/DropdownMenu/Index"),
                new DemoItem("Popover", "/Popover/Index"),
                new DemoItem("Sheet", "/Sheet/Index"),
                new DemoItem("Toast", "/Toast/Index"),
                new DemoItem("Tooltip", "/Tooltip/Index"),
            ]
        ),
        new DemoGroup(
            "Navigation",
            [
                new DemoItem("Accordion", "/Accordion/Index"),
                new DemoItem("App Header", "/AppHeader/Index"),
                new DemoItem("Breadcrumb", "/Breadcrumb/Index"),
                new DemoItem("Collapsible", "/Collapsible/Index"),
                new DemoItem("Command", "/Command/Index"),
                new DemoItem("Page Header", "/PageHeader/Index"),
                new DemoItem("Pagination", "/Pagination/Index"),
                new DemoItem("Sidebar", "/Sidebar/Index"),
                new DemoItem("Tabs", "/Tabs/Index"),
            ]
        ),
        new DemoGroup(
            "JavaScript Helpers",
            [
                new DemoItem("Js/AlertDialog", "/Js/AlertDialog/Index"),
                new DemoItem("Js/Dialog", "/Js/Dialog/Index"),
            ]
        ),
        new DemoGroup(
            "Extensibility",
            [
                new DemoItem("Slot Content", "/SlotContent/Index"),
                new DemoItem("Slot Outlet", "/SlotOutlet/Index"),
                new DemoItem("Templated Tag Helper", "/TemplatedTagHelper/Index"),
            ]
        ),
        new DemoGroup(
            "Theme Overrides",
            [
                new DemoItem("Standard", "/ThemeOverride/Standard"),
                new DemoItem("Override", "/ThemeOverride/Override"),
            ]
        ),
        new DemoGroup("Others", [new DemoItem("Semantic Icons", "/Icon/Semantic")]),
    ];
}

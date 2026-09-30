namespace ComponentPlayground.Pages.Shared;

// The demo pages, shared by the sidebar navigation and the command palette.
internal static class DemoNavigation
{
    public static readonly DemoGroup[] Groups =
    [
        new DemoGroup(
            "Display",
            [
                new DemoItem("Avatar", "/Demo/Avatar"),
                new DemoItem("Badge", "/Demo/Badge"),
                new DemoItem("Card", "/Demo/Card"),
                new DemoItem("Empty", "/Demo/Empty"),
                new DemoItem("Icon", "/Demo/Icon"),
                new DemoItem("Item", "/Demo/Item"),
                new DemoItem("Kbd", "/Demo/Kbd"),
                new DemoItem("Separator", "/Demo/Separator"),
                new DemoItem("Skeleton", "/Demo/Skeleton"),
                new DemoItem("Table", "/Demo/Table"),
            ]
        ),
        new DemoGroup("Chat Interfaces", [new DemoItem("Activity Feed", "/Demo/ActivityFeed")]),
        new DemoGroup(
            "Feedback",
            [
                new DemoItem("Alert", "/Demo/Alert"),
                new DemoItem("Progress", "/Demo/Progress"),
                new DemoItem("Spinner", "/Demo/Spinner"),
                new DemoItem("Toast", "/Demo/Toast"),
            ]
        ),
        new DemoGroup(
            "Forms",
            [
                new DemoItem("Button", "/Demo/Button"),
                new DemoItem("Button Group", "/Demo/ButtonGroup"),
                new DemoItem("Checkbox", "/Demo/Checkbox"),
                new DemoItem("Field", "/Demo/Field"),
                new DemoItem("Label", "/Demo/Label"),
                new DemoItem("Input", "/Demo/Input"),
                new DemoItem("Input Group", "/Demo/InputGroup"),
                new DemoItem("Radio", "/Demo/Radio"),
                new DemoItem("Select", "/Demo/Select"),
                new DemoItem("Slider", "/Demo/Slider"),
                new DemoItem("Textarea", "/Demo/Textarea"),
                new DemoItem("Toggle", "/Demo/Toggle"),
            ]
        ),
        new DemoGroup("Layout", [new DemoItem("Group", "/Demo/Group"), new DemoItem("Stack", "/Demo/Stack")]),
        new DemoGroup(
            "Navigation",
            [
                new DemoItem("Breadcrumb", "/Demo/Breadcrumb"),
                new DemoItem("Command", "/Demo/Command"),
                new DemoItem("Pagination", "/Demo/Pagination"),
                new DemoItem("Tabs", "/Demo/Tabs"),
            ]
        ),
    ];
}

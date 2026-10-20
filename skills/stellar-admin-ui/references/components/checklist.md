---
component: Checklist
tags: [sa-checklist, sa-checklist-item, sa-checklist-item-actions, sa-checklist-item-description, sa-checklist-item-link, sa-checklist-item-status, sa-checklist-item-title]
generated: true
---

# Checklist

Renders a list of steps or tasks, each with a status.

<!-- structure:begin -->
## Composition and behavior

Put `<sa-checklist-item>` or `<sa-checklist-item-link>` rows directly in `<sa-checklist>`. A row takes its status, title and description from the `status`, `title` and `description` attributes, or from `<sa-checklist-item-status>`, `<sa-checklist-item-title>` and `<sa-checklist-item-description>` children; using an attribute and its own child part together throws. The built-in statuses are `Todo` (the default), `InProgress` and `Done`; each draws an icon plus hidden status text ("To do", "In progress", "Done") ahead of the title. For any other status (failed, warning, optional) use `<sa-checklist-item-status label="...">` with your own `<sa-icon>`, coloured with a class; `label` is required. A done title keeps its colour unless `<sa-checklist-item-title muted="true">` is set.

`<sa-checklist-item-actions>` holds trailing buttons, links, dates or avatars. Any other child of a row (an `<sa-collapsible>` toggled by an `<sa-button commandfor>` in the actions, or a nested `<sa-checklist size="ChecklistSize.Small">`) lays out under the text. There is no group part: separate groups are several checklists with ordinary headings. Wrap a progress bar or card around the checklist yourself.
<!-- structure:end -->

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-checklist>` | Renders a list of steps or tasks, each with a status. |
| `<sa-checklist-item>` | Renders a row of a checklist: its status, a title and description, and anything else the row holds. |
| `<sa-checklist-item-actions>` | Renders the area at the end of a checklist item that holds actions, such as buttons, or other trailing content. |
| `<sa-checklist-item-description>` | Renders the description of a checklist item, shown under its title. |
| `<sa-checklist-item-link>` | Renders a row of a checklist as a link, so the whole row can be clicked. |
| `<sa-checklist-item-status>` | Renders a status of your own for a checklist item, such as a warning or a failure, in place of the built-in status icon. |
| `<sa-checklist-item-title>` | Renders the title of a checklist item. |

## Attributes

### `<sa-checklist>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `divider` | `ChecklistDivider` | `Inset` | `Inset`, `FullWidth`, `None` |
| `size` | `ChecklistSize` | `Default` | `Default`, `Small` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-checklist-item>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `description` | `string` | — | — |
| `status` | `ChecklistStatus` | `Todo` | `Todo`, `InProgress`, `Done` |
| `title` | `string` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-checklist-item-link>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `description` | `string` | — | — |
| `status` | `ChecklistStatus` | `Todo` | `Todo`, `InProgress`, `Done` |
| `title` | `string` | — | — |
| `asp-action` | `string` | — | — |
| `asp-area` | `string` | — | — |
| `asp-controller` | `string` | — | — |
| `asp-fragment` | `string` | — | — |
| `asp-host` | `string` | — | — |
| `asp-page` | `string` | — | — |
| `asp-page-handler` | `string` | — | — |
| `asp-protocol` | `string` | — | — |
| `asp-route` | `string` | — | — |
| `asp-all-route-data` | `IDictionary<string, string?>` | — | — |
| `asp-route-*` | `IDictionary<string, string?>` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-checklist-item-status>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `label` | `string` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-checklist-item-title>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `muted` | `bool` | `false` | `true`, `false` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/Checklist/_Intro.cshtml`*

```razor
<sa-checklist>
    <sa-checklist-item status="ChecklistStatus.Done" title="Passport and visa"/>
    <sa-checklist-item status="ChecklistStatus.Done" title="JR Pass voucher"/>
    <sa-checklist-item status="ChecklistStatus.InProgress" title="Hiking boots, worn in"/>
    <sa-checklist-item title="Rain shell"/>
    <sa-checklist-item title="Trekking poles"/>
</sa-checklist>
```

*From `Pages/Checklist/_Descriptions.cshtml`*

```razor
@{
    var requirements = new[]
    {
        (Status: ChecklistStatus.Done, Name: "Passport valid for six months", Detail: "Expires 2029; the return is 24 April."),
        (Status: ChecklistStatus.InProgress, Name: "Visa", Detail: "Applied 3 March; usually five working days."),
        (Status: ChecklistStatus.Todo, Name: "Visit Japan Web registration", Detail: "Completes the immigration and customs forms in advance."),
    };
}
<sa-checklist>
    @foreach (var requirement in requirements)
    {
        <sa-checklist-item status="requirement.Status" title="@requirement.Name" description="@requirement.Detail"/>
    }
</sa-checklist>
```

*From `Pages/Checklist/_CustomStatus.cshtml`*

```razor
<div class="grid gap-1">
    <div class="flex items-baseline justify-between gap-2 text-sm font-semibold">
        Ready to publish?
        <span class="text-xs font-normal text-muted-foreground">Kumano Kodo, 5 days</span>
    </div>
    <sa-checklist>
        <sa-checklist-item status="ChecklistStatus.Done" title="Photos" description="8 photos, cover set."/>
        <sa-checklist-item status="ChecklistStatus.Done" title="Price per person" description="¥186,000, two to ten travellers."/>
        <sa-checklist-item title="Description" description="Too short: 40 words, needs at least 120.">
            <sa-checklist-item-status label="Failed">
                <sa-icon name="circle-x" class="text-destructive"/>
            </sa-checklist-item-status>
            <sa-checklist-item-actions>
                <sa-button-link variant="ButtonVariant.Outline" size="ButtonSize.Small" href="#">Edit description</sa-button-link>
            </sa-checklist-item-actions>
        </sa-checklist-item>
        <sa-checklist-item title="Cancellation policy" description="Uses the default policy.">
            <sa-checklist-item-status label="Needs attention">
                <sa-icon name="triangle-alert" class="text-[var(--sa-color-warning)]"/>
            </sa-checklist-item-status>
            <sa-checklist-item-actions>
                <sa-button-link variant="ButtonVariant.Ghost" size="ButtonSize.Small" href="#">Review</sa-button-link>
            </sa-checklist-item-actions>
        </sa-checklist-item>
    </sa-checklist>
</div>
```

*From `Pages/Checklist/_Link.cshtml`*

```razor
<sa-checklist>
    <sa-checklist-item-link status="ChecklistStatus.Done" href="#" title="Add your agency details" description="Name, address and IATA number.">
        <sa-checklist-item-actions>
            <sa-icon name="chevron-right" class="size-4"/>
        </sa-checklist-item-actions>
    </sa-checklist-item-link>
    <sa-checklist-item-link status="ChecklistStatus.InProgress" href="#" title="Invite your agents" description="2 of 6 seats used.">
        <sa-checklist-item-actions>
            <sa-icon name="chevron-right" class="size-4"/>
        </sa-checklist-item-actions>
    </sa-checklist-item-link>
    <sa-checklist-item-link href="#" title="Connect a payment provider" description="Needed before you take bookings.">
        <sa-checklist-item-actions>
            <sa-icon name="chevron-right" class="size-4"/>
        </sa-checklist-item-actions>
    </sa-checklist-item-link>
</sa-checklist>
```

*From `Pages/Checklist/_Expandable.cshtml`*

```razor
<div class="grid gap-3">
    <sa-progress value="40">
        <sa-progress-label>Set up your panel</sa-progress-label>
        <sa-progress-value>2 of 5</sa-progress-value>
    </sa-progress>
    <sa-checklist>
        <sa-checklist-item status="ChecklistStatus.Done" title="Connect your booking system" description="Bookings sync every 15 minutes.">
            <sa-checklist-item-actions>
                <sa-button variant="ButtonVariant.Ghost" size="ButtonSize.Small" commandfor="step-sync" command="--toggle">
                    Change
                    <sa-icon name="chevron-down" class="transition-transform in-aria-expanded:rotate-180"/>
                </sa-button>
            </sa-checklist-item-actions>
            <sa-collapsible id="step-sync" hidden="">
                <pre class="overflow-auto rounded-md bg-muted p-3 font-mono text-xs">builder.AddVoyagerSync();</pre>
            </sa-collapsible>
        </sa-checklist-item>
        <sa-checklist-item title="Require sign-in" description="Anyone who can reach the panel can edit bookings.">
            <sa-checklist-item-status label="Needs attention">
                <sa-icon name="triangle-alert" class="text-[var(--sa-color-warning)]"/>
            </sa-checklist-item-status>
            <sa-checklist-item-actions>
                <sa-button variant="ButtonVariant.Outline" size="ButtonSize.Small" commandfor="step-auth" command="--toggle">
                    Show me how
                    <sa-icon name="chevron-down" class="transition-transform in-aria-expanded:rotate-180"/>
                </sa-button>
            </sa-checklist-item-actions>
            <sa-collapsible id="step-auth">
                <pre class="overflow-auto rounded-md bg-muted p-3 font-mono text-xs">resources.RequireAuthorization("Staff");</pre>
            </sa-collapsible>
        </sa-checklist-item>
        <sa-checklist-item title="Brand the sidebar" description="Show the Voyager logo and name.">
            <sa-checklist-item-actions>
                <sa-button variant="ButtonVariant.Outline" size="ButtonSize.Small" commandfor="step-brand" command="--toggle">
                    Show me how
                    <sa-icon name="chevron-down" class="transition-transform in-aria-expanded:rotate-180"/>
                </sa-button>
            </sa-checklist-item-actions>
            <sa-collapsible id="step-brand" hidden="">
                <pre class="overflow-auto rounded-md bg-muted p-3 font-mono text-xs">admin.ConfigureBranding(b => b.Title("Voyager"));</pre>
            </sa-collapsible>
        </sa-checklist-item>
        <sa-checklist-item description="Let your coding agent build resources for you.">
            <sa-checklist-item-status label="Optional">
                <sa-icon name="circle-dashed" class="text-muted-foreground"/>
            </sa-checklist-item-status>
            <sa-checklist-item-title>
                Install the agent skills
                <sa-badge variant="BadgeVariant.Outline">Optional</sa-badge>
            </sa-checklist-item-title>
            <sa-checklist-item-actions>
                <sa-button variant="ButtonVariant.Outline" size="ButtonSize.Small" commandfor="step-skills" command="--toggle">
                    Show me how
                    <sa-icon name="chevron-down" class="transition-transform in-aria-expanded:rotate-180"/>
                </sa-button>
            </sa-checklist-item-actions>
            <sa-collapsible id="step-skills" hidden="">
                <pre class="overflow-auto rounded-md bg-muted p-3 font-mono text-xs">npx skills add stellar-admin/stellar-admin</pre>
            </sa-collapsible>
        </sa-checklist-item>
    </sa-checklist>
</div>
```

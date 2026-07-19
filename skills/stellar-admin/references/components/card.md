---
component: Card
tags: [sa-card, sa-card-action, sa-card-content, sa-card-description, sa-card-footer, sa-card-header, sa-card-title]
generated: true
---

# Card

A flexible container that groups related content, composed of a header, title, description, content, footer, and action subcomponents.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-card>` | A flexible container that groups related content, composed of a header, title, description, content, footer, and action subcomponents. |
| `<sa-card-action>` | An action region within a card header, aligned to the top-right corner; typically contains a button or other interactive control. |
| `<sa-card-content>` | The main content region of a card. |
| `<sa-card-description>` | A secondary line of muted text within a card header, describing the card's contents. |
| `<sa-card-footer>` | The footer region of a card; typically contains actions or supplementary information. |
| `<sa-card-header>` | The header region of a card; typically contains the title, description, and action. |
| `<sa-card-title>` | The title text within a card header. |

## Attributes

### `<sa-card>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `size` | `CardSize` | `Default` | `Default`, `Small` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

## Examples

*From `Pages/Card/_Intro.cshtml`*

```razor
<sa-card class="relative mx-auto w-full max-w-sm pt-0">
    <div class="absolute inset-0 z-30 aspect-video"></div>
    <img
        src="https://images.unsplash.com/photo-1563492065599-3520f775eeed?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=2274"
        alt="Photo by cartayen on Unsplash"
        title="Photo by cartayen on Unsplash"
        class="relative z-20 aspect-video w-full object-cover"
    />
    <sa-card-header>
        <sa-card-title>Bangkok, Thailand</sa-card-title>
        <sa-card-description>
            Bangkok is an exhilarating metropolis where ancient, gilded temples stand in vibrant contrast to modern
            skyscrapers, world-class street food, and electrifying nightlife.
        </sa-card-description>
    </sa-card-header>
    <sa-card-footer>
        <sa-button variant="ButtonVariant.Outline" class="w-full">
            <sa-icon name="bookmark"/>
            Bookmark
        </sa-button>
    </sa-card-footer>
</sa-card>
```

*From `Pages/Card/_MeetingNotes.cshtml`*

```razor
<sa-card class="mx-auto w-full max-w-sm">
    <sa-card-header>
        <sa-card-title>Meeting Notes</sa-card-title>
        <sa-card-description>
            Transcript from the meeting with the client.
        </sa-card-description>
        <sa-card-action>
            <sa-dropdown-menu>
                <sa-dropdown-menu-trigger variant="ButtonVariant.Ghost" size="ButtonSize.Icon">
                    <sa-icon name="ellipsis" class="text-muted-foreground"/>
                    <span class="sr-only">More options</span>
                </sa-dropdown-menu-trigger>
                <sa-dropdown-menu-content class="w-44">
                    <sa-dropdown-menu-item>
                        <sa-icon name="captions"/>
                        Transcribe
                    </sa-dropdown-menu-item>
                    <sa-dropdown-menu-separator/>
                    <sa-dropdown-menu-item>
                        <sa-icon name="copy"/>
                        Copy transcript
                    </sa-dropdown-menu-item>
                    <sa-dropdown-menu-item>
                        <sa-icon name="share"/>
                        Share notes
                    </sa-dropdown-menu-item>
                    <sa-dropdown-menu-item>
                        <sa-icon name="download"/>
                        Export as PDF
                    </sa-dropdown-menu-item>
                    <sa-dropdown-menu-separator/>
                    <sa-dropdown-menu-item variant="DropdownMenuItemVariant.Destructive">
                        <sa-icon name="trash-2"/>
                        Delete
                    </sa-dropdown-menu-item>
                </sa-dropdown-menu-content>
            </sa-dropdown-menu>
        </sa-card-action>
    </sa-card-header>
    <sa-card-content>
        <p>
            Client requested dashboard redesign with focus on mobile
            responsiveness.
        </p>
        <ol class="mt-4 flex list-decimal flex-col gap-2 pl-6">
            <li>New analytics widgets for daily/weekly metrics</li>
            <li>Simplified navigation menu</li>
            <li>Dark mode support</li>
            <li>Timeline: 6 weeks</li>
            <li>Follow-up meeting scheduled for next Tuesday</li>
        </ol>
    </sa-card-content>
    <sa-card-footer>
        <sa-avatar-group>
            <sa-avatar src="/avatars/avatar-3.jpg" />
            <sa-avatar src="/avatars/avatar-2.jpg" />
            <sa-avatar src="/avatars/avatar-1.jpg" />
            <sa-avatar-group-count>+8</sa-avatar-group-count>
        </sa-avatar-group>
    </sa-card-footer>
</sa-card>
```

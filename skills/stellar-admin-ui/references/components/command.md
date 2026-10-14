---
component: Command
tags: [sa-command, sa-command-dialog, sa-command-empty, sa-command-group, sa-command-input, sa-command-item, sa-command-link-item, sa-command-list, sa-command-separator, sa-command-shortcut]
generated: true
---

# Command

A searchable menu of commands or options, navigated with the keyboard from a single search input.

<!-- structure:begin -->
## Composition and behavior

Put one `<sa-command-input>` and one `<sa-command-list>` inside `<sa-command>`. The list holds `<sa-command-empty>`, `<sa-command-item>`/`<sa-command-link-item>` entries, optional `<sa-command-group heading="...">` sections and `<sa-command-separator>` between groups. Name the menu with `label` (a visually hidden label for the input); a placeholder is not a label. Focus stays in the input: arrows, Home/End and Ctrl+N/J/P/K move the highlighted item via `aria-activedescendant`, and Enter chooses it. `loop="true"` wraps navigation.

With the default `filter="CommandFilter.Client"`, items are fuzzy-matched on their `value` (default: their text, excluding the shortcut) plus `keywords`, ranked within their own group (groups keep their order), and non-matches, empty groups and separators are hidden. Choosing an item (click or Enter) dispatches a bubbling `itemselect` event with `event.detail.value`; link items also navigate. Use `checked="true"` for a check mark (hidden when the item has a shortcut) and move it yourself in an `itemselect` handler by setting `data-checked="true"`.

For server-side search set `filter="CommandFilter.None"`: the menu never hides or ranks items. Unrecognized attributes on `<sa-command-input>` (e.g. `name="q"`, `hx-get`, `hx-trigger="input changed delay:200ms"`, `hx-sync="this:replace"`) land on the `<input>`; target the list, whose id is the command's `id` + `-list`, and swap its contents with a partial of groups/items. The partial must include `<sa-command-empty>`, since the swap replaces it. The component re-scans after any change: new items get ids, the empty message toggles, and the first result is highlighted.

`<sa-command-dialog>` is a modal dialog around a `<sa-command>`: open it with an invoker button (`commandfor` + `command="show-modal"`) or `showModal()`. There is no built-in open shortcut; add a `keydown` listener (e.g. Ctrl/⌘+K) that ignores `event.defaultPrevented`, because Ctrl+K inside the menu means "previous item". Closing the dialog clears the search.
<!-- structure:end -->

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-command>` | A searchable menu of commands or options, navigated with the keyboard from a single search input. |
| `<sa-command-dialog>` | A modal dialog that presents a command menu, typically as a command palette. Open and close it like any other dialog, for example with an invoker button or `showModal()`. |
| `<sa-command-empty>` | Content shown in a command menu when no items match the search. |
| `<sa-command-group>` | A set of related items in a command menu, with an optional heading. The group is hidden when none of its items match the search. |
| `<sa-command-input>` | The search input of a command menu. Attributes the tag does not recognize, such as `name`, `placeholder`, or request attributes for server-side search, are rendered on the input element. |
| `<sa-command-item>` | A selectable option in a command menu. Choosing it dispatches a bubbling `itemselect` event whose detail carries the item's value. |
| `<sa-command-link-item>` | A command menu option rendered as a link, navigating to its target when chosen. |
| `<sa-command-list>` | The scrollable list of a command menu's groups and items. When the application supplies results itself, this is the element whose contents it replaces. |
| `<sa-command-separator>` | A divider between groups in a command menu. It is hidden while a search is active. |
| `<sa-command-shortcut>` | The keyboard shortcut for a command menu item, aligned to the item's end. |

## Attributes

### `<sa-command>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `filter` | `CommandFilter` | `Client` | `Client`, `None` |
| `label` | `string` | — | — |
| `loop` | `bool` | `false` | `true`, `false` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-command-dialog>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `description` | `string` | — | — |
| `show-close-button` | `bool` | `false` | `true`, `false` |
| `title` | `string` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-command-group>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `heading` | `string` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-command-item>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `checked` | `bool` | `false` | `true`, `false` |
| `disabled` | `bool` | — | `true`, `false` |
| `keywords` | `string` | — | — |
| `value` | `string` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-command-link-item>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `checked` | `bool` | `false` | `true`, `false` |
| `disabled` | `bool` | — | `true`, `false` |
| `keywords` | `string` | — | — |
| `value` | `string` | — | — |
| `asp-action` | `string` | `null` | — |
| `asp-area` | `string` | `null` | — |
| `asp-controller` | `string` | `null` | — |
| `asp-fragment` | `string` | — | — |
| `asp-host` | `string` | — | — |
| `asp-page` | `string` | `null` | — |
| `asp-page-handler` | `string` | `null` | — |
| `asp-protocol` | `string` | — | — |
| `asp-route` | `string` | `null` | — |
| `asp-all-route-data` | `IDictionary<string, string?>` | — | — |
| `asp-route-*` | `IDictionary<string, string?>` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/Command/_Intro.cshtml`*

```razor
<sa-command label="Search Voyager" class="h-auto w-full max-w-md border shadow-md">
    <sa-command-input placeholder="Type a command or search..." />
    <sa-command-list>
        <sa-command-empty>No results found.</sa-command-empty>
        <sa-command-group heading="Suggestions">
            <sa-command-item>
                <sa-icon name="plane" />
                <span>Search flights</span>
            </sa-command-item>
            <sa-command-item keywords="accommodation stay">
                <sa-icon name="bed-double" />
                <span>Find a hotel</span>
            </sa-command-item>
            <sa-command-item disabled="true">
                <sa-icon name="car" />
                <span>Rent a car</span>
            </sa-command-item>
        </sa-command-group>
        <sa-command-separator />
        <sa-command-group heading="Account">
            <sa-command-item>
                <sa-icon name="luggage" />
                <span>My trips</span>
                <sa-command-shortcut>⌘T</sa-command-shortcut>
            </sa-command-item>
            <sa-command-item>
                <sa-icon name="credit-card" />
                <span>Payment methods</span>
                <sa-command-shortcut>⌘B</sa-command-shortcut>
            </sa-command-item>
            <sa-command-item>
                <sa-icon name="settings" />
                <span>Settings</span>
                <sa-command-shortcut>⌘S</sa-command-shortcut>
            </sa-command-item>
        </sa-command-group>
    </sa-command-list>
</sa-command>
```

*From `Pages/Command/_Dialog.cshtml`*

```razor
<div class="flex justify-center">
    <sa-button variant="ButtonVariant.Outline" commandfor="--command-dialog" command="show-modal">
        Open command palette
    </sa-button>
</div>
<sa-command-dialog id="--command-dialog">
    <sa-command>
        <sa-command-input placeholder="Type a command or search..." />
        <sa-command-list>
            <sa-command-empty>No results found.</sa-command-empty>
            <sa-command-group heading="Destinations">
                <sa-command-item value="lisbon" keywords="portugal">
                    <sa-icon name="map-pin" />
                    <span>Lisbon</span>
                </sa-command-item>
                <sa-command-item value="kyoto" keywords="japan">
                    <sa-icon name="map-pin" />
                    <span>Kyoto</span>
                </sa-command-item>
                <sa-command-item value="cape-town" keywords="south africa">
                    <sa-icon name="map-pin" />
                    <span>Cape Town</span>
                </sa-command-item>
            </sa-command-group>
            <sa-command-separator />
            <sa-command-group heading="Bookings">
                <sa-command-link-item href="#">
                    <sa-icon name="ticket" />
                    <span>Upcoming bookings</span>
                </sa-command-link-item>
                <sa-command-link-item href="#">
                    <sa-icon name="calendar" />
                    <span>Travel calendar</span>
                </sa-command-link-item>
            </sa-command-group>
        </sa-command-list>
    </sa-command>
</sa-command-dialog>
```

*From `Pages/Command/_KeyboardShortcut.cshtml`*

```razor
<p class="text-muted-foreground text-center text-sm">
    Press
    <sa-kbd-group>
        <sa-kbd>⌘</sa-kbd>
        <sa-kbd>K</sa-kbd>
    </sa-kbd-group>
    or
    <sa-kbd-group>
        <sa-kbd>Ctrl</sa-kbd>
        <sa-kbd>K</sa-kbd>
    </sa-kbd-group>
    to search Voyager
</p>
<sa-command-dialog id="--command-shortcut-dialog">
    <sa-command>
        <sa-command-input placeholder="Type a command or search..." />
        <sa-command-list>
            <sa-command-empty>No results found.</sa-command-empty>
            <sa-command-group heading="Suggestions">
                <sa-command-item>
                    <sa-icon name="plane" />
                    <span>Search flights</span>
                </sa-command-item>
                <sa-command-item keywords="accommodation stay">
                    <sa-icon name="bed-double" />
                    <span>Find a hotel</span>
                </sa-command-item>
                <sa-command-item>
                    <sa-icon name="car" />
                    <span>Rent a car</span>
                </sa-command-item>
            </sa-command-group>
            <sa-command-separator />
            <sa-command-group heading="Account">
                <sa-command-item>
                    <sa-icon name="luggage" />
                    <span>My trips</span>
                    <sa-command-shortcut>⌘T</sa-command-shortcut>
                </sa-command-item>
                <sa-command-item>
                    <sa-icon name="settings" />
                    <span>Settings</span>
                    <sa-command-shortcut>⌘S</sa-command-shortcut>
                </sa-command-item>
            </sa-command-group>
        </sa-command-list>
    </sa-command>
</sa-command-dialog>
<script>
    (() => {
        const dialog = document.getElementById('--command-shortcut-dialog');

        // ⌘K on macOS, Ctrl+K elsewhere, toggles the palette. Escape closes it natively.
        // Inside the palette Ctrl+K moves to the previous item, which marks the event as handled.
        document.addEventListener('keydown', (event) => {
            if (event.defaultPrevented) {
                return;
            }
            if (event.key.toLowerCase() === 'k' && (event.metaKey || event.ctrlKey)) {
                event.preventDefault();
                dialog.open ? dialog.close() : dialog.showModal();
            }
        });

        // Close the palette once an item is chosen.
        dialog.addEventListener('itemselect', () => dialog.close());
    })();
</script>
```

*From `Pages/Command/_Checked.cshtml`*

```razor
<sa-command id="--currency-command" label="Display currency" class="h-auto w-full max-w-sm border shadow-md">
    <sa-command-input placeholder="Search currencies..." />
    <sa-command-list>
        <sa-command-empty>No currency found.</sa-command-empty>
        <sa-command-group heading="Display prices in">
            <sa-command-item value="USD" keywords="us dollar united states">
                <span>US dollar</span>
                <span class="text-muted-foreground">USD</span>
            </sa-command-item>
            <sa-command-item value="EUR" keywords="euro europe" checked="true">
                <span>Euro</span>
                <span class="text-muted-foreground">EUR</span>
            </sa-command-item>
            <sa-command-item value="GBP" keywords="pound sterling united kingdom">
                <span>British pound</span>
                <span class="text-muted-foreground">GBP</span>
            </sa-command-item>
            <sa-command-item value="JPY" keywords="yen japan">
                <span>Japanese yen</span>
                <span class="text-muted-foreground">JPY</span>
            </sa-command-item>
            <sa-command-item value="ZAR" keywords="rand south africa">
                <span>South African rand</span>
                <span class="text-muted-foreground">ZAR</span>
            </sa-command-item>
        </sa-command-group>
    </sa-command-list>
</sa-command>
<p class="text-muted-foreground text-sm" id="--currency-output">Prices are shown in EUR.</p>
<script>
    (() => {
        const command = document.getElementById('--currency-command');
        const output = document.getElementById('--currency-output');

        // "itemselect" bubbles from the chosen item; move the check mark to it.
        command.addEventListener('itemselect', (event) => {
            command.querySelectorAll('[data-checked]').forEach((item) => item.removeAttribute('data-checked'));
            event.target.setAttribute('data-checked', 'true');
            output.textContent = `Prices are shown in ${event.detail.value}.`;
        });
    })();
</script>
```

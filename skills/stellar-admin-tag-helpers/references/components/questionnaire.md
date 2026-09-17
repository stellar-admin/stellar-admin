---
component: Questionnaire
tags: [sa-questionnaire, sa-questionnaire-actions, sa-questionnaire-choice, sa-questionnaire-choice-description, sa-questionnaire-choices, sa-questionnaire-description, sa-questionnaire-error, sa-questionnaire-input, sa-questionnaire-item, sa-questionnaire-next, sa-questionnaire-previous, sa-questionnaire-progress, sa-questionnaire-skip, sa-questionnaire-submit, sa-questionnaire-title]
generated: true
---

# Questionnaire

A container for one or more questionnaire items. Place it inside your own `<form>`.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-questionnaire>` | A container for one or more questionnaire items. Place it inside your own `<form>`. |
| `<sa-questionnaire-actions>` | The row holding a questionnaire's navigation buttons. |
| `<sa-questionnaire-choice>` | A fixed answer to a questionnaire item, rendered as a native radio button — or a checkbox when the item accepts multiple answers. |
| `<sa-questionnaire-choice-description>` | Supporting text explaining a single choice. |
| `<sa-questionnaire-choices>` | The list of answers for a questionnaire item. |
| `<sa-questionnaire-description>` | Supporting text explaining a questionnaire item. |
| `<sa-questionnaire-error>` | Displays the validation message for a questionnaire item's answer. A bound item renders one automatically, so place this only to position the message yourself or to write your own text. |
| `<sa-questionnaire-input>` | A free-text answer, shown alongside an item's fixed choices. Always give it an accessible name with a visible label, `aria-label`, or `aria-labelledby`. |
| `<sa-questionnaire-item>` | A single question, with its title, description, choices, and error. Renders a `<fieldset>`. |
| `<sa-questionnaire-next>` | Moves on to the next question. Renders a submit button, so place it inside your own `<form>`. |
| `<sa-questionnaire-previous>` | Moves back to the previous question. Renders a submit button, so place it inside your own `<form>`. |
| `<sa-questionnaire-progress>` | Shows how far through a questionnaire the reader is. Supply `current` and `total` to expose it as a progress bar; supply your own content to replace the default wording. |
| `<sa-questionnaire-skip>` | Leaves the current question unanswered and moves on. Renders a submit button, so place it inside your own `<form>`. |
| `<sa-questionnaire-submit>` | Completes the questionnaire. Renders a submit button, so place it inside your own `<form>`. |
| `<sa-questionnaire-title>` | The question a questionnaire item asks. Renders the item's `<legend>`. |

## Attributes

### `<sa-questionnaire-choice>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `checked` | `bool` | — | `true`, `false` |
| `disabled` | `bool` | — | `true`, `false` |
| `shortcut` | `string` | — | — |
| `value` | `string` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-questionnaire-choices>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `shortcuts` | `QuestionnaireShortcuts` | — | `Letters`, `Numbers` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-questionnaire-input>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `asp-for` | `ModelExpression` | — | — |
| `asp-format` | `string` | — | — |
| `type` | `string` | `text` | — |
| `render-error` | `bool` | `true` | `true`, `false` |
| `replaces-choices` | `bool` | `true` | `true`, `false` |
| `value` | `string` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-questionnaire-item>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `asp-for` | `ModelExpression` | — | — |
| `multiple` | `bool` | — | `true`, `false` |
| `name` | `string` | — | — |
| `render-error` | `bool` | `true` | `true`, `false` |
| `required` | `bool` | — | `true`, `false` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-questionnaire-next>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `size` | `ButtonSize` | `Default` | `Default`, `ExtraSmall`, `Small`, `Large`, `Icon`, `IconExtraSmall`, `IconSmall`, `IconLarge` |
| `variant` | `ButtonVariant` | `Default` | `Default`, `Destructive`, `Outline`, `Secondary`, `Ghost`, `Link` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-questionnaire-previous>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `size` | `ButtonSize` | `Default` | `Default`, `ExtraSmall`, `Small`, `Large`, `Icon`, `IconExtraSmall`, `IconSmall`, `IconLarge` |
| `variant` | `ButtonVariant` | `Outline` | `Default`, `Destructive`, `Outline`, `Secondary`, `Ghost`, `Link` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-questionnaire-progress>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `current` | `int` | — | — |
| `total` | `int` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-questionnaire-skip>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `size` | `ButtonSize` | `Default` | `Default`, `ExtraSmall`, `Small`, `Large`, `Icon`, `IconExtraSmall`, `IconSmall`, `IconLarge` |
| `variant` | `ButtonVariant` | `Outline` | `Default`, `Destructive`, `Outline`, `Secondary`, `Ghost`, `Link` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-questionnaire-submit>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `size` | `ButtonSize` | `Default` | `Default`, `ExtraSmall`, `Small`, `Large`, `Icon`, `IconExtraSmall`, `IconSmall`, `IconLarge` |
| `variant` | `ButtonVariant` | `Default` | `Default`, `Destructive`, `Outline`, `Secondary`, `Ghost`, `Link` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/Questionnaire/_Intro.cshtml`*

```razor
<form method="post">
    <sa-questionnaire>
        <sa-questionnaire-item asp-for="TravelStyle" required="true">
            <sa-questionnaire-title>How would you like to travel?</sa-questionnaire-title>
            <sa-questionnaire-description>
                We will shape the rest of your itinerary around this.
            </sa-questionnaire-description>
            <sa-questionnaire-choices>
                <sa-questionnaire-choice value="direct">
                    Direct flights only
                    <sa-questionnaire-choice-description>
                        Fewest connections, usually the highest fare.
                    </sa-questionnaire-choice-description>
                </sa-questionnaire-choice>
                <sa-questionnaire-choice value="one-stop">
                    One stop is fine
                    <sa-questionnaire-choice-description>
                        A good balance of price and travel time.
                    </sa-questionnaire-choice-description>
                </sa-questionnaire-choice>
                <sa-questionnaire-choice value="cheapest">
                    Cheapest route, any stops
                    <sa-questionnaire-choice-description>
                        Longer journeys in exchange for the lowest fare.
                    </sa-questionnaire-choice-description>
                </sa-questionnaire-choice>
            </sa-questionnaire-choices>
        </sa-questionnaire-item>
    </sa-questionnaire>
</form>
```

*From `Pages/Questionnaire/_Multiple.cshtml`*

```razor
<form method="post">
    <sa-questionnaire>
        <sa-questionnaire-item asp-for="Extras" multiple="true">
            <sa-questionnaire-title>What should we add to your booking?</sa-questionnaire-title>
            <sa-questionnaire-description>
                Select everything you would like included. You can remove extras later.
            </sa-questionnaire-description>
            <sa-questionnaire-choices>
                <sa-questionnaire-choice value="transfers">
                    Airport transfers
                </sa-questionnaire-choice>
                <sa-questionnaire-choice value="insurance">
                    Travel insurance
                </sa-questionnaire-choice>
                <sa-questionnaire-choice value="tours">
                    Guided day tours
                </sa-questionnaire-choice>
                <sa-questionnaire-choice value="lounge">
                    Airport lounge access
                </sa-questionnaire-choice>
            </sa-questionnaire-choices>
        </sa-questionnaire-item>
    </sa-questionnaire>
</form>
```

*From `Pages/Questionnaire/_Freeform.cshtml`*

```razor
<form method="post">
    <sa-questionnaire>
        <sa-questionnaire-item asp-for="Destination">
            <sa-questionnaire-title>Where would you like to go next?</sa-questionnaire-title>
            <sa-questionnaire-description>
                Pick one of our most requested trips, or tell us somewhere else.
            </sa-questionnaire-description>
            <sa-questionnaire-choices>
                <sa-questionnaire-choice value="kyoto">
                    Kyoto, Japan
                </sa-questionnaire-choice>
                <sa-questionnaire-choice value="patagonia">
                    Patagonia, Chile
                </sa-questionnaire-choice>
                <sa-questionnaire-choice value="lisbon">
                    Lisbon, Portugal
                </sa-questionnaire-choice>
                <sa-questionnaire-input asp-for="OtherDestination"
                                        aria-label="Somewhere else"
                                        placeholder="Somewhere else..."/>
            </sa-questionnaire-choices>
        </sa-questionnaire-item>
    </sa-questionnaire>
</form>
```

*From `Pages/Questionnaire/_Validation.cshtml`*

```razor
<form method="post">
    <sa-questionnaire>
        <sa-questionnaire-item asp-for="CabinClass" required="true">
            <sa-questionnaire-title>Which cabin class should we book?</sa-questionnaire-title>
            <sa-questionnaire-description>
                Fares are held for 20 minutes once you continue.
            </sa-questionnaire-description>
            <sa-questionnaire-choices>
                <sa-questionnaire-choice value="economy">
                    Economy
                </sa-questionnaire-choice>
                <sa-questionnaire-choice value="premium">
                    Premium economy
                </sa-questionnaire-choice>
                <sa-questionnaire-choice value="business">
                    Business
                </sa-questionnaire-choice>
            </sa-questionnaire-choices>
        </sa-questionnaire-item>
        <sa-questionnaire-item asp-for="Occasion">
            <sa-questionnaire-title>What is the trip built around?</sa-questionnaire-title>
            <sa-questionnaire-description>
                We time the itinerary around whatever you are travelling for.
            </sa-questionnaire-description>
            <sa-questionnaire-choices>
                <sa-questionnaire-choice value="festival">
                    A festival or event
                </sa-questionnaire-choice>
                <sa-questionnaire-choice value="wedding">
                    A wedding
                </sa-questionnaire-choice>
                <sa-questionnaire-choice value="birthday">
                    A big birthday
                </sa-questionnaire-choice>
                <sa-questionnaire-input asp-for="OtherOccasion"
                                        aria-label="Something else"
                                        placeholder="Something else..."/>
            </sa-questionnaire-choices>
        </sa-questionnaire-item>
    </sa-questionnaire>
</form>
```

*From `Pages/Questionnaire/_Steps.cshtml`*

```razor
<form method="post">
    <sa-questionnaire>
        <sa-questionnaire-progress current="2" total="4"/>
        <sa-questionnaire-item asp-for="Pace" required="true">
            <sa-questionnaire-title>What pace suits this trip?</sa-questionnaire-title>
            <sa-questionnaire-description>
                You can still adjust individual days once the itinerary is drafted.
            </sa-questionnaire-description>
            <sa-questionnaire-choices>
                <sa-questionnaire-choice value="relaxed">
                    Relaxed
                    <sa-questionnaire-choice-description>
                        Two or three plans a day, with time to wander.
                    </sa-questionnaire-choice-description>
                </sa-questionnaire-choice>
                <sa-questionnaire-choice value="balanced">
                    Balanced
                    <sa-questionnaire-choice-description>
                        A full morning and afternoon, evenings left open.
                    </sa-questionnaire-choice-description>
                </sa-questionnaire-choice>
                <sa-questionnaire-choice value="packed">
                    Packed
                    <sa-questionnaire-choice-description>
                        See as much as possible, early starts included.
                    </sa-questionnaire-choice-description>
                </sa-questionnaire-choice>
            </sa-questionnaire-choices>
        </sa-questionnaire-item>
        <sa-questionnaire-actions>
            <sa-questionnaire-previous name="step" value="back"/>
            <sa-questionnaire-skip name="step" value="skip"/>
            <sa-questionnaire-next name="step" value="next"/>
        </sa-questionnaire-actions>
    </sa-questionnaire>
</form>
```

*From `Pages/Questionnaire/_LongForm.cshtml`*

```razor
<form method="post">
    <sa-questionnaire>
        <sa-questionnaire-item asp-for="Budget" required="true">
            <sa-questionnaire-title>What is your budget per person?</sa-questionnaire-title>
            <sa-questionnaire-choices shortcuts="QuestionnaireShortcuts.Letters">
                <sa-questionnaire-choice value="under-2000">
                    Under $2,000
                </sa-questionnaire-choice>
                <sa-questionnaire-choice value="2000-5000">
                    $2,000 to $5,000
                </sa-questionnaire-choice>
                <sa-questionnaire-choice value="over-5000">
                    Over $5,000
                </sa-questionnaire-choice>
            </sa-questionnaire-choices>
        </sa-questionnaire-item>
        <sa-questionnaire-item asp-for="Accommodation" required="true">
            <sa-questionnaire-title>Where would you like to stay?</sa-questionnaire-title>
            <sa-questionnaire-choices shortcuts="QuestionnaireShortcuts.Letters">
                <sa-questionnaire-choice value="hotel">
                    International hotels
                </sa-questionnaire-choice>
                <sa-questionnaire-choice value="boutique">
                    Boutique guesthouses
                </sa-questionnaire-choice>
                <sa-questionnaire-choice value="apartment">
                    Self-catering apartments
                </sa-questionnaire-choice>
                <sa-questionnaire-input asp-for="OtherAccommodation"
                                        aria-label="Something else"
                                        placeholder="Something else..."/>
            </sa-questionnaire-choices>
        </sa-questionnaire-item>
        <sa-questionnaire-item asp-for="Interests" multiple="true">
            <sa-questionnaire-title>What would you like to build the trip around?</sa-questionnaire-title>
            <sa-questionnaire-choices shortcuts="QuestionnaireShortcuts.Letters">
                <sa-questionnaire-choice value="food">
                    Food and markets
                </sa-questionnaire-choice>
                <sa-questionnaire-choice value="history">
                    History and museums
                </sa-questionnaire-choice>
                <sa-questionnaire-choice value="outdoors">
                    Hiking and the outdoors
                </sa-questionnaire-choice>
            </sa-questionnaire-choices>
        </sa-questionnaire-item>
        <sa-questionnaire-actions>
            <sa-questionnaire-submit>Save preferences</sa-questionnaire-submit>
        </sa-questionnaire-actions>
    </sa-questionnaire>
</form>
```

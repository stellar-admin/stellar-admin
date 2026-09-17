---
component: FormSection
tags: [sa-form-section]
generated: true
---

# FormSection

Groups form content under a title with an optional description.

## Attributes

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `description` | `string` | — | — |
| `layout` | `FormSectionLayout` | — | `Stacked`, `Split`, `Card` |
| `title` | `string` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

## Example

*From `Pages/FormSection/_Intro.cshtml`*

```razor
<sa-form-section title="Trip details" description="How this trip appears in the Voyager catalog.">
    <sa-field-group>
        <sa-form-row>
            <sa-field>
                <sa-field-label for="split-name">Trip name</sa-field-label>
                <sa-input id="split-name" name="TripName" value="A week in Kyoto" />
                <sa-field-description>The name travelers see.</sa-field-description>
            </sa-field>
            <sa-field>
                <sa-field-label for="split-code">Booking code</sa-field-label>
                <sa-input id="split-code" name="BookingCode" value="KYO-07" />
            </sa-field>
        </sa-form-row>
        <sa-field>
            <sa-field-label for="split-description">Description</sa-field-label>
            <sa-textarea id="split-description" name="Description" rows="4" class="min-h-28">Explore Kyoto's temples, gardens, and local food.</sa-textarea>
            <sa-field-description>Describe the trip and its highlights.</sa-field-description>
        </sa-field>
    </sa-field-group>
</sa-form-section>
<sa-form-section title="Availability" description="Set the number of places travelers can book.">
    <sa-field-group>
        <sa-field>
            <sa-field-label for="split-places">Available places</sa-field-label>
            <sa-input id="split-places" name="AvailablePlaces" type="number" min="0" value="12" />
        </sa-field>
        <sa-field orientation="FieldOrientation.Horizontal">
            <sa-input type="checkbox" id="split-waitlist" name="AllowWaitlist" />
            <sa-field-content>
                <sa-field-label for="split-waitlist">Allow waitlist</sa-field-label>
                <sa-field-description>Accept requests when the trip is full.</sa-field-description>
            </sa-field-content>
        </sa-field>
    </sa-field-group>
</sa-form-section>
```

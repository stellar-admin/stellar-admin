---
component: Field
tags: [sa-field, sa-field-content, sa-field-description, sa-field-error, sa-field-group, sa-field-label, sa-field-legend, sa-field-separator, sa-field-set, sa-field-title]
generated: true
---

# Field

Renders a field that groups a form control with its label, description and error message.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-field>` | Renders a field that groups a form control with its label, description and error message. |
| `<sa-field-content>` | Renders a container that keeps a field's label and description stacked beside its control. |
| `<sa-field-description>` | Renders the help text of a field. |
| `<sa-field-error>` | Renders the validation error message of a field. |
| `<sa-field-group>` | Renders a group of related fields in a column with consistent spacing. |
| `<sa-field-label>` | Renders the label of a field's control as a `<label>` element. |
| `<sa-field-legend>` | Renders the caption of a field set as a `<legend>` element. |
| `<sa-field-separator>` | Renders a horizontal divider between fields. |
| `<sa-field-set>` | Renders a group of related fields as a `<fieldset>` element. |
| `<sa-field-title>` | Renders a title that looks like a field label but isn't tied to a control. |

## Attributes

### `<sa-field>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `column-span` | `string` | `1` | — |
| `column-span-lg` | `string` | — | — |
| `column-span-md` | `string` | — | — |
| `column-span-sm` | `string` | — | — |
| `column-start` | `string` | `auto` | — |
| `column-start-lg` | `string` | — | — |
| `column-start-md` | `string` | — | — |
| `column-start-sm` | `string` | — | — |
| `orientation` | `FieldOrientation` | `Vertical` | `Vertical`, `Horizontal` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-field-description>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `asp-for` | `ModelExpression` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-field-error>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `asp-for` | `ModelExpression` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-field-label>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `asp-for` | `ModelExpression` | — | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-field-legend>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `variant` | `FieldLegendVariant` | `Legend` | `Legend`, `Label` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/Field/_Intro.cshtml`*

```razor
<sa-field-group>
  <sa-field-set>
    <sa-field-legend>Payment Method</sa-field-legend>
    <sa-field-description>
      All transactions are secure and encrypted
    </sa-field-description>
    <sa-field-group>
      <sa-field>
        <sa-field-label for="card-name">
          Name on Card
        </sa-field-label>
        <sa-input
          id="card-name"
          placeholder="Ibn Battuta"
        />
      </sa-field>
      <sa-field>
        <sa-field-label for="card-number">
          Card Number
        </sa-field-label>
        <sa-input
          id="card-number"
          placeholder="1234 5678 9012 3456"
        />
        <sa-field-description>
          Enter your 16-digit card number
        </sa-field-description>
      </sa-field>
      <div class="grid grid-cols-3 gap-4">
        <sa-field>
          <sa-field-label for="exp-month">
            Month
          </sa-field-label>
          <sa-select id="exp-month">
            <option value="01">01</option>
            <option value="02">02</option>
            <option value="03">03</option>
            <option value="04">04</option>
            <option value="05">05</option>
            <option value="06">06</option>
            <option value="07">07</option>
            <option value="08">08</option>
            <option value="09">09</option>
            <option value="10">10</option>
            <option value="11">11</option>
            <option value="12">12</option>
          </sa-select>
        </sa-field>
        <sa-field>
          <sa-field-label for="exp-year">
            Year
          </sa-field-label>
          <sa-select id="exp-year">
            <option value="2024">2024</option>
            <option value="2025">2025</option>
            <option value="2026">2026</option>
            <option value="2027">2027</option>
            <option value="2028">2028</option>
            <option value="2029">2029</option>
          </sa-select>
        </sa-field>
        <sa-field>
          <sa-field-label for="cvv">CVV</sa-field-label>
          <sa-input id="cvv" placeholder="123"/>
        </sa-field>
      </div>
    </sa-field-group>
  </sa-field-set>
  <sa-field-separator/>
  <sa-field-set>
    <sa-field-legend>Billing Address</sa-field-legend>
    <sa-field-description>
      The billing address associated with your payment method
    </sa-field-description>
    <sa-field-group>
      <sa-field orientation="FieldOrientation.Horizontal">
        <sa-input
          type="checkbox"
          id="same-as-shipping"
        />
        <sa-field-label
          for="same-as-shipping"
          class="font-normal"
        >
          Same as shipping address
        </sa-field-label>
      </sa-field>
    </sa-field-group>
  </sa-field-set>
  <sa-field-set>
    <sa-field-group>
      <sa-field>
        <sa-field-label for="optional-comments">
          Comments
        </sa-field-label>
        <sa-textarea
          id="optional-comments"
          placeholder="Add any additional comments"
          class="resize-none"
        />
      </sa-field>
    </sa-field-group>
  </sa-field-set>
  <sa-field orientation="FieldOrientation.Horizontal">
    <sa-button type="button">Submit</sa-button>
    <sa-button variant="ButtonVariant.Outline" type="button">
      Cancel
    </sa-button>
  </sa-field>
</sa-field-group>
```

*From `Pages/Field/_Implicit.cshtml`*

```razor
@{
    var months = Enumerable.Range(1, 12).Select(i => $"{i:00}").Select(i => new SelectListItem(i, i));
    var years = Enumerable.Range(2024, 6).Select(i => new SelectListItem(i.ToString(), i.ToString()));
}

<sa-field-group>
    <sa-field-set>
        <sa-field-legend>Payment Method</sa-field-legend>
        <sa-field-description>
            All transactions are secure and encrypted
        </sa-field-description>
        <sa-field-group>
            <sa-input asp-for="Name"/>
            <sa-input asp-for="CardNumber"/>
            <div class="grid grid-cols-3 gap-4">
                <sa-select asp-for="Month" asp-items="@months" />
                <sa-select asp-for="Year" asp-items="@years" />
                <sa-input asp-for="Cvv"/>
            </div>
        </sa-field-group>
    </sa-field-set>
    <sa-field-separator/>
    <sa-field-set>
        <sa-field-legend>Billing Address</sa-field-legend>
        <sa-field-description>
            The billing address associated with your payment method
        </sa-field-description>
        <sa-field-group>
            <sa-input asp-for="IsBillingAddressSame" />
        </sa-field-group>
    </sa-field-set>
    <sa-field-set>
        <sa-field-group>
            <sa-textarea asp-for="Comments"/>
        </sa-field-group>
    </sa-field-set>
    <sa-field orientation="FieldOrientation.Horizontal">
        <sa-button type="button">Submit</sa-button>
        <sa-button variant="ButtonVariant.Outline" type="button">
            Cancel
        </sa-button>
    </sa-field>
</sa-field-group>
```

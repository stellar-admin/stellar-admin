---
component: Accordion
tags: [sa-accordion, sa-accordion-item, sa-accordion-item-content, sa-accordion-item-title]
generated: true
---

# Accordion

Renders a vertical stack of items that each expand to show their content.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-accordion>` | Renders a vertical stack of items that each expand to show their content. |
| `<sa-accordion-item>` | Renders a collapsible item in an accordion. |
| `<sa-accordion-item-content>` | Renders the content of an accordion item, shown when the item is expanded. |
| `<sa-accordion-item-title>` | Renders the title of an accordion item, which expands and collapses the item when clicked. |

## Attributes

### `<sa-accordion>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `variant` | `AccordionVariant` | `Default` | `Default`, `Contained` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-accordion-item-title>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `icon` | `string` | `null` | — |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/Accordion/_Intro.cshtml`*

```razor
<sa-accordion>
    <sa-accordion-item>
        <sa-accordion-item-title>
            What payment methods do you accept?
        </sa-accordion-item-title>
        <sa-accordion-item-content>
            We accept all major credit and debit cards (Visa, Mastercard, Amex) and popular digital wallets like Apple
            Pay and Google Pay for your convenience.
        </sa-accordion-item-content>
    </sa-accordion-item>
    <sa-accordion-item>
        <sa-accordion-item-title>
            When will I receive my booking confirmation?
        </sa-accordion-item-title>
        <sa-accordion-item-content>
            Your confirmation and e-tickets are usually emailed within minutes. If not, please check your spam folder or
            view the details in your account's "My Trips" section.
        </sa-accordion-item-content>
    </sa-accordion-item>
    <sa-accordion-item>
        <sa-accordion-item-title>
            Can I earn loyalty points or frequent flyer miles?
        </sa-accordion-item-title>
        <sa-accordion-item-content>
            Yes, you can enter your frequent flyer number during flight booking. For hotels and packages, eligibility
            depends on the specific service provider's loyalty program rules.
        </sa-accordion-item-content>
    </sa-accordion-item>
</sa-accordion>
```

*From `Pages/Accordion/_Single.cshtml`*

```razor
<sa-accordion>
    <sa-accordion-item name="faq">
        <sa-accordion-item-title>
            What payment methods do you accept?
        </sa-accordion-item-title>
        <sa-accordion-item-content>
            We accept all major credit and debit cards (Visa, Mastercard, Amex) and popular digital wallets like Apple
            Pay and Google Pay for your convenience.
        </sa-accordion-item-content>
    </sa-accordion-item>
    <sa-accordion-item name="faq">
        <sa-accordion-item-title>
            When will I receive my booking confirmation?
        </sa-accordion-item-title>
        <sa-accordion-item-content>
            Your confirmation and e-tickets are usually emailed within minutes. If not, please check your spam folder or
            view the details in your account's "My Trips" section.
        </sa-accordion-item-content>
    </sa-accordion-item>
    <sa-accordion-item name="faq">
        <sa-accordion-item-title>
            Can I earn loyalty points or frequent flyer miles?
        </sa-accordion-item-title>
        <sa-accordion-item-content>
            Yes, you can enter your frequent flyer number during flight booking. For hotels and packages, eligibility
            depends on the specific service provider's loyalty program rules.
        </sa-accordion-item-content>
    </sa-accordion-item>
</sa-accordion>
```

---
component: Accordion
tags: [sa-accordion, sa-accordion-item, sa-accordion-item-content, sa-accordion-item-title]
generated: true
---

# Accordion

A vertically stacked set of collapsible items, each of which can be expanded to reveal its content.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-accordion>` | A vertically stacked set of collapsible items, each of which can be expanded to reveal its content. |
| `<sa-accordion-item>` | A single collapsible item within an accordion, rendered as a native `<details>` element with a title and content region. |
| `<sa-accordion-item-content>` | The content region of an accordion item, revealed when the item is expanded. |
| `<sa-accordion-item-title>` | The clickable header of an accordion item that toggles the item open and closed. |

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

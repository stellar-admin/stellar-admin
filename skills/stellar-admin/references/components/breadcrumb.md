---
component: Breadcrumb
tags: [sa-breadcrumb, sa-breadcrumb-ellipsis, sa-breadcrumb-item, sa-breadcrumb-link, sa-breadcrumb-list, sa-breadcrumb-page, sa-breadcrumb-separator]
generated: true
---

# Breadcrumb

A breadcrumb navigation trail, rendered as a `<nav>`; shows the path to the current page. Compose it with the list, item, link, page, separator, and ellipsis subcomponents.

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-breadcrumb>` | A breadcrumb navigation trail, rendered as a `<nav>`; shows the path to the current page. Compose it with the list, item, link, page, separator, and ellipsis subcomponents. |
| `<sa-breadcrumb-ellipsis>` | An ellipsis that stands in for collapsed breadcrumb items, rendered as a presentational `<span>` with an icon and screen-reader text. |
| `<sa-breadcrumb-item>` | A single item within the breadcrumb trail, rendered as a `<li>`; wraps a link, page, or separator. |
| `<sa-breadcrumb-link>` | A navigable link within a breadcrumb item, rendered as an `<a>`; supports the standard anchor routing attributes. |
| `<sa-breadcrumb-list>` | The ordered list of breadcrumb items, rendered as an `<ol>`. |
| `<sa-breadcrumb-page>` | The current page in the breadcrumb trail, rendered as a non-interactive `<span>` marked with `aria-current="page"`. |
| `<sa-breadcrumb-separator>` | A visual separator between breadcrumb items, rendered as a presentational `<li>`; defaults to a chevron icon when no content is supplied. |

## Examples

*From `Pages/Breadcrumb/_Intro.cshtml`*

```razor
<sa-breadcrumb>
    <sa-breadcrumb-list>
        <sa-breadcrumb-item>
            <sa-breadcrumb-link href="#">Home</sa-breadcrumb-link>
        </sa-breadcrumb-item>
        <sa-breadcrumb-separator/>
        <sa-breadcrumb-item>
            <sa-breadcrumb-link href="#">Europe</sa-breadcrumb-link>
        </sa-breadcrumb-item>
        <sa-breadcrumb-separator/>
        <sa-breadcrumb-item>
            <sa-breadcrumb-link href="#">Italy</sa-breadcrumb-link>
        </sa-breadcrumb-item>
        <sa-breadcrumb-separator/>
        <sa-breadcrumb-page>Grand Hotel Venice</sa-breadcrumb-page>
    </sa-breadcrumb-list>
</sa-breadcrumb>
```

*From `Pages/Breadcrumb/_Collapsed.cshtml`*

```razor
<sa-breadcrumb>
    <sa-breadcrumb-list>
        <sa-breadcrumb-item>
            <sa-breadcrumb-link href="#">Home</sa-breadcrumb-link>
        </sa-breadcrumb-item>
        <sa-breadcrumb-separator/>
        <sa-breadcrumb-item>
            <sa-breadcrumb-ellipsis />
        </sa-breadcrumb-item>
        <sa-breadcrumb-separator/>
        <sa-breadcrumb-item>
            <sa-breadcrumb-link href="#">Trip #TRV-987</sa-breadcrumb-link>
        </sa-breadcrumb-item>
        <sa-breadcrumb-separator/>
        <sa-breadcrumb-page>Add Traveler Details</sa-breadcrumb-page>
    </sa-breadcrumb-list>
</sa-breadcrumb>
```

---
component: Carousel
tags: [sa-carousel, sa-carousel-content, sa-carousel-indicators, sa-carousel-item, sa-carousel-next, sa-carousel-previous]
generated: true
---

# Carousel

A scrollable collection of slides with optional navigation controls.

<!-- structure:begin -->
## Composition and behavior

Use one `<sa-carousel-content>` inside `<sa-carousel>`, with `<sa-carousel-item>` as its direct children. Previous/next buttons and optional `<sa-carousel-indicators>` are siblings of the content. Name the carousel and each slide with `aria-label` or `aria-labelledby`. Reserve horizontal space for the arrows, or vertical space plus a definite viewport height for a vertical carousel.

Orientation defaults to `CarouselOrientation.Horizontal`; buttons default to `ButtonVariant.Outline` and `ButtonSize.IconSmall`. Set `--carousel-spacing` on the root to change the gap. Responsive item basis must account for this gap. Indicators represent distinct reachable scroll positions, so their count can be smaller than the item count when several items fit at the end.

The light-DOM web component uses native scrolling and Invoker Commands. Without JavaScript or command support the content remains scrollable and navigation controls are hidden. There is no autoplay, infinite looping, mouse grab/drag, or Embla API. The viewport handles arrow keys, Home, and End when focused; it leaves keys in slide content alone and honors reduced motion. Translate the controls' `aria-label` values and provide slide labels for indicator names.
<!-- structure:end -->

## Tags

| Tag | Description |
|-----|-------------|
| `<sa-carousel>` | A scrollable collection of slides with optional navigation controls. |
| `<sa-carousel-content>` | The scrollable viewport containing carousel items. |
| `<sa-carousel-indicators>` | Navigation dots for the distinct scroll positions in a carousel. |
| `<sa-carousel-item>` | An individual slide in a carousel. |
| `<sa-carousel-next>` | A button that scrolls to the next carousel position. |
| `<sa-carousel-previous>` | A button that scrolls to the previous carousel position. |

## Attributes

### `<sa-carousel>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `orientation` | `CarouselOrientation` | — | `Horizontal`, `Vertical` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

> In Razor, enum values are written fully-qualified, e.g. `variant="ButtonVariant.Outline"`.

### `<sa-carousel-next>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `size` | `ButtonSize` | — | `Default`, `ExtraSmall`, `Small`, `Large`, `Icon`, `IconExtraSmall`, `IconSmall`, `IconLarge` |
| `variant` | `ButtonVariant` | — | `Default`, `Destructive`, `Outline`, `Secondary`, `Ghost`, `Link` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

### `<sa-carousel-previous>`

| Attribute | Type | Default | Values |
|-----------|------|---------|--------|
| `size` | `ButtonSize` | — | `Default`, `ExtraSmall`, `Small`, `Large`, `Icon`, `IconExtraSmall`, `IconSmall`, `IconLarge` |
| `variant` | `ButtonVariant` | — | `Default`, `Destructive`, `Outline`, `Secondary`, `Ghost`, `Link` |
| `class` | `string` | — | Extra Tailwind utilities; merged last, so it overrides defaults. |

## Examples

*From `Pages/Carousel/_Intro.cshtml`*

```razor
<div class="mx-auto w-full max-w-xl px-12">
    <sa-carousel aria-label="Voyager destinations">
        <sa-carousel-content>
            <sa-carousel-item aria-label="Kyoto">
                <figure class="relative w-full aspect-[3/2] min-h-56 overflow-hidden rounded-xl bg-muted">
                    <img src="/cities/kyoto.jpg" alt="Kiyomizu-dera above red autumn foliage in Kyoto" width="1200" height="800" class="absolute inset-0 h-full w-full object-cover" loading="lazy"/>
                    <figcaption class="absolute inset-x-0 bottom-0 bg-linear-to-t from-black/90 via-black/60 to-transparent px-5 pt-16 pb-5 text-white">
                        <p class="text-2xl font-semibold tracking-tight">Kyoto</p>
                        <p class="mt-1 text-sm text-white/90">Japan</p>
                        <p class="mt-3 text-xs text-white/90">Photo by <a href="https://unsplash.com/photos/oP50BOGpyj0" target="_blank" rel="noopener noreferrer" class="underline underline-offset-2 hover:text-white focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-white">Bhanu Singh</a> on Unsplash</p>
                    </figcaption>
                </figure>
            </sa-carousel-item>
            <sa-carousel-item aria-label="Lisbon">
                <figure class="relative w-full aspect-[3/2] min-h-56 overflow-hidden rounded-xl bg-muted">
                    <img src="/cities/lisbon.jpg" alt="Terracotta rooftops and historic churches in Lisbon" width="1200" height="800" class="absolute inset-0 h-full w-full object-cover" loading="lazy"/>
                    <figcaption class="absolute inset-x-0 bottom-0 bg-linear-to-t from-black/90 via-black/60 to-transparent px-5 pt-16 pb-5 text-white">
                        <p class="text-2xl font-semibold tracking-tight">Lisbon</p>
                        <p class="mt-1 text-sm text-white/90">Portugal</p>
                        <p class="mt-3 text-xs text-white/90">Photo by <a href="https://unsplash.com/photos/alj8EUjeIj4" target="_blank" rel="noopener noreferrer" class="underline underline-offset-2 hover:text-white focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-white">João Reguengos</a> on Unsplash</p>
                    </figcaption>
                </figure>
            </sa-carousel-item>
            <sa-carousel-item aria-label="Cape Town">
                <figure class="relative w-full aspect-[3/2] min-h-56 overflow-hidden rounded-xl bg-muted">
                    <img src="/cities/cape-town.jpg" alt="Waves breaking along the coast with Table Mountain beyond" width="1200" height="675" class="absolute inset-0 h-full w-full object-cover" loading="lazy"/>
                    <figcaption class="absolute inset-x-0 bottom-0 bg-linear-to-t from-black/90 via-black/60 to-transparent px-5 pt-16 pb-5 text-white">
                        <p class="text-2xl font-semibold tracking-tight">Cape Town</p>
                        <p class="mt-1 text-sm text-white/90">South Africa</p>
                        <p class="mt-3 text-xs text-white/90">Photo by <a href="https://unsplash.com/photos/JiGoRzghR3E" target="_blank" rel="noopener noreferrer" class="underline underline-offset-2 hover:text-white focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-white">Matthys Pienaar</a> on Unsplash</p>
                    </figcaption>
                </figure>
            </sa-carousel-item>
            <sa-carousel-item aria-label="Chiang Mai">
                <figure class="relative w-full aspect-[3/2] min-h-56 overflow-hidden rounded-xl bg-muted">
                    <img src="/cities/chiang-mai.jpg" alt="The ancient brick chedi of Wat Chedi Luang in Chiang Mai" width="1200" height="675" class="absolute inset-0 h-full w-full object-cover" loading="lazy"/>
                    <figcaption class="absolute inset-x-0 bottom-0 bg-linear-to-t from-black/90 via-black/60 to-transparent px-5 pt-16 pb-5 text-white">
                        <p class="text-2xl font-semibold tracking-tight">Chiang Mai</p>
                        <p class="mt-1 text-sm text-white/90">Thailand</p>
                        <p class="mt-3 text-xs text-white/90">Photo by <a href="https://unsplash.com/photos/QMGGhIkljKo" target="_blank" rel="noopener noreferrer" class="underline underline-offset-2 hover:text-white focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-white">YingChu Chen</a> on Unsplash</p>
                    </figcaption>
                </figure>
            </sa-carousel-item>
        </sa-carousel-content>
        <sa-carousel-previous/>
        <sa-carousel-next/>
    </sa-carousel>
</div>
```

*From `Pages/Carousel/_Sizes.cshtml`*

```razor
<div class="mx-auto w-full max-w-4xl px-12">
    <sa-carousel aria-label="Voyager destinations">
        <sa-carousel-content>
            <sa-carousel-item aria-label="Kyoto" class="md:basis-[calc(50%-0.5rem)]">
                <figure class="relative w-full aspect-[3/2] min-h-56 overflow-hidden rounded-xl bg-muted">
                    <img src="/cities/kyoto.jpg" alt="Kiyomizu-dera above red autumn foliage in Kyoto" width="1200" height="800" class="absolute inset-0 h-full w-full object-cover" loading="lazy"/>
                    <figcaption class="absolute inset-x-0 bottom-0 bg-linear-to-t from-black/90 via-black/60 to-transparent px-5 pt-16 pb-5 text-white">
                        <p class="text-2xl font-semibold tracking-tight">Kyoto</p>
                        <p class="mt-1 text-sm text-white/90">Japan</p>
                        <p class="mt-3 text-xs text-white/90">Photo by <a href="https://unsplash.com/photos/oP50BOGpyj0" target="_blank" rel="noopener noreferrer" class="underline underline-offset-2 hover:text-white focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-white">Bhanu Singh</a> on Unsplash</p>
                    </figcaption>
                </figure>
            </sa-carousel-item>
            <sa-carousel-item aria-label="Lisbon" class="md:basis-[calc(50%-0.5rem)]">
                <figure class="relative w-full aspect-[3/2] min-h-56 overflow-hidden rounded-xl bg-muted">
                    <img src="/cities/lisbon.jpg" alt="Terracotta rooftops and historic churches in Lisbon" width="1200" height="800" class="absolute inset-0 h-full w-full object-cover" loading="lazy"/>
                    <figcaption class="absolute inset-x-0 bottom-0 bg-linear-to-t from-black/90 via-black/60 to-transparent px-5 pt-16 pb-5 text-white">
                        <p class="text-2xl font-semibold tracking-tight">Lisbon</p>
                        <p class="mt-1 text-sm text-white/90">Portugal</p>
                        <p class="mt-3 text-xs text-white/90">Photo by <a href="https://unsplash.com/photos/alj8EUjeIj4" target="_blank" rel="noopener noreferrer" class="underline underline-offset-2 hover:text-white focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-white">João Reguengos</a> on Unsplash</p>
                    </figcaption>
                </figure>
            </sa-carousel-item>
            <sa-carousel-item aria-label="Cape Town" class="md:basis-[calc(50%-0.5rem)]">
                <figure class="relative w-full aspect-[3/2] min-h-56 overflow-hidden rounded-xl bg-muted">
                    <img src="/cities/cape-town.jpg" alt="Waves breaking along the coast with Table Mountain beyond" width="1200" height="675" class="absolute inset-0 h-full w-full object-cover" loading="lazy"/>
                    <figcaption class="absolute inset-x-0 bottom-0 bg-linear-to-t from-black/90 via-black/60 to-transparent px-5 pt-16 pb-5 text-white">
                        <p class="text-2xl font-semibold tracking-tight">Cape Town</p>
                        <p class="mt-1 text-sm text-white/90">South Africa</p>
                        <p class="mt-3 text-xs text-white/90">Photo by <a href="https://unsplash.com/photos/JiGoRzghR3E" target="_blank" rel="noopener noreferrer" class="underline underline-offset-2 hover:text-white focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-white">Matthys Pienaar</a> on Unsplash</p>
                    </figcaption>
                </figure>
            </sa-carousel-item>
            <sa-carousel-item aria-label="Chiang Mai" class="md:basis-[calc(50%-0.5rem)]">
                <figure class="relative w-full aspect-[3/2] min-h-56 overflow-hidden rounded-xl bg-muted">
                    <img src="/cities/chiang-mai.jpg" alt="The ancient brick chedi of Wat Chedi Luang in Chiang Mai" width="1200" height="675" class="absolute inset-0 h-full w-full object-cover" loading="lazy"/>
                    <figcaption class="absolute inset-x-0 bottom-0 bg-linear-to-t from-black/90 via-black/60 to-transparent px-5 pt-16 pb-5 text-white">
                        <p class="text-2xl font-semibold tracking-tight">Chiang Mai</p>
                        <p class="mt-1 text-sm text-white/90">Thailand</p>
                        <p class="mt-3 text-xs text-white/90">Photo by <a href="https://unsplash.com/photos/QMGGhIkljKo" target="_blank" rel="noopener noreferrer" class="underline underline-offset-2 hover:text-white focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-white">YingChu Chen</a> on Unsplash</p>
                    </figcaption>
                </figure>
            </sa-carousel-item>
        </sa-carousel-content>
        <sa-carousel-previous/>
        <sa-carousel-next/>
        <sa-carousel-indicators aria-label="Choose a destination"/>
    </sa-carousel>
</div>
```

*From `Pages/Carousel/_Indicators.cshtml`*

```razor
<div class="mx-auto w-full max-w-xl px-12">
    <sa-carousel aria-label="Voyager destinations">
        <sa-carousel-content>
            <sa-carousel-item aria-label="Kyoto">
                <figure class="relative w-full aspect-[3/2] min-h-56 overflow-hidden rounded-xl bg-muted">
                    <img src="/cities/kyoto.jpg" alt="Kiyomizu-dera above red autumn foliage in Kyoto" width="1200" height="800" class="absolute inset-0 h-full w-full object-cover" loading="lazy"/>
                    <figcaption class="absolute inset-x-0 bottom-0 bg-linear-to-t from-black/90 via-black/60 to-transparent px-5 pt-16 pb-5 text-white">
                        <p class="text-2xl font-semibold tracking-tight">Kyoto</p>
                        <p class="mt-1 text-sm text-white/90">Japan</p>
                        <p class="mt-3 text-xs text-white/90">Photo by <a href="https://unsplash.com/photos/oP50BOGpyj0" target="_blank" rel="noopener noreferrer" class="underline underline-offset-2 hover:text-white focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-white">Bhanu Singh</a> on Unsplash</p>
                    </figcaption>
                </figure>
            </sa-carousel-item>
            <sa-carousel-item aria-label="Lisbon">
                <figure class="relative w-full aspect-[3/2] min-h-56 overflow-hidden rounded-xl bg-muted">
                    <img src="/cities/lisbon.jpg" alt="Terracotta rooftops and historic churches in Lisbon" width="1200" height="800" class="absolute inset-0 h-full w-full object-cover" loading="lazy"/>
                    <figcaption class="absolute inset-x-0 bottom-0 bg-linear-to-t from-black/90 via-black/60 to-transparent px-5 pt-16 pb-5 text-white">
                        <p class="text-2xl font-semibold tracking-tight">Lisbon</p>
                        <p class="mt-1 text-sm text-white/90">Portugal</p>
                        <p class="mt-3 text-xs text-white/90">Photo by <a href="https://unsplash.com/photos/alj8EUjeIj4" target="_blank" rel="noopener noreferrer" class="underline underline-offset-2 hover:text-white focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-white">João Reguengos</a> on Unsplash</p>
                    </figcaption>
                </figure>
            </sa-carousel-item>
            <sa-carousel-item aria-label="Cape Town">
                <figure class="relative w-full aspect-[3/2] min-h-56 overflow-hidden rounded-xl bg-muted">
                    <img src="/cities/cape-town.jpg" alt="Waves breaking along the coast with Table Mountain beyond" width="1200" height="675" class="absolute inset-0 h-full w-full object-cover" loading="lazy"/>
                    <figcaption class="absolute inset-x-0 bottom-0 bg-linear-to-t from-black/90 via-black/60 to-transparent px-5 pt-16 pb-5 text-white">
                        <p class="text-2xl font-semibold tracking-tight">Cape Town</p>
                        <p class="mt-1 text-sm text-white/90">South Africa</p>
                        <p class="mt-3 text-xs text-white/90">Photo by <a href="https://unsplash.com/photos/JiGoRzghR3E" target="_blank" rel="noopener noreferrer" class="underline underline-offset-2 hover:text-white focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-white">Matthys Pienaar</a> on Unsplash</p>
                    </figcaption>
                </figure>
            </sa-carousel-item>
            <sa-carousel-item aria-label="Chiang Mai">
                <figure class="relative w-full aspect-[3/2] min-h-56 overflow-hidden rounded-xl bg-muted">
                    <img src="/cities/chiang-mai.jpg" alt="The ancient brick chedi of Wat Chedi Luang in Chiang Mai" width="1200" height="675" class="absolute inset-0 h-full w-full object-cover" loading="lazy"/>
                    <figcaption class="absolute inset-x-0 bottom-0 bg-linear-to-t from-black/90 via-black/60 to-transparent px-5 pt-16 pb-5 text-white">
                        <p class="text-2xl font-semibold tracking-tight">Chiang Mai</p>
                        <p class="mt-1 text-sm text-white/90">Thailand</p>
                        <p class="mt-3 text-xs text-white/90">Photo by <a href="https://unsplash.com/photos/QMGGhIkljKo" target="_blank" rel="noopener noreferrer" class="underline underline-offset-2 hover:text-white focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-white">YingChu Chen</a> on Unsplash</p>
                    </figcaption>
                </figure>
            </sa-carousel-item>
        </sa-carousel-content>
        <sa-carousel-previous/>
        <sa-carousel-next/>
        <sa-carousel-indicators aria-label="Choose a destination"/>
    </sa-carousel>
</div>
```

*From `Pages/Carousel/_Vertical.cshtml`*

```razor
<div class="mx-auto w-full max-w-xl px-12 py-12">
    <sa-carousel aria-label="Voyager destinations" orientation="CarouselOrientation.Vertical">
        <sa-carousel-content class="h-72">
            <sa-carousel-item aria-label="Kyoto">
                <figure class="relative h-full overflow-hidden rounded-xl bg-muted">
                    <img src="/cities/kyoto.jpg" alt="Kiyomizu-dera above red autumn foliage in Kyoto" width="1200" height="800" class="absolute inset-0 h-full w-full object-cover" loading="lazy"/>
                    <figcaption class="absolute inset-x-0 bottom-0 bg-linear-to-t from-black/90 via-black/60 to-transparent px-5 pt-16 pb-5 text-white">
                        <p class="text-2xl font-semibold tracking-tight">Kyoto</p>
                        <p class="mt-1 text-sm text-white/90">Japan</p>
                        <p class="mt-3 text-xs text-white/90">Photo by <a href="https://unsplash.com/photos/oP50BOGpyj0" target="_blank" rel="noopener noreferrer" class="underline underline-offset-2 hover:text-white focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-white">Bhanu Singh</a> on Unsplash</p>
                    </figcaption>
                </figure>
            </sa-carousel-item>
            <sa-carousel-item aria-label="Lisbon">
                <figure class="relative h-full overflow-hidden rounded-xl bg-muted">
                    <img src="/cities/lisbon.jpg" alt="Terracotta rooftops and historic churches in Lisbon" width="1200" height="800" class="absolute inset-0 h-full w-full object-cover" loading="lazy"/>
                    <figcaption class="absolute inset-x-0 bottom-0 bg-linear-to-t from-black/90 via-black/60 to-transparent px-5 pt-16 pb-5 text-white">
                        <p class="text-2xl font-semibold tracking-tight">Lisbon</p>
                        <p class="mt-1 text-sm text-white/90">Portugal</p>
                        <p class="mt-3 text-xs text-white/90">Photo by <a href="https://unsplash.com/photos/alj8EUjeIj4" target="_blank" rel="noopener noreferrer" class="underline underline-offset-2 hover:text-white focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-white">João Reguengos</a> on Unsplash</p>
                    </figcaption>
                </figure>
            </sa-carousel-item>
            <sa-carousel-item aria-label="Cape Town">
                <figure class="relative h-full overflow-hidden rounded-xl bg-muted">
                    <img src="/cities/cape-town.jpg" alt="Waves breaking along the coast with Table Mountain beyond" width="1200" height="675" class="absolute inset-0 h-full w-full object-cover" loading="lazy"/>
                    <figcaption class="absolute inset-x-0 bottom-0 bg-linear-to-t from-black/90 via-black/60 to-transparent px-5 pt-16 pb-5 text-white">
                        <p class="text-2xl font-semibold tracking-tight">Cape Town</p>
                        <p class="mt-1 text-sm text-white/90">South Africa</p>
                        <p class="mt-3 text-xs text-white/90">Photo by <a href="https://unsplash.com/photos/JiGoRzghR3E" target="_blank" rel="noopener noreferrer" class="underline underline-offset-2 hover:text-white focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-white">Matthys Pienaar</a> on Unsplash</p>
                    </figcaption>
                </figure>
            </sa-carousel-item>
            <sa-carousel-item aria-label="Chiang Mai">
                <figure class="relative h-full overflow-hidden rounded-xl bg-muted">
                    <img src="/cities/chiang-mai.jpg" alt="The ancient brick chedi of Wat Chedi Luang in Chiang Mai" width="1200" height="675" class="absolute inset-0 h-full w-full object-cover" loading="lazy"/>
                    <figcaption class="absolute inset-x-0 bottom-0 bg-linear-to-t from-black/90 via-black/60 to-transparent px-5 pt-16 pb-5 text-white">
                        <p class="text-2xl font-semibold tracking-tight">Chiang Mai</p>
                        <p class="mt-1 text-sm text-white/90">Thailand</p>
                        <p class="mt-3 text-xs text-white/90">Photo by <a href="https://unsplash.com/photos/QMGGhIkljKo" target="_blank" rel="noopener noreferrer" class="underline underline-offset-2 hover:text-white focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-white">YingChu Chen</a> on Unsplash</p>
                    </figcaption>
                </figure>
            </sa-carousel-item>
        </sa-carousel-content>
        <sa-carousel-previous/>
        <sa-carousel-next/>
    </sa-carousel>
</div>
```

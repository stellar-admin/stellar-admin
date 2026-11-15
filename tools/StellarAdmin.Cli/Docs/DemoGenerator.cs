using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using DocsSamples;
using Spectre.Console;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.Cli.Docs;

/// <summary>
/// Generates the DocsSamples documentation artifacts (rendered demo HTML, source-code includes, and
/// downloaded static assets) and writes them into the sibling <c>website</c> project.
/// </summary>
internal sealed partial class DemoGenerator
{
    // The theme fixtures (util/theme-check/presets) the samples serve from /presets/, exported for the
    // theme builder's presets. "default" is the token stylesheet alone, with no preset.
    private static readonly IReadOnlyList<ThemeFixture> Themes = ThemeFixtures.Load(
        DocsPaths.RepoRoot
    );

    private static readonly string[] PresetNames =
    [
        .. Themes.Select(theme => theme.Name).Where(name => name != ThemeFixtures.DefaultName),
    ];

    // Injected into the head of every exported demo page. Mirrors the docs website's light/dark
    // selection in localStorage ("theme", the next-themes key; shared because the demos are served
    // same-origin), both in the inline iframe and the full-preview tab. The storage and matchMedia
    // listeners keep already-open demos in sync with live toggles. The demos use the default theme;
    // saThemeInit, called in place of the samples' preset link (see PresetLinkRegex), loads its font.
    private const string ThemeSyncScript = """
            <script>
                (function () {
                    var media = window.matchMedia('(prefers-color-scheme: dark)');
                    function applyDark() {
                        var theme = null;
                        try { theme = localStorage.getItem('theme'); } catch (e) { }
                        var dark = theme === 'dark' || (theme !== 'light' && media.matches);
                        document.documentElement.classList.toggle('dark', dark);
                        document.documentElement.style.colorScheme = dark ? 'dark' : 'light';
                    }
                    window.saThemeInit = function () {
                        window.saLoadThemeFonts('default');
                    };
                    applyDark();
                    window.addEventListener('storage', function (e) {
                        if (e.key === null || e.key === 'theme') applyDark();
                    });
                    media.addEventListener('change', applyDark);
                })();
            </script>
        """;

    // Injected into the exported image upload and rich text image demos, which have no server to
    // upload to. It answers the demo upload handler's requests in the browser: progress over a second or so, then the
    // picked file's name as the value and an object URL for it, as the samples' handler would.
    private const string DemoUploadScript = """
            <script>
                (function () {
                    var Native = window.XMLHttpRequest;
                    class DemoUploadRequest extends Native {
                        open(method, url) {
                            this._demo = /[?&]handler=Upload\b/.test(String(url));
                            if (!this._demo) return super.open.apply(this, arguments);
                        }
                        setRequestHeader() {
                            if (!this._demo) return super.setRequestHeader.apply(this, arguments);
                        }
                        get status() { return this._demo ? this._status || 0 : super.status; }
                        get response() { return this._demo ? this._response || null : super.response; }
                        abort() {
                            if (!this._demo) return super.abort();
                            clearInterval(this._timer);
                            this.dispatchEvent(new Event('abort'));
                        }
                        send(body) {
                            if (!this._demo) return super.send(body);
                            var request = this;
                            var file = body.get('file');
                            var total = file.size || 1;
                            var loaded = 0;
                            this._timer = setInterval(function () {
                                loaded = Math.min(total, loaded + total / 8);
                                request.upload.dispatchEvent(new ProgressEvent('progress', { lengthComputable: true, loaded: loaded, total: total }));
                                if (loaded < total) return;
                                clearInterval(request._timer);
                                request._status = 200;
                                request._response = { value: file.name, url: URL.createObjectURL(file) };
                                request.dispatchEvent(new Event('load'));
                            }, 150);
                        }
                    }
                    window.XMLHttpRequest = DemoUploadRequest;
                })();
            </script>
        """;

    /// <summary>Content root of the DocsSamples web project, used to host it in-memory.</summary>
    public static readonly string DocsSamplesContentRoot = Path.Combine(
        DocsPaths.RepoRoot,
        "docs",
        "DocsSamples"
    );

    private static readonly string PageSourceCodeFolder = Path.Combine(
        DocsPaths.RepoRoot,
        "docs",
        "DocsSamples",
        "Pages"
    );

    private static readonly string RenderedPagesOutputFolder = Path.Combine(
        DocsPaths.WebsiteRoot,
        "public",
        "demo",
        "ui"
    );

    private static readonly string PagesSourceCodeOutputFolder = Path.Combine(
        DocsPaths.WebsiteRoot,
        "content",
        "docs",
        "ui",
        "components",
        "_include"
    );

    private static readonly string DownloadedAssetsOutputFolder = Path.Combine(
        DocsPaths.WebsiteRoot,
        "public",
        "demo",
        "ui",
        "assets"
    );

    // The icon picker demos' icons endpoint, and the static copy of its response that the exported
    // demos load instead, since they have no server.
    private const string DemoIconsUrl = "/IconPicker?handler=Icons";
    private const string ExportedIconsUrl = "/demo/ui/assets/icons.json";

    /// <summary>Static assets referenced by demos at fixed, known URLs.</summary>
    public static readonly string[] FixedStaticAssets =
    [
        "/avatars/avatar-1.jpg",
        "/avatars/avatar-2.jpg",
        "/avatars/avatar-3.jpg",
        "/cities/cape-town.jpg",
        "/cities/chiang-mai.jpg",
        "/cities/kyoto.jpg",
        "/cities/lisbon.jpg",
        "/gradients/gradient-1.jpg",
    ];

    /// <summary>
    /// Deletes previously generated output so renamed/removed demos don't leave orphaned files behind.
    /// Only the two top-level output trees are cleared; the assets folder is nested inside
    /// <see cref="RenderedPagesOutputFolder"/> and is removed along with it.
    /// </summary>
    public static void Clean() =>
        DocsPaths.Delete(RenderedPagesOutputFolder, PagesSourceCodeOutputFolder);

    // Renders every demo partial and its source include, and downloads the assets they use. Deletes the
    // existing demos first, unless keepOutput, so renamed or removed ones don't leave files behind.
    // Returns whether every step succeeded
    public static async Task<bool> GenerateAsync(bool keepOutput)
    {
        AnsiConsole.Write(new Rule("[bold]StellarAdmin.UI docs demos[/]").LeftJustified());
        AnsiConsole.MarkupLineInterpolated(
            $"Hosting DocsSamples from: [grey]{DocsSamplesContentRoot}[/]"
        );

        var generator = new DemoGenerator();
        using var factory = new DocsSamplesApplicationFactory(DocsSamplesContentRoot);
        using var client = factory.CreateClient();

        var runner = new StepRunner();
        if (!keepOutput)
        {
            runner.Add(
                "Clean existing demos",
                () =>
                {
                    Clean();
                    return Task.CompletedTask;
                }
            );
        }

        return await runner
            .Add(
                "Download fixed static assets",
                () => generator.DownloadFixedStaticAssetsAsync(client)
            )
            .Add(
                "Download dynamic static assets",
                () => generator.DownloadDynamicStaticAssetsAsync(client)
            )
            .Add(
                "Download preset stylesheets",
                () => generator.DownloadPresetStylesheetsAsync(client)
            )
            .Add("Download demo icons", () => generator.DownloadDemoIconsAsync(client))
            .Add(
                "Download theme variable manifest",
                () => generator.DownloadThemeVariableManifestAsync(client)
            )
            .Add(
                "Generate demos",
                async step =>
                {
                    var demoPartials = FindDemoPartials();
                    step.SetTotal(demoPartials.Count);
                    foreach (var partial in demoPartials)
                    {
                        step.Detail = partial.Name;
                        await generator.GenerateDemoPartialSourceFileAsync(partial.Name);
                        await generator.RenderDemoPartialOutputAsync(
                            client,
                            partial.Name,
                            partial.Layout,
                            partial.OutputFileName
                        );
                        step.Increment();
                    }
                }
            )
            .RunAsync();
    }

    /// <summary>Downloads the demo assets that live at fixed, known URLs.</summary>
    public async Task DownloadFixedStaticAssetsAsync(HttpClient client)
    {
        if (!Directory.Exists(DownloadedAssetsOutputFolder))
            Directory.CreateDirectory(DownloadedAssetsOutputFolder);

        foreach (var url in FixedStaticAssets)
        {
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            await File.WriteAllBytesAsync(
                Path.Combine(DownloadedAssetsOutputFolder, Path.GetFileName(url)),
                await response.Content.ReadAsByteArrayAsync()
            );
        }
    }

    /// <summary>Scrapes the index page for CSS/JS assets and downloads them.</summary>
    public async Task DownloadDynamicStaticAssetsAsync(HttpClient client)
    {
        if (!Directory.Exists(DownloadedAssetsOutputFolder))
            Directory.CreateDirectory(DownloadedAssetsOutputFolder);

        var indexPageResponse = await client.GetAsync("/?clean");
        indexPageResponse.EnsureSuccessStatusCode();

        var content = await indexPageResponse.Content.ReadAsStringAsync();

        foreach (Match match in DemoAssetRegex().Matches(content))
        {
            var filename = Path.GetFileName(match.Groups["filename"].Value);

            // Presets are downloaded separately under stable names (the theme builder reads them),
            // and the samples' own theme picker is not exported.
            if (
                match.Groups["url"].Value.Contains("/presets/")
                || filename.StartsWith("appearance.")
            )
            {
                continue;
            }

            var assetResponse = await client.GetAsync(match.Groups["url"].Value);
            assetResponse.EnsureSuccessStatusCode();

            await File.WriteAllBytesAsync(
                Path.Combine(DownloadedAssetsOutputFolder, filename),
                await assetResponse.Content.ReadAsByteArrayAsync()
            );
        }
    }

    /// <summary>
    /// Downloads every preset stylesheet under its stable, unfingerprinted name, and writes
    /// presets.json beside them: each theme's name, label and description, default first, for the
    /// website's theme builder.
    /// </summary>
    public async Task DownloadPresetStylesheetsAsync(HttpClient client)
    {
        var folder = Path.Combine(DownloadedAssetsOutputFolder, "presets");
        Directory.CreateDirectory(folder);

        foreach (var preset in PresetNames)
        {
            var response = await client.GetAsync($"/presets/{preset}.css");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsByteArrayAsync();
            if (content.Length == 0)
                throw new InvalidOperationException($"/presets/{preset}.css was served empty.");

            await File.WriteAllBytesAsync(Path.Combine(folder, $"{preset}.css"), content);
        }

        await File.WriteAllTextAsync(
            Path.Combine(folder, "presets.json"),
            JsonSerializer.Serialize(
                Themes.Select(theme => new
                {
                    name = theme.Name,
                    label = theme.Label,
                    description = theme.Description,
                }),
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                }
            ) + "\n"
        );
    }

    /// <summary>
    /// Downloads the icon picker demos' icons as a static file. Only Lucide's are kept: the samples
    /// also register the icon packs the docs preview, which would make the file several times larger.
    /// </summary>
    public async Task DownloadDemoIconsAsync(HttpClient client)
    {
        var response = await client.GetAsync(DemoIconsUrl);
        response.EnsureSuccessStatusCode();

        var lucide = new LucideIconPack().GetIcons().Keys.ToHashSet(StringComparer.Ordinal);
        var json = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;
        var icons = json["icons"]!
            .AsArray()
            .Where(icon => lucide.Contains(icon!["name"]!.GetValue<string>()))
            .Select(icon => icon!.DeepClone());

        await File.WriteAllTextAsync(
            Path.Combine(DownloadedAssetsOutputFolder, Path.GetFileName(ExportedIconsUrl)),
            new JsonObject { ["icons"] = new JsonArray([.. icons]) }.ToJsonString(
                new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }
            )
        );
    }

    /// <summary>Downloads the theme variable manifest under its stable name, for the website's theme builder.</summary>
    public async Task DownloadThemeVariableManifestAsync(HttpClient client)
    {
        var response = await client.GetAsync(
            "/_content/StellarAdmin.UI/stellar-admin.theme-variables.json"
        );
        response.EnsureSuccessStatusCode();

        await File.WriteAllBytesAsync(
            Path.Combine(DownloadedAssetsOutputFolder, "stellar-admin.theme-variables.json"),
            await response.Content.ReadAsByteArrayAsync()
        );
    }

    /// <summary>Reads a demo partial's source and writes a fenced Razor code-include (.mdx).</summary>
    public async Task GenerateDemoPartialSourceFileAsync(string page)
    {
        if (!Directory.Exists(PagesSourceCodeOutputFolder))
            Directory.CreateDirectory(PagesSourceCodeOutputFolder);

        var sourceFile = Path.Combine(
            PageSourceCodeFolder,
            page.Replace('/', Path.DirectorySeparatorChar) + ".cshtml"
        );

        var readSourceLines = await File.ReadAllLinesAsync(sourceFile);

        var stringsToRemove = new List<string>();
        var cleanedLines = new List<string>();
        var hasProcessedDirectives = false;
        var isProcessingStripSection = false;
        var charactersToDelete = 0;
        foreach (var sourceLine in readSourceLines)
        {
            // Read past the directives
            if (
                !hasProcessedDirectives
                && (sourceLine.StartsWith("@") || string.IsNullOrEmpty(sourceLine))
            )
                continue;

            if (isProcessingStripSection)
            {
                if (sourceLine.StartsWith("-->", StringComparison.CurrentCultureIgnoreCase))
                    isProcessingStripSection = false;
                else
                    stringsToRemove.Add(sourceLine);

                continue;
            }

            if (sourceLine.StartsWith("<!--strip", StringComparison.CurrentCultureIgnoreCase))
            {
                isProcessingStripSection = true;
                continue;
            }

            hasProcessedDirectives = true;

            if (sourceLine.IndexOf("<!-- code end -->", StringComparison.Ordinal) >= 0)
                break;

            if (
                sourceLine.IndexOf("<!-- code begin -->", StringComparison.Ordinal)
                is var index
                    and >= 0
            )
            {
                charactersToDelete = index;
                cleanedLines.Clear();
                continue;
            }

            var x =
                charactersToDelete > 0
                    ? sourceLine.Length > charactersToDelete
                        ? sourceLine.Remove(0, charactersToDelete)
                        : string.Empty
                    : sourceLine;

            foreach (var stringToRemove in stringsToRemove)
                x = x.Replace(stringToRemove, string.Empty, StringComparison.OrdinalIgnoreCase);
            cleanedLines.Add(x);
        }

        cleanedLines.Insert(0, "```razor");
        cleanedLines.Add("```");

        var filename = Path.Combine(PagesSourceCodeOutputFolder, GenerateFilename(page) + ".mdx");
        await File.WriteAllLinesAsync(filename, cleanedLines);
    }

    /// <summary>Renders a demo partial via the running site and writes the fixed-up HTML.</summary>
    public async Task RenderDemoPartialOutputAsync(
        HttpClient client,
        string partialName,
        string? layout,
        string? outputFileName
    )
    {
        var response = await client.GetAsync($"/DocsStatic/?name={partialName}&layout={layout}");
        response.EnsureSuccessStatusCode();

        var rendered = await response.Content.ReadAsStringAsync();
        await DownloadModuleScriptsAsync(client, rendered);

        var content = FixDemoContent(rendered);
        if (
            partialName.StartsWith("ImageUpload/", StringComparison.Ordinal)
            || partialName == "RichTextEditor/_Images"
        )
        {
            content = content.Replace("<head>", "<head>\n" + DemoUploadScript);
        }

        if (partialName.StartsWith("IconPicker/", StringComparison.Ordinal))
        {
            content = content.Replace(
                $"\"{DemoIconsUrl}\"",
                $"\"{ExportedIconsUrl}\"",
                StringComparison.Ordinal
            );
        }

        if (partialName == "DataGrid/_Intro")
        {
            // Exported demos have no server to handle the grid's query parameters.
            content = content.Replace("aria-disabled=\"false\"", string.Empty);
            content = Regex.Replace(
                content,
                "href=\"/DataGrid[^\"]*\"",
                "aria-disabled=\"true\" tabindex=\"-1\""
            );
        }

        if (!Directory.Exists(RenderedPagesOutputFolder))
            Directory.CreateDirectory(RenderedPagesOutputFolder);

        await File.WriteAllTextAsync(
            Path.Combine(
                RenderedPagesOutputFolder,
                $"{outputFileName ?? GenerateFilename(partialName)}.html"
            ),
            content
        );
    }

    // A component that loads its own bundle, such as the rich text editor, renders its module script
    // in the demo; the index page that the other assets come from doesn't reference it
    private static async Task DownloadModuleScriptsAsync(HttpClient client, string content)
    {
        foreach (Match match in ModuleScriptRegex().Matches(content))
        {
            var path = Path.Combine(DownloadedAssetsOutputFolder, match.Groups["filename"].Value);
            if (File.Exists(path))
                continue;

            var response = await client.GetAsync(match.Groups["url"].Value);
            response.EnsureSuccessStatusCode();

            await File.WriteAllBytesAsync(path, await response.Content.ReadAsByteArrayAsync());
        }
    }

    private static string FixDemoContent(string input)
    {
        // The samples app's preferences target a link that the website theme sync replaces.
        input = SamplesAppearanceScriptRegex().Replace(input, string.Empty);

        foreach (var url in FixedStaticAssets)
        {
            input = input.Replace(
                $"src=\"{url}\"",
                $"src=\"/demo/ui/assets/{Path.GetFileName(url)}\""
            );
        }

        input = DemoAssetRegex()
            .Replace(
                input,
                match =>
                    $"{match.Groups["tag"].Value}/demo/ui/assets/{match.Groups["filename"].Value}"
            );

        // Sync the demo with the docs website's light/dark selection. Injected here rather than in the
        // DocsSamples layouts so it never affects the samples app itself. The samples' preset link
        // (their theme picker's) is replaced by the saThemeInit() call that loads the default font.
        input = input.Replace("<head>", "<head>\n" + ThemeSyncScript);
        input = PresetLinkRegex().Replace(input, "<script>saThemeInit();</script>");

        return input;
    }

    private static string GenerateFilename(string input)
    {
        // Simple kekab-case converter
        return Regex
            .Replace(
                input.Replace("/", "").Replace("_", ""),
                "(?!^)([A-Z])",
                "-$1",
                RegexOptions.Compiled
            )
            .Trim()
            .ToLower();
    }

    [GeneratedRegex(
        @"(?<tag><(link|script).*(href|src)="")(?<url>.*\/(?<filename>.*\.(css|js)))",
        RegexOptions.IgnoreCase,
        "en-US"
    )]
    private static partial Regex DemoAssetRegex();

    [GeneratedRegex(
        @"<script(?=[^>]*\btype=""module"")[^>]*\bsrc=""(?<url>/_content/[^""]*/(?<filename>[^""/]+\.js))""",
        RegexOptions.IgnoreCase
    )]
    private static partial Regex ModuleScriptRegex();

    [GeneratedRegex(
        @"[ \t]*<script[^>]*src=""/js/appearance(?:\.[a-z0-9]+)?\.js[^""]*""[^>]*></script>\r?\n?",
        RegexOptions.IgnoreCase
    )]
    private static partial Regex SamplesAppearanceScriptRegex();

    // Matches the samples layout's preset link (its theme picker's stylesheet), with or without an href.
    [GeneratedRegex(@"<link[^>]*id=""docs-sample-theme""[^>]*/?>", RegexOptions.IgnoreCase)]
    private static partial Regex PresetLinkRegex();

    // The partials under Pages that are no demo of their own: the shared layouts and partials, and the
    // parts other demos include. A name ending in / covers a folder
    private static readonly string[] NotDemos =
    [
        "Shared/",
        "Showcase/GalleryParts/",
        "EnvironmentIndicator/_Shell",
    ];

    // The demos rendered in a layout other than _DemoLayout or to another file name. A partial listed
    // more than once renders once per entry
    private static readonly DemoPartial[] DemoExceptions =
    [
        new("AppHeader/_WithSidebar", "_CleanLayout"),
        new("EnvironmentIndicator/_Banner", "_CleanLayout"),
        new("EnvironmentIndicator/_CustomColor", "_CleanLayout"),
        new("EnvironmentIndicator/_Frame", "_CleanLayout"),
        new("EnvironmentIndicator/_Intro", "_CleanLayout"),
        new("EnvironmentIndicator/_TabSize", "_CleanLayout"),
        new("FormSection/_Card", "_CleanLayout"),
        new("FormSection/_Intro", "_CleanLayout"),
        new("FormSection/_Stacked", "_CleanLayout"),
        new("PageContainer/_Full", "_CleanLayout"),
        new("PageContainer/_Large", "_CleanLayout"),
        new("PageContainer/_Medium", "_CleanLayout"),
        new("PageContainer/_Small", "_CleanLayout"),
        new("PageHeader/_Badge", "_CleanLayout"),
        new("PageHeader/_Breadcrumb", "_CleanLayout"),
        new("PageHeader/_FullPage", "_CleanLayout"),
        new("PageHeader/_Intro", "_CleanLayout"),
        new("PageHeader/_Nav", "_CleanLayout"),
        new("PageHeader/_Sticky", "_CleanLayout"),
        new("PageHeader/_TitleOnly", "_CleanLayout"),
        new("Showcase/_AdminShell", "_CleanLayout"),
        new("Showcase/_Forms", "_ShowcaseLayout"),
        new("Showcase/_Masonry", "_ShowcaseLayout"),
        new("Showcase/_Overlays", "_ShowcaseLayout"),
        new("Showcase/_ThemeShowcase", "_ShowcaseLayout"),
        new("Sidebar/_AlternateTriggerIcon", "_CleanLayout"),
        new("Sidebar/_CollapsibleGroups", "_CleanLayout"),
        new("Sidebar/_CollapsibleIcon", "_CleanLayout"),
        new("Sidebar/_CollapsibleItems", "_CleanLayout"),
        new("Sidebar/_FloatingVariant", "_CleanLayout"),
        new("Sidebar/_InsetVariant", "_CleanLayout"),
        new("Sidebar/_Intro", "_CleanLayout"),
        new("Sidebar/_RightSide", "_CleanLayout"),
        new("Sidebar/_SidebarInDialog", "_CleanLayout"),
        new("ThemeOverride/_ThemeOverride", OutputFileName: "theme-override-standard"),
        new(
            "ThemeOverride/_ThemeOverride",
            OutputFileName: "theme-override",
            Layout: "_ThemeOverrideDemoLayout"
        ),
    ];

    /// <summary>
    /// Finds the demo partials to render and emit source includes for: every partial in a folder of
    /// the samples' pages, except the ones that are no demo of their own.
    /// </summary>
    public static IReadOnlyList<DemoPartial> FindDemoPartials()
    {
        var names = Directory
            .EnumerateFiles(PageSourceCodeFolder, "_*.cshtml", SearchOption.AllDirectories)
            .Select(path =>
                Path.ChangeExtension(Path.GetRelativePath(PageSourceCodeFolder, path), null)
                    .Replace(Path.DirectorySeparatorChar, '/')
            )
            .Where(name =>
                name.Contains('/')
                && Path.GetFileName(name) is not ("_ViewImports" or "_ViewStart")
                && !NotDemos.Any(notDemo =>
                    notDemo.EndsWith('/')
                        ? name.StartsWith(notDemo, StringComparison.Ordinal)
                        : name == notDemo
                )
            )
            .Order(StringComparer.Ordinal)
            .ToList();

        var unknownExceptions = DemoExceptions.Select(demo => demo.Name).Except(names).ToList();
        if (unknownExceptions.Count > 0)
        {
            throw new InvalidOperationException(
                $"The demo exceptions name partials that are no demo: {string.Join(", ", unknownExceptions)}."
            );
        }

        return
        [
            .. names.SelectMany(name =>
                DemoExceptions
                    .Where(demo => demo.Name == name)
                    .DefaultIfEmpty(new DemoPartial(name))
            ),
        ];
    }
}

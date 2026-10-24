using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using DocsSamples;
using StellarAdmin.UI.Icons;

namespace DocsSamplesGenerator;

/// <summary>
/// Generates the DocsSamples documentation artifacts (rendered demo HTML, source-code includes, and
/// downloaded static assets) and writes them into the sibling <c>website</c> project.
/// </summary>
internal sealed partial class Generator
{
    private static readonly string RepoRootFolder = GetRepoRootFolder();

    // Generator.cs lives at <repoRoot>/docs/DocsSamplesGenerator/, so the repo root is two directories
    // up from this source file. [CallerFilePath] is resolved at compile time, which keeps this correct
    // regardless of the working directory the CLI is launched from.
    private static string GetRepoRootFolder([CallerFilePath] string sourceFilePath = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFilePath)!, "..", ".."));

    // The theme fixtures (util/theme-check/presets) the samples serve from /presets/, exported for the
    // theme builder's presets. "default" is the token stylesheet alone, with no preset.
    private static readonly IReadOnlyList<ThemeFixture> Themes = ThemeFixtures.Load(RepoRootFolder);

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
        RepoRootFolder,
        "docs",
        "DocsSamples"
    );

    private static readonly string PageSourceCodeFolder = Path.Combine(
        RepoRootFolder,
        "docs",
        "DocsSamples",
        "Pages"
    );

    private static readonly string DocsProjectRootFolder = Environment.GetEnvironmentVariable(
        "STELLARADMIN_WEBSITE_DIR"
    )
        is { Length: > 0 } websiteDir
        ? Path.GetFullPath(websiteDir)
        : Path.GetFullPath(Path.Combine(RepoRootFolder, "..", "website"));

    private static readonly string RenderedPagesOutputFolder = Path.Combine(
        DocsProjectRootFolder,
        "public",
        "demo",
        "ui"
    );

    private static readonly string PagesSourceCodeOutputFolder = Path.Combine(
        DocsProjectRootFolder,
        "content",
        "docs",
        "ui",
        "components",
        "_include"
    );

    private static readonly string DownloadedAssetsOutputFolder = Path.Combine(
        DocsProjectRootFolder,
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
    public void CleanOutputFolders()
    {
        foreach (var folder in new[] { RenderedPagesOutputFolder, PagesSourceCodeOutputFolder })
        {
            if (Directory.Exists(folder))
                Directory.Delete(folder, recursive: true);
        }
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

    /// <summary>The demo partials to render and emit source includes for.</summary>
    public static readonly DemoPartial[] DemoPartials =
    [
        new("Accordion/_Disabled"),
        new("Accordion/_Intro"),
        new("Accordion/_Open"),
        new("Accordion/_Single"),
        new("Alert/_Actions"),
        new("Alert/_Basic"),
        new("Alert/_Destructive"),
        new("Alert/_Icons"),
        new("Alert/_Intro"),
        new("Alert/_Shorthand"),
        new("AlertDialog/_Destructive"),
        new("AlertDialog/_Intro"),
        new("AlertDialog/_JsApi"),
        new("AlertDialog/_Media"),
        new("AlertDialog/_Size"),
        new("AlertDialog/_SmallMedia"),
        new("AppHeader/_Actions"),
        new("AppHeader/_CustomHeight"),
        new("AppHeader/_Intro"),
        new("AppHeader/_WithSidebar", "_CleanLayout"),
        new("Attachment/_Group"),
        new("Attachment/_Image"),
        new("Attachment/_Intro"),
        new("Attachment/_Sizes"),
        new("Attachment/_States"),
        new("Attachment/_Trigger"),
        new("Avatar/_Badge"),
        new("Avatar/_BadgeWithIcon"),
        new("Avatar/_Group"),
        new("Avatar/_GroupWithCount"),
        new("Avatar/_GroupWithIconCount"),
        new("Avatar/_InEmpty"),
        new("Avatar/_Intro"),
        new("Avatar/_NameOrInitials"),
        new("Avatar/_Sizes"),
        new("Badge/_CustomColors"),
        new("Badge/_IconLeft"),
        new("Badge/_IconRight"),
        new("Badge/_Intro"),
        new("Badge/_LongText"),
        new("Badge/_Spinner"),
        new("Badge/_Variants"),
        new("Breadcrumb/_Collapsed"),
        new("Breadcrumb/_CustomCss"),
        new("Breadcrumb/_CustomSeparator"),
        new("Breadcrumb/_Icons"),
        new("Breadcrumb/_Intro"),
        new("Bubble/_Align"),
        new("Bubble/_Button"),
        new("Bubble/_Group"),
        new("Bubble/_Intro"),
        new("Bubble/_Link"),
        new("Bubble/_Reactions"),
        new("Bubble/_Variants"),
        new("Button/_AdditionalAttributes"),
        new("Button/_IconLeft"),
        new("Button/_IconOnly"),
        new("Button/_IconRight"),
        new("Button/_Intro"),
        new("Button/_InvalidStates"),
        new("Button/_SizesVariants"),
        new("Button/_Spinner"),
        new("ButtonGroup/_Basic"),
        new("ButtonGroup/_Intro"),
        new("ButtonGroup/_Nested"),
        new("ButtonGroup/_Pagination"),
        new("ButtonGroup/_PaginationSplit"),
        new("ButtonGroup/_Vertical"),
        new("ButtonGroup/_VerticalNested"),
        new("ButtonGroup/_Separator"),
        new("ButtonGroup/_Sizes"),
        new("ButtonGroup/_WithFields"),
        new("ButtonGroup/_WithIcons"),
        new("ButtonGroup/_WithInput"),
        new("ButtonGroup/_WithInputGroup"),
        new("ButtonGroup/_WithLike"),
        new("ButtonGroup/_WithPopover"),
        new("ButtonGroup/_WithSelect"),
        new("ButtonGroup/_WithSelectAndInput"),
        new("ButtonGroup/_WithText"),
        new("Card/_Default"),
        new("Card/_FooterWithBorder"),
        new("Card/_HeaderWithBorder"),
        new("Card/_Intro"),
        new("Card/_Login"),
        new("Card/_MeetingNotes"),
        new("Card/_Small"),
        new("Card/_WithImage"),
        new("Carousel/_Indicators"),
        new("Carousel/_Intro"),
        new("Carousel/_Sizes"),
        new("Carousel/_Vertical"),
        new("Checkbox/_ChoiceCards"),
        new("Checkbox/_Disabled"),
        new("Checkbox/_FieldExplicit"),
        new("Checkbox/_FieldImplicit"),
        new("Checkbox/_Group"),
        new("Checkbox/_GroupModelBinding"),
        new("Checkbox/_ClassNames"),
        new("Checkbox/_Intro"),
        new("Checkbox/_ManualValidationExplicit"),
        new("Checkbox/_ManualValidationImplicit"),
        new("Checkbox/_ModelBinding"),
        new("Checkbox/_ReadOnly"),
        new("Checkbox/_Validation"),
        new("Checklist/_Actions"),
        new("Checklist/_Card"),
        new("Checklist/_CustomStatus"),
        new("Checklist/_Descriptions"),
        new("Checklist/_Dividers"),
        new("Checklist/_Expandable"),
        new("Checklist/_Groups"),
        new("Checklist/_Intro"),
        new("Checklist/_Link"),
        new("Checklist/_Nested"),
        new("Checklist/_Statuses"),
        new("CheckboxGroup/_Intro"),
        new("CheckboxGroup/_ChoiceCards"),
        new("CheckboxGroup/_Items"),
        new("CheckboxGroup/_Media"),
        new("CheckboxGroup/_Columns"),
        new("CheckboxGroup/_CollectionBinding"),
        new("CheckboxGroup/_ReadOnly"),
        new("CheckboxGroup/_Validation"),
        new("Chip/_Disabled"),
        new("Chip/_Intro"),
        new("Chip/_Invalid"),
        new("Chip/_Media"),
        new("Chip/_ReadOnly"),
        new("Chip/_Removable"),
        new("Collapsible/_Expanded"),
        new("Collapsible/_Intro"),
        new("Collapsible/_Settings"),
        new("Command/_Checked"),
        new("Command/_Dialog"),
        new("Command/_Intro"),
        new("Command/_KeyboardShortcut"),
        new("Command/_Scrollable"),
        new("Dialog/_Dismissing"),
        new("DataGrid/_AddRow"),
        new("DataGrid/_Editing"),
        new("DataGrid/_EditingErrors"),
        new("DataGrid/_EditorTemplates"),
        new("DataGrid/_Intro"),
        new("DataGrid/_NewItem"),
        new("DataGrid/_RowDetails"),
        new("Details/_Columns"),
        new("Details/_Divided"),
        new("Details/_EmptyValues"),
        new("Details/_Horizontal"),
        new("Details/_InForm"),
        new("Details/_Intro"),
        new("Details/_RecordPage"),
        new("Details/_RowDetails"),
        new("Details/_Starts"),
        new("Details/_Templates"),
        new("Details/_Values"),
        new("DictionaryEditor/_Disabled"),
        new("DictionaryEditor/_EnumKeys"),
        new("DictionaryEditor/_Intro"),
        new("DictionaryEditor/_Items"),
        new("DictionaryEditor/_Numbers"),
        new("DictionaryEditor/_ReadOnly"),
        new("DictionaryEditor/_Validation"),
        new("Dialog/_Intro"),
        new("Dialog/_JsApi"),
        new("Dialog/_JsEvents"),
        new("Dialog/_ReturnFormValue"),
        new("Dialog/_ReturnValue"),
        new("Dialog/_ScrollableContent"),
        new("Dialog/_StickyFooter"),
        new("DropdownMenu/_CheckboxEvents"),
        new("DropdownMenu/_CheckboxItems"),
        new("DropdownMenu/_ClickEvents"),
        new("DropdownMenu/_CloseOnClick"),
        new("DropdownMenu/_Inset"),
        new("DropdownMenu/_Intro"),
        new("DropdownMenu/_Links"),
        new("DropdownMenu/_RadioEvents"),
        new("DropdownMenu/_RadioGroup"),
        new("DropdownMenu/_Submenu"),
        new("Empty/_Basic"),
        new("Empty/_Intro"),
        new("Empty/_WithBorder"),
        new("Empty/_WithIcon"),
        new("Empty/_WithMutedBackground"),
        new("Empty/_WithMutedBackgroundAlt"),
        new("EnvironmentIndicator/_Banner", "_CleanLayout"),
        new("EnvironmentIndicator/_CustomColor", "_CleanLayout"),
        new("EnvironmentIndicator/_Frame", "_CleanLayout"),
        new("EnvironmentIndicator/_Intro", "_CleanLayout"),
        new("EnvironmentIndicator/_TabSize", "_CleanLayout"),
        new("Field/_ClassNames"),
        new("Field/_Checkbox"),
        new("Field/_CheckboxImplicit"),
        new("Field/_FieldGroup"),
        new("Field/_FieldGroupImplicit"),
        new("Field/_Fieldset"),
        new("Field/_FieldsetImplicit"),
        new("Field/_Implicit"),
        new("Field/_Input"),
        new("Field/_InputImplicit"),
        new("Field/_Intro"),
        new("Field/_Radio"),
        new("Field/_RadioImplicit"),
        new("Field/_Select"),
        new("Field/_SelectImplicit"),
        new("Field/_Textarea"),
        new("Field/_TextareaImplicit"),
        new("Field/_UsageField"),
        new("Field/_UsageFieldContent"),
        new("Field/_UsageFieldGroup"),
        new("FormSection/_Intro", "_CleanLayout"),
        new("FormSection/_Card", "_CleanLayout"),
        new("FormSection/_Narrow"),
        new("FormSection/_Stacked", "_CleanLayout"),
        new("FormSection/_WithoutDescription"),
        new("Grid/_DisplayFields"),
        new("Grid/_GridItems"),
        new("Grid/_Intro"),
        new("Grid/_Narrow"),
        new("Grid/_Spans"),
        new("Grid/_Starts"),
        new("Group/_Align"),
        new("Group/_Gap"),
        new("Group/_Justify"),
        new("Icon/_Color"),
        new("Icon/_Custom"),
        new("Icon/_Intro"),
        new("Icon/_Size"),
        new("Icon/_StrokeWidth"),
        new("IconPicker/_Disabled"),
        new("IconPicker/_IconsUrl"),
        new("IconPicker/_Inline"),
        new("IconPicker/_Intro"),
        new("IconPicker/_ModelBinding"),
        new("IconPicker/_ReadOnly"),
        new("Image/_AspectRatio"),
        new("Image/_Figure"),
        new("Image/_Interaction"),
        new("Image/_Intro"),
        new("Image/_Sizes"),
        new("Image/_States"),
        new("ImageUpload/_Attachment"),
        new("ImageUpload/_Cell"),
        new("ImageUpload/_Compact"),
        new("ImageUpload/_Disabled"),
        new("ImageUpload/_Intro"),
        new("ImageUpload/_Limits"),
        new("ImageUpload/_ReadOnly"),
        new("Input/_FieldExplicit"),
        new("Input/_FieldImplicit"),
        new("Input/_InputTypesModelBinding"),
        new("Input/_InputTypes"),
        new("Input/_ClassNames"),
        new("Input/_Intro"),
        new("Input/_ManualValidationExplicit"),
        new("Input/_ManualValidationImplicit"),
        new("Input/_ModelBinding"),
        new("Input/_Validation"),
        new("InputGroup/_ButtonGroup"),
        new("InputGroup/_Buttons"),
        new("InputGroup/_Icons"),
        new("InputGroup/_Intro"),
        new("InputGroup/_Label"),
        new("InputGroup/_Spinner"),
        new("InputGroup/_Text"),
        new("InputGroup/_Textarea"),
        new("InputOtp/_Composition"),
        new("InputOtp/_CustomSeparator"),
        new("InputOtp/_Disabled"),
        new("InputOtp/_FourDigits"),
        new("InputOtp/_Groups"),
        new("InputOtp/_ClassNames"),
        new("InputOtp/_Intro"),
        new("InputOtp/_ManualValidationExplicit"),
        new("InputOtp/_ManualValidationImplicit"),
        new("InputOtp/_ModelBinding"),
        new("InputOtp/_Pattern"),
        new("InputOtp/_ReadOnly"),
        new("InputOtp/_Separator"),
        new("InputOtp/_Size"),
        new("InputOtp/_Validation"),
        new("Item/_Components"),
        new("Item/_Footer"),
        new("Item/_Group"),
        new("Item/_Header"),
        new("Item/_HeaderAndFooter"),
        new("Item/_Image"),
        new("Item/_Intro"),
        new("Item/_Link"),
        new("Item/_Separator"),
        new("Item/_Sizes"),
        new("Item/_Variants"),
        new("Js/AlertDialog/_Intro"),
        new("Js/AlertDialog/_ManualResult"),
        new("Js/Dialog/_FormValues"),
        new("Js/Dialog/_Intro"),
        new("Js/Dialog/_ManualResult"),
        new("Kbd/_ArrowKeys"),
        new("Kbd/_Basic"),
        new("Kbd/_InputGroup"),
        new("Kbd/_Intro"),
        new("Kbd/_KbdGroup"),
        new("Kbd/_ModifierKeys"),
        new("Kbd/_Tooltip"),
        new("Kbd/_WithIconAndText"),
        new("Kbd/_WithIcons"),
        new("Label/_Intro"),
        new("Label/_ModelBinding"),
        new("Label/_WithField"),
        new("LinkButton/_AdditionalAttributes"),
        new("LinkButton/_IconLeft"),
        new("LinkButton/_IconOnly"),
        new("LinkButton/_IconRight"),
        new("LinkButton/_Intro"),
        new("LinkButton/_SizesVariants"),
        new("LinkButton/_Url"),
        new("ListEditor/_Affixes"),
        new("ListEditor/_Disabled"),
        new("ListEditor/_Empty"),
        new("ListEditor/_Intro"),
        new("ListEditor/_ItemChecks"),
        new("ListEditor/_Numbered"),
        new("ListEditor/_Numbers"),
        new("ListEditor/_ReadOnly"),
        new("ListEditor/_Validation"),
        new("Marker/_Button"),
        new("Marker/_Icon"),
        new("Marker/_Intro"),
        new("Marker/_Link"),
        new("Marker/_Separator"),
        new("Marker/_Status"),
        new("Marker/_Variants"),
        new("Message/_Actions"),
        new("Message/_Avatar"),
        new("Message/_Group"),
        new("Message/_HeaderFooter"),
        new("Message/_Intro"),
        new("MessageScroller/_Directions"),
        new("MessageScroller/_InitialPosition"),
        new("MessageScroller/_Intro"),
        new("MessageScroller/_NewMessages"),
        new("PageContainer/_Full", "_CleanLayout"),
        new("PageContainer/_Large", "_CleanLayout"),
        new("PageContainer/_Medium", "_CleanLayout"),
        new("PageContainer/_Small", "_CleanLayout"),
        new("PageHeader/_Badge", "_CleanLayout"),
        new("PageHeader/_Breadcrumb", "_CleanLayout"),
        new("PageHeader/_FullPage", "_CleanLayout"),
        new("PageHeader/_Intro", "_CleanLayout"),
        new("PageHeader/_Nav", "_CleanLayout"),
        new("PageHeader/_TitleOnly", "_CleanLayout"),
        new("Pagination/_CustomContent"),
        new("Pagination/_Intro"),
        new("Pagination/_Url"),
        new("Popover/_ButtonGroup"),
        new("Popover/_Hover"),
        new("Popover/_Intro"),
        new("Popover/_JsApi"),
        new("Popover/_JsEvents"),
        new("Popover/_ManualDismiss"),
        new("Popover/_Offset"),
        new("Popover/_Position"),
        new("Progress/_FileUploadList"),
        new("Progress/_Intro"),
        new("Progress/_MinMax"),
        new("Progress/_WithLabel"),
        new("Questionnaire/_Card"),
        new("Questionnaire/_CustomProgress"),
        new("Questionnaire/_Freeform"),
        new("Questionnaire/_Intro"),
        new("Questionnaire/_LongForm"),
        new("Questionnaire/_Multiple"),
        new("Questionnaire/_Shortcuts"),
        new("Questionnaire/_Steps"),
        new("Questionnaire/_Validation"),
        new("Radio/_ChoiceCards"),
        new("Radio/_Disabled"),
        new("Radio/_FieldExplicit"),
        new("Radio/_FieldImplicit"),
        new("Radio/_ClassNames"),
        new("Radio/_Intro"),
        new("Radio/_ManualValidationExplicit"),
        new("Radio/_ManualValidationImplicit"),
        new("Radio/_ModelBinding"),
        new("Radio/_ReadOnly"),
        new("Radio/_Validation"),
        new("RichTextEditor/_Disabled"),
        new("RichTextEditor/_Images"),
        new("RichTextEditor/_Intro"),
        new("RichTextEditor/_ModelBinding"),
        new("RichTextEditor/_ReadOnly"),
        new("RadioGroup/_Intro"),
        new("RadioGroup/_ChoiceCards"),
        new("RadioGroup/_Items"),
        new("RadioGroup/_Media"),
        new("RadioGroup/_Columns"),
        new("RadioGroup/_ModelBinding"),
        new("RadioGroup/_ReadOnly"),
        new("RadioGroup/_Validation"),
        new("SegmentedControl/_ChangeEvent"),
        new("SegmentedControl/_Disabled"),
        new("SegmentedControl/_Icons"),
        new("SegmentedControl/_Intro"),
        new("SegmentedControl/_ModelBinding"),
        new("SegmentedControl/_ReadOnly"),
        new("SegmentedControl/_Validation"),
        new("Select/_Disabled"),
        new("Select/_FieldExplicit"),
        new("Select/_FieldImplicit"),
        new("Select/_Groups"),
        new("Select/_ClassNames"),
        new("Select/_Intro"),
        new("Select/_ManualValidationExplicit"),
        new("Select/_ManualValidationImplicit"),
        new("Select/_ModelBinding"),
        new("Select/_ReadOnly"),
        new("Select/_Sizes"),
        new("Select/_Validation"),
        new("Separator/_Horizontal"),
        new("Separator/_InList"),
        new("Separator/_Intro"),
        new("Separator/_Vertical"),
        new("Separator/_VerticalMenu"),
        new("Sheet/_Dismissing"),
        new("Sheet/_Intro"),
        new("Sheet/_JsApi"),
        new("Sheet/_JsEvents"),
        new("Sheet/_NoCloseButton"),
        new("Sheet/_ReturnFormValue"),
        new("Sheet/_Sides"),
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
        new("Skeleton/_Avatar"),
        new("Skeleton/_Form"),
        new("Skeleton/_Intro"),
        new("Skeleton/_Table"),
        new("Skeleton/_Text"),
        new("Slider/_Disabled"),
        new("Slider/_ClassNames"),
        new("Slider/_CustomMarks"),
        new("Slider/_Intro"),
        new("Slider/_Marks"),
        new("Slider/_ModelBinding"),
        new("Slider/_MultipleThumbs"),
        new("Slider/_Range"),
        new("Slider/_ReadOnly"),
        new("Slider/_Steps"),
        new("Slider/_ThumbAlignment"),
        new("Slider/_Value"),
        new("Slider/_Vertical"),
        new("SlotContent/_Intro"),
        new("SlotOutlet/_Fallback"),
        new("SlotOutlet/_Intro"),
        new("Spinner/_Color"),
        new("Spinner/_InBadges"),
        new("Spinner/_InButtons"),
        new("Spinner/_InEmpty"),
        new("Spinner/_InInputGroup"),
        new("Spinner/_Intro"),
        new("Spinner/_Size"),
        new("Stack/_Align"),
        new("Stack/_Gap"),
        new("Stack/_Justify"),
        new("Switch/_Disabled"),
        new("Switch/_ClassNames"),
        new("Switch/_Intro"),
        new("Switch/_ModelBinding"),
        new("Switch/_ReadOnly"),
        new("Switch/_Sizes"),
        new("Table/_Border"),
        new("Table/_Intro"),
        new("Table/_RowDetails"),
        new("Table/_RowDetailsDivider"),
        new("Table/_RowDetailsRowClick"),
        new("Table/_RowSelection"),
        new("Table/_Select"),
        new("Tabs/_Active"),
        new("Tabs/_Disabled"),
        new("Tabs/_Icons"),
        new("Tabs/_IconsOnly"),
        new("Tabs/_Intro"),
        new("Tabs/_Line"),
        new("Tabs/_Orientation"),
        new("Tabs/_Panels"),
        new("Tabs/_PanelsLine"),
        new("Tabs/_PanelsLoadOnShow"),
        new("Tabs/_PanelsVertical"),
        new("Tabs/_Url"),
        new("TemplatedTagHelper/_Intro"),
        new("TemplatedTagHelper/_ViewOverride"),
        new("Textarea/_FieldExplicit"),
        new("Textarea/_FieldImplicit"),
        new("Textarea/_ClassNames"),
        new("Textarea/_Intro"),
        new("Textarea/_ManualValidationExplicit"),
        new("Textarea/_ManualValidationImplicit"),
        new("Textarea/_ModelBinding"),
        new("Textarea/_Validation"),
        new("ThemeOverride/_ThemeOverride", OutputFileName: "theme-override-standard"),
        new(
            "ThemeOverride/_ThemeOverride",
            OutputFileName: "theme-override",
            Layout: "_ThemeOverrideDemoLayout"
        ),
        new("Theming/_BrandedPanel"),
        new("Theming/_CompactSection"),
        new("Theming/_CustomDensity"),
        new("Theming/_DarkSection"),
        new("Theming/_DensityLevels"),
        new("Timeline/_Avatars"),
        new("Timeline/_Card"),
        new("Timeline/_Comments"),
        new("Timeline/_Expandable"),
        new("Timeline/_Failure"),
        new("Timeline/_Groups"),
        new("Timeline/_Intro"),
        new("Timeline/_MarkerStyles"),
        new("Timeline/_Markers"),
        new("Timeline/_MediaStyles"),
        new("Timeline/_Numbered"),
        new("Timeline/_Progress"),
        new("Timeline/_TimeGutter"),
        new("Timeline/_TimePlacement"),
        new("Toast/_Action"),
        new("Toast/_Intro"),
        new("Toast/_Promise"),
        new("Toast/_Types"),
        new("Toggle/_Disabled"),
        new("Toggle/_Intro"),
        new("Toggle/_ModelBinding"),
        new("Toggle/_Outline"),
        new("Toggle/_ReadOnly"),
        new("Toggle/_Sizes"),
        new("Toggle/_WithText"),
        new("ToggleGroup/_ClassNames"),
        new("ToggleGroup/_Intro"),
        new("ToggleGroup/_Joined"),
        new("ToggleGroup/_ModelBinding"),
        new("ToggleGroup/_Multiple"),
        new("ToggleGroup/_Outline"),
        new("ToggleGroup/_ReadOnly"),
        new("ToggleGroup/_Sizes"),
        new("ToggleGroup/_Spacing"),
        new("ToggleGroup/_Vertical"),
        new("Tooltip/_Delay"),
        new("Tooltip/_Elements"),
        new("Tooltip/_Intro"),
        new("Tooltip/_JsApi"),
        new("Tooltip/_JsEvents"),
        new("Tooltip/_Offset"),
        new("Tooltip/_Position"),
    ];
}

using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace StellarAdmin.Diagnostics;

// What Development's sheet shows: each file that configured a problem, with the lines around each problem and the
// problems under their lines, then the problems with no line to show
internal sealed class ConfigurationProblemsSheet
{
    private ConfigurationProblemsSheet(
        ConfigurationProblemCounts counts,
        IReadOnlyList<ConfigurationProblemsFile> files,
        IReadOnlyList<ConfigurationProblem> unlocated
    )
    {
        Counts = counts;
        Files = files;
        Unlocated = unlocated;
    }

    public ConfigurationProblemCounts Counts { get; }

    public IReadOnlyList<ConfigurationProblemsFile> Files { get; }

    // Problems whose configuration has no line, such as a build without debug symbols
    public IReadOnlyList<ConfigurationProblem> Unlocated { get; }

    // Reads each file once. A file that can't be read, such as one built on another machine, shows its problems'
    // line numbers without their code.
    public static ConfigurationProblemsSheet Create(
        IReadOnlyList<ConfigurationProblem> problems,
        string? contentRoot,
        Func<string, IReadOnlyList<string>?> readLines
    )
    {
        var files = problems
            .Where(problem => problem.Source is not null)
            .GroupBy(problem => problem.Source!.FilePath)
            .Select(group => CreateFile(group.Key, [.. group], contentRoot, readLines(group.Key)))
            .ToList();

        return new(
            ConfigurationProblemCounts.From(problems),
            files,
            [.. problems.Where(problem => problem.Source is null)]
        );
    }

    // Highlights a line of C#: comments and string literals
    public static IHtmlContent HighlightCode(string text)
    {
        var html = new StringBuilder();
        var start = 0;
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == '/' && i + 1 < text.Length && text[i + 1] == '/')
            {
                html.Append(Encode(text[start..i]));
                html.Append("<span class=\"sa-configuration-problems-comment\">")
                    .Append(Encode(text[i..]))
                    .Append("</span>");

                return new HtmlString(html.ToString());
            }

            if (text[i] == '"')
            {
                var verbatim = i > 0 && text[i - 1] == '@';
                var end = i + 1;
                while (end < text.Length && text[end] != '"')
                {
                    end += !verbatim && text[end] == '\\' ? 2 : 1;
                }

                end = Math.Min(end + 1, text.Length);
                html.Append(Encode(text[start..i]));
                html.Append("<span class=\"sa-configuration-problems-string\">")
                    .Append(Encode(text[i..end]))
                    .Append("</span>");
                start = end;
                i = end - 1;
            }
        }

        html.Append(Encode(text[start..]));

        return new HtmlString(html.ToString());
    }

    // A message or fix, with the code that backticks mark
    public static IHtmlContent FormatMessage(string text)
    {
        var html = new StringBuilder();
        var parts = text.Split('`');
        for (var i = 0; i < parts.Length; i++)
        {
            var isCode = i % 2 == 1 && i < parts.Length - 1;
            html.Append(isCode ? "<code>" : "")
                .Append(Encode(i % 2 == 1 && !isCode ? "`" + parts[i] : parts[i]))
                .Append(isCode ? "</code>" : "");
        }

        return new HtmlString(html.ToString());
    }

    private static ConfigurationProblemsFile CreateFile(
        string filePath,
        IReadOnlyList<ConfigurationProblem> problems,
        string? contentRoot,
        IReadOnlyList<string>? lines
    )
    {
        var byLine = problems
            .GroupBy(problem => problem.Source!.Line)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<ConfigurationProblem>)
                    [.. group.OrderBy(problem => problem.Level)]
            );
        var hits = byLine.Keys.Order().ToList();

        // Each problem's line with one line either side, merged where they touch
        var ranges = new List<(int From, int To)>();
        foreach (var line in hits)
        {
            var (from, to) = lines is null ? (line, line) : (Math.Max(1, line - 1), line + 1);
            if (lines is not null)
            {
                to = Math.Min(to, Math.Max(lines.Count, line));
            }

            if (ranges.Count > 0 && from <= ranges[^1].To + 1)
            {
                ranges[^1] = (ranges[^1].From, Math.Max(ranges[^1].To, to));
            }
            else
            {
                ranges.Add((from, to));
            }
        }

        string TextOf(int line) => lines is not null && line <= lines.Count ? lines[line - 1] : "";

        // The code keeps its indentation relative to the least indented problem line
        var indent = hits.Min(line => Indentation(TextOf(line)));
        var rows = new List<ConfigurationProblemsRow>();
        foreach (var (from, to) in ranges)
        {
            if (rows.Count > 0)
            {
                rows.Add(ConfigurationProblemsRow.Gap);
            }

            for (var line = from; line <= to; line++)
            {
                var text = TextOf(line);
                rows.Add(
                    new(
                        line,
                        text[Math.Min(indent, Indentation(text))..],
                        byLine.GetValueOrDefault(line) ?? []
                    )
                );
            }
        }

        var displayPath = problems[0].Source!.GetDisplayPath(contentRoot);
        var separator = displayPath.LastIndexOfAny(['/', '\\']) + 1;

        return new(
            filePath,
            displayPath[..separator],
            displayPath[separator..],
            ConfigurationProblemCounts.From(problems),
            rows
        );
    }

    private static string Encode(string text) => HtmlEncoder.Default.Encode(text);

    private static int Indentation(string text) => text.Length - text.TrimStart().Length;
}

internal sealed record ConfigurationProblemCounts(int Errors, int Warnings, int Infos)
{
    public int Total => Errors + Warnings + Infos;

    public static ConfigurationProblemCounts From(IEnumerable<ConfigurationProblem> problems)
    {
        var levels = problems.Select(problem => problem.Level).ToList();

        return new(
            levels.Count(level => level == ConfigurationProblemLevel.Error),
            levels.Count(level => level == ConfigurationProblemLevel.Warning),
            levels.Count(level => level == ConfigurationProblemLevel.Info)
        );
    }

    // Each level with problems, most severe first
    public IEnumerable<(ConfigurationProblemLevel Level, int Count)> GetLevels()
    {
        if (Errors > 0)
        {
            yield return (ConfigurationProblemLevel.Error, Errors);
        }

        if (Warnings > 0)
        {
            yield return (ConfigurationProblemLevel.Warning, Warnings);
        }

        if (Infos > 0)
        {
            yield return (ConfigurationProblemLevel.Info, Infos);
        }
    }
}

internal sealed record ConfigurationProblemsFile(
    string FilePath,
    string Directory,
    string FileName,
    ConfigurationProblemCounts Counts,
    IReadOnlyList<ConfigurationProblemsRow> Rows
);

// A line of a file with the problems configured on it, or a gap between lines that aren't shown
internal sealed record ConfigurationProblemsRow(
    int Number,
    string Text,
    IReadOnlyList<ConfigurationProblem> Problems
)
{
    public static readonly ConfigurationProblemsRow Gap = new(0, "", []);

    public bool IsGap => Number == 0;

    // The most severe problem's level, which colours the line
    public ConfigurationProblemLevel? Level => Problems.Count > 0 ? Problems[0].Level : null;
}

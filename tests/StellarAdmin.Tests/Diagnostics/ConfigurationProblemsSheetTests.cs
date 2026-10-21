using StellarAdmin.Diagnostics;
using TUnit.Assertions.Enums;

namespace StellarAdmin.Tests.Diagnostics;

public class ConfigurationProblemsSheetTests
{
    private static readonly string FilePath = Path.Combine("/app", "Admin", "Products.cs");

    private static readonly string[] Lines =
    [
        "namespace Admin;", // 1
        "", // 2
        "    first();", // 3
        "    second();", // 4
        "", // 5
        "", // 6
        "        third();", // 7
        "    fourth();", // 8
    ];

    [Test]
    public async Task Create_CountsEachLevel()
    {
        // Arrange
        ConfigurationProblem[] problems =
        [
            Problem(3, ConfigurationProblemLevel.Error),
            Problem(4, ConfigurationProblemLevel.Info),
            Problem(7, ConfigurationProblemLevel.Warning),
            Problem(8, ConfigurationProblemLevel.Warning),
        ];

        // Act
        var sheet = ConfigurationProblemsSheet.Create(problems, "/app", _ => Lines);

        // Assert
        await Assert.That(sheet.Counts).IsEqualTo(new ConfigurationProblemCounts(1, 2, 1));
        await Assert
            .That(sheet.Counts.GetLevels().Select(level => level.Level))
            .IsEquivalentTo(
                [
                    ConfigurationProblemLevel.Error,
                    ConfigurationProblemLevel.Warning,
                    ConfigurationProblemLevel.Info,
                ],
                CollectionOrdering.Matching
            );
    }

    [Test]
    public async Task Create_FileUnderContentRoot_SplitsTheRelativePath()
    {
        // Arrange
        ConfigurationProblem[] problems = [Problem(3)];

        // Act
        var file = ConfigurationProblemsSheet.Create(problems, "/app", _ => Lines).Files.Single();

        // Assert
        await Assert.That(file.Directory).IsEqualTo($"Admin{Path.DirectorySeparatorChar}");
        await Assert.That(file.FileName).IsEqualTo("Products.cs");
        await Assert.That(file.FilePath).IsEqualTo(FilePath);
    }

    [Test]
    public async Task Create_MergesTouchingLinesAndMarksTheGaps()
    {
        // Arrange
        ConfigurationProblem[] problems = [Problem(3), Problem(4), Problem(8)];

        // Act
        var rows = ConfigurationProblemsSheet.Create(problems, "/app", _ => Lines).Files.Single().Rows;

        // Assert
        await Assert.That(rows.Select(row => row.Number)).IsEquivalentTo([2, 3, 4, 5, 0, 7, 8], CollectionOrdering.Matching);
    }

    [Test]
    public async Task Create_ProblemsOnOneLine_StackMostSevereFirst()
    {
        // Arrange
        ConfigurationProblem[] problems =
        [
            Problem(3, ConfigurationProblemLevel.Info),
            Problem(3, ConfigurationProblemLevel.Error),
        ];

        // Act
        var row = ConfigurationProblemsSheet
            .Create(problems, "/app", _ => Lines)
            .Files.Single()
            .Rows.Single(row => row.Number == 3);

        // Assert
        await Assert.That(row.Level).IsEqualTo(ConfigurationProblemLevel.Error);
        await Assert
            .That(row.Problems.Select(problem => problem.Level))
            .IsEquivalentTo(
                [ConfigurationProblemLevel.Error, ConfigurationProblemLevel.Info],
                CollectionOrdering.Matching
            );
    }

    [Test]
    public async Task Create_RemovesTheIndentationOfTheLeastIndentedProblemLine()
    {
        // Arrange
        ConfigurationProblem[] problems = [Problem(7), Problem(8)];

        // Act
        var rows = ConfigurationProblemsSheet.Create(problems, "/app", _ => Lines).Files.Single().Rows;

        // Assert
        await Assert
            .That(rows.Select(row => row.Text))
            .IsEquivalentTo(["", "    third();", "fourth();"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task Create_UnreadableFile_ShowsOnlyTheProblemLines()
    {
        // Arrange
        ConfigurationProblem[] problems = [Problem(3), Problem(8)];

        // Act
        var rows = ConfigurationProblemsSheet.Create(problems, "/app", _ => null).Files.Single().Rows;

        // Assert
        await Assert.That(rows.Select(row => row.Number)).IsEquivalentTo([3, 0, 8], CollectionOrdering.Matching);
        await Assert.That(rows.All(row => row.Text == "")).IsTrue();
    }

    [Test]
    public async Task Create_WithoutSource_ListsTheProblemAsUnlocated()
    {
        // Arrange
        var problem = Problem(3) with { Source = null };

        // Act
        var sheet = ConfigurationProblemsSheet.Create([problem], "/app", _ => Lines);

        // Assert
        await Assert.That(sheet.Files).IsEmpty();
        await Assert.That(sheet.Unlocated.Single()).IsEqualTo(problem);
    }

    [Test]
    public async Task FormatMessage_MarksBacktickedCode()
    {
        // Act
        var html = ConfigurationProblemsSheet.FormatMessage("Add `List<int>` to `Sizes`.").ToString();

        // Assert
        await Assert
            .That(html)
            .IsEqualTo("Add <code>List&lt;int&gt;</code> to <code>Sizes</code>.");
    }

    [Test]
    public async Task FormatMessage_UnpairedBacktick_KeepsIt()
    {
        // Act
        var html = ConfigurationProblemsSheet.FormatMessage("A `B").ToString();

        // Assert
        await Assert.That(html).IsEqualTo("A `B");
    }

    [Test]
    public async Task HighlightCode_MarksStringsAndComments()
    {
        // Act
        var html = ConfigurationProblemsSheet
            .HighlightCode("item.Group = \"Trips // x\"; // Note")
            .ToString();

        // Assert
        await Assert
            .That(html)
            .IsEqualTo(
                "item.Group = <span class=\"sa-configuration-problems-string\">&quot;Trips // x&quot;</span>; "
                    + "<span class=\"sa-configuration-problems-comment\">// Note</span>"
            );
    }

    private static ConfigurationProblem Problem(
        int line,
        ConfigurationProblemLevel level = ConfigurationProblemLevel.Warning
    ) =>
        new(ConfigurationProblemKind.BlankLabel, $"Message {line}", "Fix")
        {
            Level = level,
            Source = new(FilePath, line),
        };
}

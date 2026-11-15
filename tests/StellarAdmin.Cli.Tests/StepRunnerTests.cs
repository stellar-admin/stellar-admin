using TUnit.Assertions;
using TUnit.Core;

namespace StellarAdmin.Cli.Tests;

public class StepRunnerTests
{
    [Test]
    public async Task RunAsync_WhenAStepFails_SkipsTheLaterSteps()
    {
        // Arrange
        List<string> ran = [];
        var sut = new StepRunner()
            .Add("Fail", step => Fail(step, ran))
            .Add("Later", () => Record(ran, "Later"));

        // Act
        var succeeded = await sut.RunAsync();

        // Assert
        await Assert.That(succeeded).IsFalse();
        await Assert.That(ran).IsEquivalentTo(["Fail"]);
    }

    [Test]
    public async Task RunAsync_WhenAStepFails_RunsTheAlwaysStepsAndFails()
    {
        // Arrange
        List<string> ran = [];
        var sut = new StepRunner()
            .Add("Fail", step => Fail(step, ran))
            .Add("Later", () => Record(ran, "Later"))
            .AddAlways("Always", () => Record(ran, "Always"));

        // Act
        var succeeded = await sut.RunAsync();

        // Assert
        await Assert.That(succeeded).IsFalse();
        await Assert.That(ran).IsEquivalentTo(["Fail", "Always"]);
    }

    private static Task Fail(StepRunner.Step step, List<string> ran)
    {
        ran.Add("Fail");
        step.Error("failed");
        return Task.CompletedTask;
    }

    private static Task Record(List<string> ran, string title)
    {
        ran.Add(title);
        return Task.CompletedTask;
    }
}

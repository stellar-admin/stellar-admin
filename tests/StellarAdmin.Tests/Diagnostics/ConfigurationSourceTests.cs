using System.Runtime.CompilerServices;
using StellarAdmin.Diagnostics;

namespace StellarAdmin.Tests.Diagnostics;

public class ConfigurationSourceTests
{
    [Test]
    public async Task Capture_ReturnsTheCallersLine()
    {
        // Arrange
        var line = CurrentLine() + 3;

        // Act
        var source = ConfigurationSource.Capture();

        // Assert
        await Assert.That(source).IsEqualTo(new ConfigurationSource(CurrentFile(), line));
    }

    [Test]
    public async Task Describe_FileOutsideContentRoot_ReturnsFullPath()
    {
        // Arrange
        var source = new ConfigurationSource(Path.Combine("/other", "Program.cs"), 7);

        // Act
        var description = source.Describe(Path.Combine("/app", "src"));

        // Assert
        await Assert.That(description).IsEqualTo($"{Path.Combine("/other", "Program.cs")}:7");
    }

    [Test]
    public async Task Describe_FileUnderContentRoot_ReturnsRelativePath()
    {
        // Arrange
        var source = new ConfigurationSource(Path.Combine("/app", "Admin", "Products.cs"), 42);

        // Act
        var description = source.Describe("/app");

        // Assert
        await Assert.That(description).IsEqualTo($"{Path.Combine("Admin", "Products.cs")}:42");
    }

    [Test]
    public async Task FromException_ReturnsTheLineThatThrew()
    {
        // Arrange
        var line = 0;
        Exception? exception = null;

        // Act
        try
        {
            line = CurrentLine() + 1;
            throw new InvalidOperationException("Broken");
        }
        catch (InvalidOperationException caught)
        {
            exception = caught;
        }

        // Assert
        await Assert
            .That(ConfigurationSource.FromException(exception))
            .IsEqualTo(new ConfigurationSource(CurrentFile(), line));
    }

    private static string CurrentFile([CallerFilePath] string path = "") => path;

    private static int CurrentLine([CallerLineNumber] int line = 0) => line;
}

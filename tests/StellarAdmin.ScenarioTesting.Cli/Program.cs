using StellarAdmin.ScenarioTesting.Hosting;
using StellarAdmin.ScenarioTesting.Runner;
using StellarAdmin.ScenarioTesting.Scenarios;

if (args is ["--specs", .. var specArgs])
{
    return await RunSpecsAsync(specArgs);
}

if (args.Length == 0 || ScenarioCatalog.Find(args[0]) is not { } scenario)
{
    if (args.Length > 0)
    {
        Console.Error.WriteLine($"Unknown scenario '{args[0]}'.");
    }

    Console.WriteLine(
        "Usage: dotnet run --project tests/StellarAdmin.Scenarios.Host -- <scenario>"
    );
    Console.WriteLine(
        "       dotnet run --project tests/StellarAdmin.Scenarios.Host -- --specs <file|dir>... [--screenshots <dir>]"
    );
    Console.WriteLine();
    Console.WriteLine("Scenarios:");
    foreach (var item in ScenarioCatalog.All)
    {
        Console.WriteLine($"  {item.Name, -12} {item.Description}");
    }

    return args.Length == 0 ? 0 : 1;
}

var app = ScenarioHost.Create(scenario, args[1..]);
Console.WriteLine($"Running scenario '{scenario.Name}': {scenario.Description}");
await app.RunAsync();

return 0;

// Runs spec files, or every spec in a folder, outside the test runner; screenshots save only with
// --screenshots
static async Task<int> RunSpecsAsync(string[] args)
{
    string? screenshotDirectory = null;
    var paths = new List<string>();
    for (var i = 0; i < args.Length; i++)
    {
        if (args[i] == "--screenshots" && i + 1 < args.Length)
        {
            screenshotDirectory = Path.GetFullPath(args[++i]);
        }
        else
        {
            paths.AddRange(
                Directory.Exists(args[i]) ? SpecFiles.Find(args[i]) : [Path.GetFullPath(args[i])]
            );
        }
    }

    if (paths.Count == 0)
    {
        Console.Error.WriteLine("Pass at least one spec file or folder.");

        return 1;
    }

    await using var browser = await SpecBrowser.LaunchAsync();
    var failed = 0;
    foreach (var path in paths)
    {
        try
        {
            var completed = await SpecRunner.RunAsync(
                SpecLoader.Load(path, Path.GetRelativePath(Environment.CurrentDirectory, path)),
                browser,
                screenshotDirectory
            );
            Console.WriteLine($"passed {SpecFiles.Name(path)} ({completed} steps)");
        }
        catch (SpecException exception)
        {
            Console.Error.WriteLine($"failed {exception.Message}");
            failed++;
        }
    }

    return failed == 0 ? 0 : 1;
}

using StellarAdmin.Scenarios;

if (args.Length == 0 || ScenarioCatalog.Find(args[0]) is not { } scenario)
{
    if (args.Length > 0)
    {
        Console.Error.WriteLine($"Unknown scenario '{args[0]}'.");
    }

    Console.WriteLine(
        "Usage: dotnet run --project tests/StellarAdmin.Scenarios.Host -- <scenario>"
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

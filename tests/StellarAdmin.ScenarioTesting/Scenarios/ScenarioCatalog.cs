using System.Reflection;
using StellarAdmin.ScenarioTesting.Hosting;

namespace StellarAdmin.ScenarioTesting.Scenarios;

// Every concrete Scenario in the assemblies, one per [ScenarioArguments] attribute on a class that has them
public sealed class ScenarioCatalog
{
    public ScenarioCatalog(params IEnumerable<Assembly> assemblies)
    {
        All =
        [
            .. assemblies
                .Distinct()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(Scenario)) && !type.IsAbstract)
                .SelectMany(Create)
                .OrderBy(scenario => scenario.Name, StringComparer.Ordinal),
        ];

        if (
            All.GroupBy(scenario => scenario.Name, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(group => group.Count() > 1) is
            { } duplicate
        )
        {
            throw new InvalidOperationException(
                $"Scenario name '{duplicate.Key}' is used by "
                    + string.Join(
                        " and ",
                        duplicate.Select(scenario => scenario.GetType().FullName)
                    )
                    + "."
            );
        }
    }

    public IReadOnlyList<Scenario> All { get; }

    public Scenario? Find(string name) =>
        All.FirstOrDefault(scenario =>
            string.Equals(scenario.Name, name, StringComparison.OrdinalIgnoreCase)
        );

    private static IEnumerable<Scenario> Create(Type type)
    {
        var attributes = type.GetCustomAttributes<ScenarioArgumentsAttribute>().ToArray();
        if (attributes.Length == 0)
        {
            yield return (Scenario)Activator.CreateInstance(type)!;
            yield break;
        }

        foreach (var attribute in attributes)
        {
            var scenario = (Scenario)Activator.CreateInstance(type, attribute.Values)!;
            scenario.Arguments = attribute.Values;

            yield return scenario;
        }
    }
}

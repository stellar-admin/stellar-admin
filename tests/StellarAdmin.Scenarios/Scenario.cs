using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin.Scenarios;

// A named admin panel configuration that tests, screenshots and the scenario host all run
public abstract class Scenario
{
    public abstract string Description { get; }

    public abstract string Name { get; }

    // The page the host opens and screenshots capture
    public virtual string StartPath => "/stellaradmin";

    public abstract void ConfigureAdmin(StellarAdminBuilder admin);

    public virtual void ConfigureServices(IServiceCollection services) { }

    // Names the scenario in test explorers and data-driven test names
    public override string ToString() => Name;
}

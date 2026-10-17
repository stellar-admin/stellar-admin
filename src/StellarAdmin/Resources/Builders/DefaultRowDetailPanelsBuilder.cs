using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin.Resources.Builders;

internal sealed class DefaultRowDetailPanelsBuilder<TResource>(IServiceCollection services)
    : ResourceRowDetailPanelsBuilder<TResource>(services);

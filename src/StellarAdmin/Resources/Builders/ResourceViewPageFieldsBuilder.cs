using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

internal sealed class ResourceViewPageFieldsBuilder<TModel>(Action<Action<IFormScope>> configure)
    : ResourceViewFieldsBuilder<TModel>(configure);

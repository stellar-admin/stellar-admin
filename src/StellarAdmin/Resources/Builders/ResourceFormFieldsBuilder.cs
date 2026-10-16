using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

internal sealed class ResourceFormFieldsBuilder<TResource>(Action<Action<IFormScope>> configure)
    : ResourceFieldsBuilder<TResource>(configure);

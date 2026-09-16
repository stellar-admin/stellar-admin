using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace StellarAdmin.Pro.Infrastructure.Mvc;

// Registers controller types that MVC's built-in discovery misses — closed generic
// controllers, which ControllerFeatureProvider skips — and doubles as the convention
// that assigns their route-friendly names, since MVC derives the name from the CLR
// type name, which for a generic type is mangled with its arity (UsersController`1),
// breaking both routing and view lookup.
internal sealed class StellarAdminControllerFeatureProvider
    : IApplicationFeatureProvider<ControllerFeature>,
        IControllerModelConvention
{
    private readonly Dictionary<TypeInfo, string> _controllers = [];

    public void AddController(Type controllerType, string controllerName)
    {
        if (
            _controllers.Any(entry =>
                string.Equals(entry.Value, controllerName, StringComparison.OrdinalIgnoreCase)
                && entry.Key != controllerType.GetTypeInfo()
            )
        )
        {
            throw new InvalidOperationException(
                $"Controller name '{controllerName}' is already registered."
            );
        }

        if (
            _controllers.TryGetValue(controllerType.GetTypeInfo(), out var existing)
            && existing != controllerName
        )
        {
            throw new InvalidOperationException(
                $"Controller type '{controllerType}' is already registered as '{existing}'."
            );
        }

        _controllers.TryAdd(controllerType.GetTypeInfo(), controllerName);
    }

    public void Apply(ControllerModel controller)
    {
        if (_controllers.TryGetValue(controller.ControllerType, out var controllerName))
        {
            controller.ControllerName = controllerName;
        }
    }

    public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
    {
        foreach (var controllerType in _controllers.Keys)
        {
            if (feature.Controllers.Contains(controllerType))
            {
                continue;
            }

            feature.Controllers.Add(controllerType);
        }
    }
}

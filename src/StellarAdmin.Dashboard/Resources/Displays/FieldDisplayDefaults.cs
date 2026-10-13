using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Displays;

// The display a field uses when it configures none, chosen from the property's data type before its
// CLR type. Collections and other types have none, so the display field renders them itself.
internal static class FieldDisplayDefaults
{
    // The display a column's cells render, or null to let the grid render a [UIHint] template or
    // the value itself. Its metadata resolves as the grid resolves it, property by property.
    public static FieldDisplay? Resolve(
        DataGridColumnOptions column,
        IModelMetadataProvider metadataProvider
    )
    {
        if (column.Display is not null || column.UsesBuiltInRendering)
        {
            return column.Display;
        }

        var expression = column.DisplayExpression ?? column.FieldExpression;
        if (ResourcePropertyPath.GetProperties(expression) is not { } properties)
        {
            return null;
        }

        var containerType = expression.Parameters[0].Type;
        ModelMetadata? metadata = null;
        foreach (var property in properties)
        {
            metadata = metadataProvider.GetMetadataForProperty(containerType, property.Name);
            containerType = metadata.ModelType;
        }

        return metadata!.TemplateHint is null ? Resolve(metadata) : null;
    }

    public static FieldDisplay? Resolve(ModelMetadata metadata)
    {
        switch (metadata.DataTypeName)
        {
            case nameof(DataType.EmailAddress):
                return new EmailDisplay();
            case nameof(DataType.Url):
                return new LinkDisplay();
            case nameof(DataType.PhoneNumber):
                return new PhoneDisplay();
        }

        var type = metadata.UnderlyingOrModelType;
        if (
            type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(DateOnly)
            || type == typeof(TimeOnly)
        )
        {
            return new DateDisplay();
        }

        if (type == typeof(bool))
        {
            return new BooleanDisplay();
        }

        if (metadata.IsEnum)
        {
            return metadata.IsFlagsEnum ? null : new EnumDisplay();
        }

        if (IsNumber(type))
        {
            return new NumberDisplay();
        }

        return type == typeof(string) ? new TextDisplay() : null;
    }

    private static bool IsNumber(Type type) =>
        Type.GetTypeCode(type)
            is TypeCode.Byte
                or TypeCode.SByte
                or TypeCode.Int16
                or TypeCode.UInt16
                or TypeCode.Int32
                or TypeCode.UInt32
                or TypeCode.Int64
                or TypeCode.UInt64
                or TypeCode.Single
                or TypeCode.Double
                or TypeCode.Decimal;
}

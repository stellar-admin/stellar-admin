using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace StellarAdmin.TagHelpers;

// Stores queued toasts in TempData as one JSON string, because TempData only holds simple types.
// The same camelCase JSON is sent in the SA-Toasts header and read by the client. The default
// encoder escapes non-ASCII characters, which keeps the header value ASCII.
internal static class ToastQueue
{
    public const string HeaderName = "SA-Toasts";
    public const int MaxHeaderBytes = 4096;
    public const string TempDataKey = "StellarAdmin.Toasts";

    private static readonly JsonSerializerOptions SerializerOptions = new(
        JsonSerializerDefaults.Web
    )
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    // Moves as many queued toasts as fit in maxBytes into a JSON array and leaves the rest queued.
    // The first toast is always taken so that an oversized toast cannot block the queue.
    public static string? DrainToHeader(ITempDataDictionary tempData, int maxBytes)
    {
        var messages = Peek(tempData);
        if (messages.Count == 0)
        {
            return null;
        }

        var header = new StringBuilder("[");
        var taken = 0;
        foreach (var message in messages)
        {
            var json = JsonSerializer.Serialize(message, SerializerOptions);
            if (taken > 0 && header.Length + json.Length + 2 > maxBytes)
            {
                break;
            }

            if (taken > 0)
            {
                header.Append(',');
            }

            header.Append(json);
            taken++;
        }

        header.Append(']');

        if (taken == messages.Count)
        {
            tempData.Remove(TempDataKey);
        }
        else
        {
            tempData[TempDataKey] = JsonSerializer.Serialize(messages[taken..], SerializerOptions);
        }

        return header.ToString();
    }

    public static void Enqueue(ITempDataDictionary tempData, Toast toast)
    {
        var messages = Peek(tempData);
        messages.Add(ToastMessage.From(toast));

        tempData[TempDataKey] = JsonSerializer.Serialize(messages, SerializerOptions);
    }

    private static List<ToastMessage> Peek(ITempDataDictionary tempData)
    {
        if (tempData.Peek(TempDataKey) is not string json)
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<ToastMessage>>(json, SerializerOptions) ?? [];
    }

    private sealed record ToastActionMessage(string Label, string Href);

    private sealed record ToastMessage(
        string Title,
        string? Description,
        ToastType Type,
        long? Duration,
        ToastActionMessage? Action
    )
    {
        public static ToastMessage From(Toast toast) =>
            new(
                toast.Title,
                toast.Description,
                toast.Type,
                toast.Duration is { } duration ? (long)duration.TotalMilliseconds : null,
                toast.Action is { } action
                    ? new ToastActionMessage(action.Label, action.Href)
                    : null
            );
    }
}

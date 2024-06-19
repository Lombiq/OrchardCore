using Microsoft.AspNetCore.Mvc.Localization;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OrchardCore.Json.Serialization;

public class LocalizedHtmlStringConverter : JsonConverter<LocalizedHtmlString>
{
    public override void Write(Utf8JsonWriter writer, LocalizedHtmlString value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString(nameof(LocalizedHtmlString.Name), value.Name);
        writer.WriteString(nameof(LocalizedHtmlString.Value), value.Value);
        writer.WriteBoolean(nameof(LocalizedHtmlString.IsResourceNotFound), value.IsResourceNotFound);

        writer.WriteEndObject();
    }

    public override LocalizedHtmlString Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var token = JsonNode.Parse(ref reader);

        if (token is JsonValue jsonValue)
        {
            var text = jsonValue.GetValue<string>();
            return new LocalizedHtmlString(text, text);
        }

        if (token is JsonObject jsonObject)
        {
            var dictionary = new Dictionary<string, JsonNode>(jsonObject, StringComparer.OrdinalIgnoreCase);
            var name = dictionary.TryGetValue(nameof(LocalizedHtmlString.Name), out var nameNode)
                ? nameNode.Deserialize<string>()
                : null;
            var value = dictionary.TryGetValue(nameof(LocalizedHtmlString.Value), out var valueNode)
                ? valueNode.Deserialize<string>() ?? name
                : name;
            var isResourceNotFound =
                dictionary.TryGetValue(nameof(LocalizedHtmlString.IsResourceNotFound), out var notFoundNode) &&
                notFoundNode.Deserialize<bool>();

            name ??= value;
            if (string.IsNullOrEmpty(name)) throw new InvalidOperationException("Missing name.");

            return new LocalizedHtmlString(name, value, isResourceNotFound);
        }

        throw new InvalidOperationException($"Can't parse token \"{token}\". It should be an object or a string");
    }
}

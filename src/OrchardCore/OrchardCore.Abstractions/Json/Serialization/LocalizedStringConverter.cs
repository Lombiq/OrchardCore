using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Json.Serialization;

public class LocalizedStringConverter : JsonConverter<LocalizedString>
{
    public override LocalizedString Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var node = JsonNode.Parse(ref reader);

        if (node?.GetValueKind() == JsonValueKind.String)
        {
            var text = node.GetValue<string>();
            return new(text, text);
        }

        if (node is JsonObject jsonObject)
        {
            var dictionary = new Dictionary<string, JsonNode>(jsonObject, StringComparer.OrdinalIgnoreCase);
            var name = dictionary.TryGetValue(nameof(LocalizedString.Name), out var nameNode)
                ? nameNode.GetValue<string>()
                : null;
            var value = dictionary.TryGetValue(nameof(LocalizedString.Value), out var valueNode)
                ? valueNode.GetValue<string>() ?? name
                : name;
            var isResourceNotFound =
                dictionary.TryGetValue(nameof(LocalizedString.ResourceNotFound), out var notFoundNode) &&
                notFoundNode.GetValue<bool>();
            var searchedLocation = dictionary.TryGetValue(nameof(LocalizedString.SearchedLocation), out var locationNode)
                ? locationNode.GetValue<string>()
                : null;

            name ??= value;
            if (string.IsNullOrEmpty(name)) throw new InvalidOperationException("Missing name.");

            return new(name, value, isResourceNotFound, searchedLocation);
        }

        throw new InvalidOperationException($"Can't parse token \"{node}\". It should be an object or a string");
    }

    public override void Write(Utf8JsonWriter writer, LocalizedString value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString(nameof(LocalizedString.Name), value.Name);
        writer.WriteString(nameof(LocalizedString.Value), value.Value);
        writer.WriteBoolean(nameof(LocalizedString.ResourceNotFound), value.ResourceNotFound);
        writer.WriteString(nameof(LocalizedString.SearchedLocation), value.SearchedLocation);

        writer.WriteEndObject();
    }
}

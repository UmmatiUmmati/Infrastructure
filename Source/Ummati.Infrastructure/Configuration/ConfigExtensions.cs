namespace Ummati.Infrastructure.Configuration;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Pulumi;

public static class ConfigExtensions
{
    public static T GetFromJson<T>(this Config config, string key)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(key);

        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonStringEnumConverter());
        return config.RequireObject<JsonElement>(key).Deserialize<T>(options)!;
    }

    public static string GetString(this Config config, string key)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(key);

        return config.Require(key);
    }
}

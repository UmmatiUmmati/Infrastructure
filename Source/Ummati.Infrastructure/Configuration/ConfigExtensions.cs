namespace Ummati.Infrastructure.Configuration;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Pulumi;

public static class ConfigExtensions
{
    public static T Get<T>(this Config config, string key)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(key);

        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonStringEnumConverter());
        return config.RequireObject<JsonElement>(key).Deserialize<T>(options)!;
    }

    public static string GetString(this Config config, string key, string? pattern = null)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(key);

        var value = config.Require(key);
        if (!string.IsNullOrWhiteSpace(pattern))
        {
            Guard.IsMatch(key, value, pattern);
        }

        return value;
    }

    // public static double GetDouble(this Config config, string key, double? minimum = null, double? maximum = null)
    // {
    //     ArgumentNullException.ThrowIfNull(config);
    //
    //     var value = double.Parse(config.Require(key), CultureInfo.InvariantCulture);
    //     AssertIsBetween(key, value, minimum, maximum);
    //     return value;
    // }
    //
    // public static int GetInteger(this Config config, string key, int? minimum = null, int? maximum = null)
    // {
    //     ArgumentNullException.ThrowIfNull(config);
    //
    //     var value = int.Parse(config.Require(key), CultureInfo.InvariantCulture);
    //     AssertIsBetween(key, value, minimum, maximum);
    //
    //     return value;
    // }
    //
    // public static IEnumerable<string> GetStringCollection(this Config config, string key)
    // {
    //     ArgumentNullException.ThrowIfNull(config);
    //
    //     return config
    //         .RequireObject<JsonElement>(key)
    //         .EnumerateArray()
    //         .Select(x => x.GetString()!)
    //         .Where(x => x is not null);
    // }
    //
    // public static IEnumerable<int> GetIntegerCollection(
    //     this Config config,
    //     string key,
    //     int? minimum = null,
    //     int? maximum = null) =>
    //     config.GetStringCollection(key).Select(x =>
    //     {
    //         var value = int.Parse(x, CultureInfo.InvariantCulture);
    //         AssertIsBetween(key, value, minimum, maximum);
    //         return value;
    //     });
    //
    // public static IEnumerable<T> GetCollection<T>(this Config config, string key, Dictionary<string, T> map) =>
    //     config.GetStringCollection(key).Select(x => GetFromMap(x, map));
    //
    // public static T Get<T>(this Config config, string key, Dictionary<string, T> map)
    // {
    //     ArgumentNullException.ThrowIfNull(config);
    //     ArgumentNullException.ThrowIfNull(map);
    //
    //     return GetFromMap(config.Require(key), map);
    // }
    //
    // private static T GetFromMap<T>(string value, Dictionary<string, T> map)
    // {
    //     if (map.TryGetValue(value, out var result))
    //     {
    //         return result;
    //     }
    //
    //     throw new InvalidOperationException($"{typeof(T).Name} with value '{value}' not recognised.");
    // }
}

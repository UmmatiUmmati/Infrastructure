namespace Ummati.Infrastructure.Resources;

using System.Diagnostics.CodeAnalysis;
using Pulumi;
using Ummati.Infrastructure.Configuration;

public class ComponentResource<T> : ComponentResource
{
    public ComponentResource(
        [NotNull] string name,
        [NotNull] IConfiguration configuration,
        [NotNull] string location,
        ComponentResourceOptions? options = null)
        : base(GetType(configuration), GetName(name, configuration, location), options)
    {
    }

    private static string GetType(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return $"{configuration.ApplicationName}:{typeof(T).Name}";
    }

    private static string GetName(string name, IConfiguration configuration, string location)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(location);

        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException($"'{nameof(name)}' cannot be empty.", nameof(name));
        }

        if (string.IsNullOrEmpty(location))
        {
            throw new ArgumentException($"'{nameof(location)}' cannot be empty.", nameof(location));
        }

#pragma warning disable CA1308 // Normalize strings to uppercase
        var type = typeof(T).Name.ToLowerInvariant().Replace("Resource", string.Empty, StringComparison.Ordinal);
#pragma warning restore CA1308 // Normalize strings to uppercase

        return $"{name}-{type}-{location}-{configuration.Environment}-";
    }
}

namespace Ummati.Infrastructure.Configuration;

using System;
using System.Collections.Generic;
using System.Linq;

public static class ConfigurationExtensions
{
    public static string GetAzureActiveDirectoryDescription(this IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return string.Join(Environment.NewLine, configuration.GetAzureActiveDirecoryTags());
    }

#pragma warning disable CA1002 // Do not expose generic lists
    public static List<string> GetAzureActiveDirecoryTags(this IConfiguration configuration)
#pragma warning restore CA1002 // Do not expose generic lists
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return configuration.GetTags("Azure Active Directory").Select(x => $"{x.Key}={x.Value}").ToList();
    }

    public static Dictionary<string, string> GetTags(this IConfiguration configuration, string location)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return new()
        {
            { TagName.Application, configuration.ApplicationName },
            { TagName.Environment, configuration.Environment },
            { TagName.Location, location },
        };
    }
}

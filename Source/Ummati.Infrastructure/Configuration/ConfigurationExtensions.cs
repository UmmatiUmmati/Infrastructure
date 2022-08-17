namespace Ummati.Infrastructure.Configuration;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using FluentValidation;
using Pulumi;
using Ummati.Infrastructure.Validators;

public static class ConfigurationExtensions
{
    public static string GetAzureActiveDirectoryDescription(this IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return string.Join(Environment.NewLine, configuration.GetAzureActiveDirecoryTags());
    }

    public static List<string> GetAzureActiveDirecoryTags(this IConfiguration configuration)
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

    public static void Validate(this IConfiguration configuration)
    {
        var configurationValidator = new ConfigurationValidator();
        var validationResult = configurationValidator.Validate(configuration);
        if (!validationResult.IsValid)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Validation of configuration failed.");

            var i = 1;
            foreach (var error in validationResult.Errors)
            {
                stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"{i}. {error}");
                ++i;
            }

            Log.Error(stringBuilder.ToString());
            configurationValidator.ValidateAndThrow(configuration);
        }
    }
}

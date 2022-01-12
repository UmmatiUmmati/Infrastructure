namespace Ummati.Infrastructure.Stacks;

using Pulumi;
using Pulumi.AzureNative.Resources;

#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public class AzureKubernetesStack : Stack
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
    public AzureKubernetesStack()
    {
        if (Configuration is null)
        {
            Configuration = new Configuration();
        }

        var commonResourceGroup = GetResourceGroup("common", Configuration.CommonLocation);
    }

    public static IConfiguration Configuration { get; set; } = default!;

    private static Dictionary<string, string> GetTags(string location) =>
        new()
        {
            { TagName.Application, Configuration.ApplicationName },
            { TagName.Environment, Configuration.Environment },
            { TagName.Location, location },
        };

    private static ResourceGroup GetResourceGroup(string name, string location) =>
        new(
            $"{Configuration.ApplicationName}-{name}-{location}-{Configuration.Environment}-",
            new ResourceGroupArgs()
            {
                Location = location,
                Tags = GetTags(location),
            });
}

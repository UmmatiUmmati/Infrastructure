namespace Ummati.Infrastructure.Stacks;

using System.Collections.Immutable;
using Pulumi;
using Pulumi.AzureNative.OperationalInsights;
using Pulumi.AzureNative.OperationalInsights.Inputs;
using Pulumi.AzureNative.Resources;
using Ummati.Infrastructure.Resources;

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
        var workspace = GetWorkspace(Configuration.CommonLocation, commonResourceGroup);

        var outputs = new List<Output<string>>();
        foreach (var location in Configuration.Locations)
        {
            var identityResource = new IdentityResource(
                $"identity-{location}-{Configuration.Environment}-",
                Configuration,
                location);

            var resourceGroup = GetResourceGroup("kubernetes", location);

            var virtualNetworkResource = new VirtualNetworkResource(
                $"virtualnetwork-{location}-{Configuration.Environment}-",
                Configuration,
                location,
                resourceGroup);

            var kubernetesResource = new KubernetesResource(
                $"virtualnetwork-{location}-{Configuration.Environment}-",
                Configuration,
                location,
                resourceGroup,
                identityResource,
                virtualNetworkResource);

            outputs.Add(kubernetesResource.KubeConfig);
        }

        this.KubeConfigs = Output.All(outputs.Select(x => x));
    }

    public static IConfiguration Configuration { get; set; } = default!;

    [Output]
    public Output<ImmutableArray<string>> KubeConfigs { get; private set; }

    private static ResourceGroup GetResourceGroup(string name, string location) =>
        new(
            $"{Configuration.ApplicationName}-{name}-{location}-{Configuration.Environment}-",
            new ResourceGroupArgs()
            {
                Location = location,
                Tags = Configuration.GetTags(location),
            });

    private static Workspace GetWorkspace(string location, ResourceGroup resourceGroup) =>
        new(
            $"log-analytics-{location}-{Configuration.Environment}-",
            new WorkspaceArgs()
            {
                Location = location,
                ResourceGroupName = resourceGroup.Name,
                RetentionInDays = 30,
                Sku = new WorkspaceSkuArgs()
                {
                    Name = WorkspaceSkuNameEnum.PerGB2018,
                },
                Tags = Configuration.GetTags(location),
            });
}

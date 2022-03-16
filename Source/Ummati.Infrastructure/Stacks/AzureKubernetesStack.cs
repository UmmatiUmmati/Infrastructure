namespace Ummati.Infrastructure.Stacks;

using System.Collections.Immutable;
using Pulumi;
using Pulumi.AzureNative.Resources;
using Ummati.Infrastructure.Configuration;
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
            Configuration.Validate();
        }

        var monitorResourceGroup = GetResourceGroup("monitor", Configuration.CommonLocation);
        var monitorResource = new MonitorResource(
            $"common",
            Configuration,
            Configuration.CommonLocation,
            monitorResourceGroup);

        var kubernetesResources = new List<KubernetesResource>();
        foreach (var location in Configuration.Locations)
        {
            var kubernetesResource = CreateKubernetesResource(monitorResource, location);
            kubernetesResources.Add(kubernetesResource);
        }

        this.KubeConfigs = Output.All(kubernetesResources.Select(x => x.KubeConfig));
        this.KubeFqdns = Output.All(kubernetesResources.Select(x => x.KubeFqdn));
    }

    public static IConfiguration Configuration { get; set; } = default!;

    [Output]
    public Output<ImmutableArray<string>> KubeConfigs { get; private set; }

    [Output]
    public Output<ImmutableArray<string>> KubeFqdns { get; private set; }

    private static KubernetesResource CreateKubernetesResource(
        MonitorResource monitorResource,
        string location)
    {
        var identityResource = new IdentityResource(
            $"kubernetes",
            Configuration,
            location);

        var resourceGroup = GetResourceGroup("kubernetes", location);

        var virtualNetworkResource = new VirtualNetworkResource(
            "kubernetes",
            Configuration,
            location,
            resourceGroup);

        return new KubernetesResource(
            $"kubernetes",
            Configuration,
            location,
            resourceGroup,
            monitorResource,
            identityResource,
            virtualNetworkResource);
    }

    private static ResourceGroup GetResourceGroup(string name, string location) =>
        new(
            $"{Configuration.ApplicationName}-{name}-{location}-{Configuration.Environment}-",
            new ResourceGroupArgs()
            {
                Location = location,
                Tags = Configuration.GetTags(location),
            });
}

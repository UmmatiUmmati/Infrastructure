namespace Ummati.Infrastructure.Stacks;

using System.Collections.Immutable;
using Pulumi;
using Pulumi.AzureNative.Resources;
using Ummati.Infrastructure.Configuration;
using Ummati.Infrastructure.Resources;

#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public class MainStack : Stack
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
    public MainStack()
    {
        if (Configuration is null)
        {
            Configuration = new Configuration();
            Configuration.Validate();
        }

        foreach (var location in Configuration.Locations.Concat(new string[] { Configuration.CommonLocation }).Distinct())
        {
            var networkwatcherResourceGroup = GetResourceGroup("networkwatcher", Configuration.CommonLocation);
            var networkWatcherResource = new NetworkWatcherResource(
                "networkwatcher",
                Configuration,
                location,
                networkwatcherResourceGroup);
        }

        var monitorResourceGroup = GetResourceGroup("monitor", Configuration.CommonLocation);
        var monitorResource = new MonitorResource(
            $"common",
            Configuration,
            Configuration.CommonLocation,
            monitorResourceGroup);

        var bastionResourceGroup = GetResourceGroup("bastion", Configuration.CommonLocation);
        var bastionVirtualNetworkResource = new VirtualNetworkResource(
            "bastion",
            Configuration,
            Configuration.CommonLocation,
            bastionResourceGroup);
        var bastionResource = new BastionResource(
            $"common",
            Configuration,
            Configuration.CommonLocation,
            bastionResourceGroup,
            bastionVirtualNetworkResource);

        var kubernetesResources = new List<KubernetesResource>();
        foreach (var location in Configuration.Locations)
        {
            var identityResource = new IdentityResource(
                $"kubernetes",
                Configuration,
                location);

            var resourceGroup = GetResourceGroup("kubernetes", location);
            var kubernetesVirtualNetworkResource = new VirtualNetworkResource(
                "kubernetes",
                Configuration,
                location,
                resourceGroup);
            var kubernetesResource = new KubernetesResource(
                $"kubernetes",
                Configuration,
                location,
                resourceGroup,
                monitorResource,
                identityResource,
                kubernetesVirtualNetworkResource);
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

    private static ResourceGroup GetResourceGroup(string name, string location) =>
        new(
            $"{Configuration.ApplicationName}-{name}-{location}-{Configuration.Environment}-",
            new ResourceGroupArgs()
            {
                Location = location,
                Tags = Configuration.GetTags(location),
            });
}

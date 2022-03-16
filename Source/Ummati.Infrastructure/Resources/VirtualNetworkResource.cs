namespace Ummati.Infrastructure.Resources;
using Pulumi;
using Pulumi.AzureNative.Network;
using Pulumi.AzureNative.Network.Inputs;
using Pulumi.AzureNative.Resources;
using Ummati.Infrastructure.Configuration;

public class VirtualNetworkResource : ComponentResource<VirtualNetworkResource>
{
    public VirtualNetworkResource(
        string name,
        IConfiguration configuration,
        string location,
        ResourceGroup resourceGroup,
        ComponentResourceOptions? options = null)
        : base(name, configuration, location, options)
    {
        ArgumentNullException.ThrowIfNull(resourceGroup);

        var virtualNetwork = new VirtualNetwork(
            $"{name}-virtualnetwork-{location}-{configuration.Environment}-",
            new VirtualNetworkArgs()
            {
                AddressSpace = new AddressSpaceArgs()
                {
                    AddressPrefixes =
                    {
                        "10.0.0.0/16",
                    },
                },
                Location = location,
                ResourceGroupName = resourceGroup.Name,
                Tags = configuration.GetTags(location),
            });

        var subnet = new Subnet(
            $"{name}-subnet-{location}-{configuration.Environment}",
            new Pulumi.AzureNative.Network.SubnetArgs()
            {
                AddressPrefix = $"10.0.0.0/16",
                ResourceGroupName = resourceGroup.Name,
                VirtualNetworkName = virtualNetwork.Name,
            });

        var networkWatcher = new NetworkWatcher(
            $"{name}-networkwatcher-{location}-{configuration.Environment}-",
            new NetworkWatcherArgs()
            {
                Location = location,
                ResourceGroupName = resourceGroup.Name,
                Tags = configuration.GetTags(location),
            });

        this.SubnetId = subnet.Id;

        this.RegisterOutputs();
    }

    public Output<string> SubnetId { get; set; }
}

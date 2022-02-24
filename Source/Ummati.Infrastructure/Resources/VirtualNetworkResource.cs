namespace Ummati.Infrastructure.Resources;

using System;
using Pulumi;
using Pulumi.AzureNative.Network;
using Pulumi.AzureNative.Network.Inputs;
using Pulumi.AzureNative.Resources;

public class VirtualNetworkResource : ComponentResource
{
    public VirtualNetworkResource(
        string name,
        IConfiguration configuration,
        string location,
        ResourceGroup resourceGroup,
        ComponentResourceOptions? options = null)
#pragma warning disable CA1062 // Validate arguments of public methods
        : base($"{configuration.ApplicationName}:{nameof(VirtualNetworkResource)}", name, options)
#pragma warning restore CA1062 // Validate arguments of public methods
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(location);
        ArgumentNullException.ThrowIfNull(resourceGroup);

        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException($"'{nameof(name)}' cannot be empty.", nameof(name));
        }

        if (string.IsNullOrEmpty(location))
        {
            throw new ArgumentException($"'{nameof(location)}' cannot be empty.", nameof(location));
        }

        var virtualNetwork = new VirtualNetwork(
            $"virtualnetwork-{location}-{configuration.Environment}-",
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
            $"subnet-{location}-{configuration.Environment}",
            new Pulumi.AzureNative.Network.SubnetArgs()
            {
                AddressPrefix = $"10.0.0.0/23",
                ResourceGroupName = resourceGroup.Name,
                VirtualNetworkName = virtualNetwork.Name,
            });
        var networkWatcher = new NetworkWatcher(
            $"networkwatcher-{location}-{configuration.Environment}-",
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

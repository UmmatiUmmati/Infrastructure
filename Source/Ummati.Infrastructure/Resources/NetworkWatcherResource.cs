namespace Ummati.Infrastructure.Resources;

using Pulumi;
using Pulumi.AzureNative.Network;
using Pulumi.AzureNative.Resources;
using Ummati.Infrastructure.Configuration;

public class NetworkWatcherResource : ComponentResource<NetworkWatcherResource>
{
    public NetworkWatcherResource(
        string name,
        IConfiguration configuration,
        string location,
        ResourceGroup resourceGroup,
        ComponentResourceOptions? options = null)
        : base(name, configuration, location, options)
    {
        ArgumentNullException.ThrowIfNull(resourceGroup);

        var networkWatcher = new NetworkWatcher(
            $"networkwatcher-{location}-{configuration.Environment}-",
            new NetworkWatcherArgs()
            {
                Location = location,
                ResourceGroupName = resourceGroup.Name,
                Tags = configuration.GetTags(location),
            });
    }
}

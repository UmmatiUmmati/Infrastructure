namespace Ummati.Infrastructure.Resources;

using Pulumi;
using Pulumi.AzureNative.Network;
using Pulumi.AzureNative.Network.Inputs;
using Pulumi.AzureNative.Resources;
using Ummati.Infrastructure.Configuration;
using PublicIPAddressArgs = Pulumi.AzureNative.Network.PublicIPAddressArgs;

public class BastionResource : ComponentResource<BastionResource>
{
    public BastionResource(
        string name,
        IConfiguration configuration,
        string location,
        ResourceGroup resourceGroup,
        Output<string> subnetId,
        ComponentResourceOptions? options = null)
        : base(name, configuration, location, options)
    {
        ArgumentNullException.ThrowIfNull(resourceGroup);
        ArgumentNullException.ThrowIfNull(subnetId);

        var publicIPAddress = new PublicIPAddress(
            $"{name}-{configuration.ApplicationName}-publicipaddress-{location}-{configuration.Environment}",
            new PublicIPAddressArgs()
            {
                Location = location,
                PublicIPAllocationMethod = IPAllocationMethod.Static,
                ResourceGroupName = resourceGroup.Name,
                Sku = new PublicIPAddressSkuArgs()
                {
                    Name = PublicIPAddressSkuName.Standard,
                },
                Tags = configuration.GetTags(location),
            });
        var bastionHost = new BastionHost(
            $"{name}-{configuration.ApplicationName}-bastionhost-{location}-{configuration.Environment}",
            new BastionHostArgs()
            {
                IpConfigurations = new List<BastionHostIPConfigurationArgs>()
                {
                     new BastionHostIPConfigurationArgs()
                     {
                         PublicIPAddress = new SubResourceArgs()
                         {
                             Id = publicIPAddress.Id,
                         },
                         Subnet = new SubResourceArgs()
                         {
                             Id = subnetId,
                         },
                     },
                },
                Location = location,
                ResourceGroupName = resourceGroup.Name,

                // TODO: Select SKU once available.
                Tags = configuration.GetTags(location),
            });

        this.RegisterOutputs();
    }
}

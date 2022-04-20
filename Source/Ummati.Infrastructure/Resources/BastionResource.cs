namespace Ummati.Infrastructure.Resources;

using Pulumi;
using Pulumi.AzureNative.Resources;
using Ummati.Infrastructure.Configuration;

public class BastionResource : ComponentResource<BastionResource>
{
    public BastionResource(
        string name,
        IConfiguration configuration,
        string location,
        ResourceGroup resourceGroup,
        VirtualNetworkResource virtualNetworkResource,
        ComponentResourceOptions? options = null)
        : base(name, configuration, location, options)
    {
        ArgumentNullException.ThrowIfNull(resourceGroup);
        ArgumentNullException.ThrowIfNull(virtualNetworkResource);

        // var networkInterface = new NetworkInterface(
        //     $"{name}-{configuration.ApplicationName}-networkinterface-{location}-{configuration.Environment}",
        //     new NetworkInterfaceArgs()
        //     {
        //         IpConfigurations = new List<NetworkInterfaceIPConfigurationArgs>()
        //         {
        //              new NetworkInterfaceIPConfigurationArgs()
        //              {
        //                  Subnet = new SubnetArgs()
        //                  {
        //                      Id = virtualNetworkResource.SubnetId,
        //                  },
        //              },
        //         },
        //         Location = location,
        //         ResourceGroupName = resourceGroup.Name,
        //         Tags = configuration.GetTags(location),
        //     });
        //
        // var virtualMachine = new VirtualMachine(
        //     $"{name}-{configuration.ApplicationName}-virtualmachine-{location}-{configuration.Environment}",
        //     new VirtualMachineArgs()
        //     {
        //         HardwareProfile = new HardwareProfileArgs()
        //         {
        //             VmSize = VirtualMachineSizeTypes.Standard_B1s,
        //         },
        //         Location = location,
        //         NetworkProfile = new NetworkProfileArgs()
        //         {
        //             NetworkInterfaces = new List<NetworkInterfaceReferenceArgs>()
        //             {
        //                 new NetworkInterfaceReferenceArgs()
        //                 {
        //                     Id = networkInterface.Id,
        //                     Primary = true,
        //                 },
        //             },
        //         },
        //         OsProfile = new OSProfileArgs()
        //         {
        //             AdminPassword = "jumpboxuser",
        //             ComputerName = "jumpbox",
        //             LinuxConfiguration = new LinuxConfigurationArgs()
        //             {
        //                 DisablePasswordAuthentication = true,
        //                 PatchSettings = new LinuxPatchSettingsArgs()
        //                 {
        //                     AssessmentMode = LinuxPatchAssessmentMode.ImageDefault,
        //                 },
        //                 ProvisionVMAgent = true,
        //             },
        //         },
        //         ResourceGroupName = resourceGroup.Name,
        //         StorageProfile = new StorageProfileArgs()
        //         {
        //             ImageReference = new ImageReferenceArgs()
        //             {
        //                 Offer = "0001-com-ubuntu-server-focal",
        //                 Publisher = "canonical",
        //                 Sku = "20_04-lts-gen2",
        //                 Version = "latest",
        //             },
        //             OsDisk = new OSDiskArgs()
        //             {
        //                 Name = $"{configuration.ApplicationName}-jumpboxosdisk-{location}-{configuration.Environment}",
        //                 Caching = CachingTypes.ReadWrite,
        //                 CreateOption = DiskCreateOptionTypes.FromImage,
        //                 ManagedDisk = new ManagedDiskParametersArgs()
        //                 {
        //                     StorageAccountType = StorageAccountTypes.Premium_LRS,
        //                 },
        //             },
        //         },
        //         Tags = configuration.GetTags(location),
        //     });

        // var publicIPAddress = new Pulumi.AzureNative.Network.PublicIPAddress(
        //     "{name}-publicIPAddress",
        //     new Pulumi.AzureNative.Network.PublicIPAddressArgs()
        //     {
        //         // DnsSettings = new Pulumi.AzureNative.Network.Inputs.PublicIPAddressDnsSettingsArgs()
        //         // {
        //         //     DomainNameLabel = "dnslbl",
        //         // },
        //         Location = location,
        //         ResourceGroupName = resourceGroup.Name,
        //         Tags = configuration.GetTags(location),
        //     });
        // var bastionHost = new Pulumi.AzureNative.Network.BastionHost(
        //     $"{name}-{configuration.ApplicationName}-bastionhost-{location}-{configuration.Environment}",
        //     new Pulumi.AzureNative.Network.BastionHostArgs()
        //     {
        //         IpConfigurations = new List<Pulumi.AzureNative.Network.Inputs.BastionHostIPConfigurationArgs>()
        //         {
        //             new Pulumi.AzureNative.Network.Inputs.BastionHostIPConfigurationArgs()
        //             {
        //                 PublicIPAddress = new Pulumi.AzureNative.Network.Inputs.SubResourceArgs()
        //                 {
        //                     Id = publicIPAddress.Id,
        //                 },
        //                 Subnet = new Pulumi.AzureNative.Network.Inputs.SubResourceArgs()
        //                 {
        //                     Id = virtualNetworkResource.SubnetId,
        //                 },
        //             },
        //         },
        //         Location = location,
        //         ResourceGroupName = resourceGroup.Name,
        //         Tags = configuration.GetTags(location),
        //     });
        this.RegisterOutputs();
    }
}

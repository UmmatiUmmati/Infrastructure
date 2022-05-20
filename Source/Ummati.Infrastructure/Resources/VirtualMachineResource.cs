namespace Ummati.Infrastructure.Resources;

using Pulumi;
using Pulumi.AzureNative.Compute;
using Pulumi.AzureNative.Compute.Inputs;
using Pulumi.AzureNative.Network;
using Pulumi.AzureNative.Network.Inputs;
using Pulumi.AzureNative.Resources;
using Ummati.Infrastructure.Configuration;
using NetworkProfileArgs = Pulumi.AzureNative.Compute.Inputs.NetworkProfileArgs;
using SubnetArgs = Pulumi.AzureNative.Network.Inputs.SubnetArgs;

public class VirtualMachineResource : ComponentResource<VirtualMachineResource>
{
    public VirtualMachineResource(
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

        var networkInterface = new NetworkInterface(
            $"{name}-{configuration.ApplicationName}-networkinterface-{location}-{configuration.Environment}",
            new NetworkInterfaceArgs()
            {
                IpConfigurations = new List<NetworkInterfaceIPConfigurationArgs>()
                {
                      new NetworkInterfaceIPConfigurationArgs()
                      {
                          PrivateIPAllocationMethod = IPAllocationMethod.Dynamic,
                          Subnet = new SubnetArgs()
                          {
                              Id = subnetId,
                          },
                      },
                },
                Location = location,
                ResourceGroupName = resourceGroup.Name,
                Tags = configuration.GetTags(location),
            });
        var virtualMachine = new VirtualMachine(
            $"{name}-{configuration.ApplicationName}-virtualmachine-{location}-{configuration.Environment}",
            new VirtualMachineArgs()
            {
                HardwareProfile = new HardwareProfileArgs()
                {
                    VmSize = VirtualMachineSizeTypes.Standard_B1s,
                },
                Location = location,
                NetworkProfile = new NetworkProfileArgs()
                {
                    NetworkInterfaces = new List<NetworkInterfaceReferenceArgs>()
                    {
                         new NetworkInterfaceReferenceArgs()
                         {
                             Id = networkInterface.Id,
                             Primary = true,
                         },
                    },
                },
                OsProfile = new OSProfileArgs()
                {
                    AdminUsername = "jumpboxuser",
                    AdminPassword = "jumpboxuser",
                    ComputerName = "jumpbox",
                    LinuxConfiguration = new LinuxConfigurationArgs()
                    {
                        DisablePasswordAuthentication = true,
                        PatchSettings = new LinuxPatchSettingsArgs()
                        {
                            AssessmentMode = LinuxPatchAssessmentMode.ImageDefault,
                        },
                        ProvisionVMAgent = true,
                        Ssh = new SshConfigurationArgs()
                        {
                        },
                    },
                },
                ResourceGroupName = resourceGroup.Name,
                StorageProfile = new StorageProfileArgs()
                {
                    ImageReference = new ImageReferenceArgs()
                    {
                        Offer = "0001-com-ubuntu-server-focal",
                        Publisher = "canonical",
                        Sku = "20_04-lts-gen2",
                        Version = "latest",
                    },
                    OsDisk = new OSDiskArgs()
                    {
                        Name = $"{configuration.ApplicationName}-jumpboxosdisk-{location}-{configuration.Environment}",
                        Caching = CachingTypes.ReadWrite,
                        CreateOption = DiskCreateOptionTypes.FromImage,
                        ManagedDisk = new ManagedDiskParametersArgs()
                        {
                            StorageAccountType = StorageAccountTypes.Premium_LRS,
                        },
                    },
                },
                Tags = configuration.GetTags(location),
            });

        this.RegisterOutputs();
    }
}

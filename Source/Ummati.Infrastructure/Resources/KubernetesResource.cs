namespace Ummati.Infrastructure.Resources;

using System.Text;
using Pulumi;
using Pulumi.AzureNative.ContainerService;
using Pulumi.AzureNative.ContainerService.Inputs;
using Pulumi.AzureNative.Resources;

public class KubernetesResource : ComponentResource
{
    public KubernetesResource(
        string name,
        IConfiguration configuration,
        string location,
        ResourceGroup resourceGroup,
        CommonResource commonResource,
        IdentityResource identityResource,
        VirtualNetworkResource virtualNetworkResource,
        ComponentResourceOptions? options = null)
#pragma warning disable CA1062 // Validate arguments of public methods
        : base($"{configuration.ApplicationName}:{nameof(KubernetesResource)}", name, options)
#pragma warning restore CA1062 // Validate arguments of public methods
    {
        Validate(name, location, resourceGroup, commonResource, identityResource, virtualNetworkResource);

        var managedCluster = new ManagedCluster(
            $"kubernetes-{location}-{configuration.Environment}-",
            new ManagedClusterArgs()
            {
                ResourceGroupName = resourceGroup.Name,
                AgentPoolProfiles = new InputList<ManagedClusterAgentPoolProfileArgs>()
                {
                    new ManagedClusterAgentPoolProfileArgs()
                    {
                        Count = configuration.KubernetesNodeCount,
                        MaxPods = configuration.KubernetesMaximumPods,
                        Mode = AgentPoolMode.System,
                        Name = "default",
                        OsDiskSizeGB = configuration.KubernetesOsDiskSizeGB,
                        OsType = OSType.Linux,
                        ScaleSetEvictionPolicy = configuration.KubernetesScaleSetEvictionPolicy,
                        Tags = configuration.GetTags(location),
                        Type = AgentPoolType.VirtualMachineScaleSets,
                        UpgradeSettings = new AgentPoolUpgradeSettingsArgs()
                        {
                            MaxSurge = configuration.KubernetesMaximumSurge,
                        },
                        VmSize = configuration.KubernetesVmSize,
                        VnetSubnetID = virtualNetworkResource.SubnetId,
                    },
                },
                DnsPrefix = "AzureNativeprovider",
                EnableRBAC = true,
                Tags = configuration.GetTags(location),

                // KubernetesVersion = "1.22.4", // You can only upgrade one minor version at a time.
                NodeResourceGroup = $"{configuration.ApplicationName}-kubernetesnodes-{location}-{configuration.Environment}",
                NetworkProfile = new ContainerServiceNetworkProfileArgs()
                {
                    NetworkPlugin = NetworkPlugin.Azure,
                    DnsServiceIP = "10.0.2.254",
                    ServiceCidr = "10.0.2.0/24",
                    DockerBridgeCidr = "172.17.0.1/16",
                },
                ServicePrincipalProfile = new ManagedClusterServicePrincipalProfileArgs
                {
                    ClientId = identityResource.ClientId,
                    Secret = identityResource.ClientSecret,
                },

                // AddonProfiles = new InputMap<ManagedClusterAddonProfileArgs>()
                // {
                //     {
                //         "omsAgent",
                //         new ManagedClusterAddonProfileArgs()
                //         {
                //             Enabled = true,
                //             Config = new InputMap<string>()
                //             {
                //                 { "logAnalyticsWorkspaceId", commonResource.WorkspaceId },
                //             },
                //         },
                //     },
                // },
            });

        var maintenanceConfiguration = new MaintenanceConfiguration(
            $"maintenanceconfiguration-{location}-{configuration.Environment}",
            new MaintenanceConfigurationArgs()
            {
                ResourceGroupName = resourceGroup.Name,
                ResourceName = managedCluster.Name,
                TimeInWeek = configuration.KubernetesMaintenanceDays
                    .Select(day =>
                        new TimeInWeekArgs()
                        {
                            Day = day,
                            HourSlots = configuration.KubernetesMaintenanceHourSlots.ToList(),
                        })
                    .ToList(),
            });

        this.KubeConfig = GetKubeConfig(resourceGroup.Name, managedCluster.Name);

        this.RegisterOutputs();
    }

    public Output<string> KubeConfig { get; set; }

    private static void Validate(
        string name,
        string location,
        ResourceGroup resourceGroup,
        CommonResource commonResource,
        IdentityResource identityResource,
        VirtualNetworkResource virtualNetworkResource)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(location);
        ArgumentNullException.ThrowIfNull(resourceGroup);
        ArgumentNullException.ThrowIfNull(commonResource);
        ArgumentNullException.ThrowIfNull(identityResource);
        ArgumentNullException.ThrowIfNull(virtualNetworkResource);

        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException($"'{nameof(name)}' cannot be empty.", nameof(name));
        }

        if (string.IsNullOrEmpty(location))
        {
            throw new ArgumentException($"'{nameof(location)}' cannot be empty.", nameof(location));
        }
    }

    private static Output<string> GetKubeConfig(
        Output<string> resourceGroupName,
        Output<string> clusterName)
    {
        var output = ListManagedClusterUserCredentials.Invoke(
            new ListManagedClusterUserCredentialsInvokeArgs()
            {
                ResourceGroupName = resourceGroupName,
                ResourceName = clusterName,
            });
        return output
            .Apply(credentials =>
            {
                try
                {
                    var base64EncodedKubeConfig = credentials.Kubeconfigs.First().Value;
                    var kubeConfig = Convert.FromBase64String(base64EncodedKubeConfig);
                    return Encoding.UTF8.GetString(kubeConfig);
                }
                catch (NullReferenceException)
                {
                    // Returned in tests.
                    return string.Empty;
                }
            })
            .Apply(Output.CreateSecret);
    }
}

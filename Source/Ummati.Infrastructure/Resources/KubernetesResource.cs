namespace Ummati.Infrastructure.Resources;

using System.Globalization;
using System.Text;
using Pulumi;
using Pulumi.AzureNative.Compute;
using Pulumi.AzureNative.ContainerService.V20220101;
using Pulumi.AzureNative.ContainerService.V20220101.Inputs;
using Pulumi.AzureNative.Resources;
using Ummati.Infrastructure.Configuration;

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

        var nodePoolProfiles = new List<ManagedClusterAgentPoolProfileArgs>();
        foreach (var nodePoolGroup in configuration.Kubernetes.NodePools.GroupBy(x => x.Type))
        {
            var index = 1;
            foreach (var nodePool in nodePoolGroup)
            {
                nodePoolProfiles.AddRange(
                    GetManagedClusterAgentPoolProfileArgs(
                        index,
                        location,
                        resourceGroup,
                        configuration,
                        nodePool,
                        virtualNetworkResource));
                ++index;
            }
        }

        var managedCluster = new ManagedCluster(
            $"kubernetes-{location}-{configuration.Environment}-",
            new ManagedClusterArgs()
            {
                ResourceGroupName = resourceGroup.Name,
                AgentPoolProfiles = nodePoolProfiles,
                AutoUpgradeProfile = new ManagedClusterAutoUpgradeProfileArgs()
                {
                    UpgradeChannel = configuration.Kubernetes.InternalUpgradeChannel,
                },
                ApiServerAccessProfile = new ManagedClusterAPIServerAccessProfileArgs()
                {
                    EnablePrivateCluster = true,
                    EnablePrivateClusterPublicFQDN = true,
                    PrivateDNSZone = "none",

                    // DisableRunCommand = true,
                },
                DnsPrefix = configuration.ApplicationName,
                EnableRBAC = true,
                Tags = configuration.GetTags(location),
                NodeResourceGroup = $"{configuration.ApplicationName}-kubernetesnodes-{location}-{configuration.Environment}",
                NetworkProfile = new ContainerServiceNetworkProfileArgs()
                {
                    NetworkPlugin = NetworkPlugin.Azure,
                    DnsServiceIP = "10.1.0.10",
                    ServiceCidr = "10.1.0.0/16",
                    DockerBridgeCidr = "172.17.0.1/16",
                    LoadBalancerSku = LoadBalancerSku.Standard,
                },
                ServicePrincipalProfile = new ManagedClusterServicePrincipalProfileArgs
                {
                    ClientId = identityResource.ClientId,
                    Secret = identityResource.ClientSecret,
                },
                Sku = new ManagedClusterSKUArgs()
                {
                    Name = ManagedClusterSKUName.Basic,
                    Tier = configuration.Kubernetes.InternalSKUTier,
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
                TimeInWeek = configuration.Kubernetes.Maintenance
                    .SelectMany(maintenance => maintenance
                        .InternalDays
                        .Select(day =>
                            new TimeInWeekArgs()
                            {
                                Day = day,
                                HourSlots = maintenance.HourSlots.ToList(),
                            }))
                    .ToList(),
            });

        this.KubeConfig = GetKubeConfig(resourceGroup.Name, managedCluster.Name);
        this.KubeFqdn = managedCluster.Fqdn;

        this.RegisterOutputs();
    }

    public Output<string> KubeConfig { get; set; }

    public Output<string> KubeFqdn { get; set; }

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

    private static IEnumerable<ManagedClusterAgentPoolProfileArgs> GetManagedClusterAgentPoolProfileArgs(
        int index,
        string location,
        ResourceGroup resourceGroup,
        IConfiguration configuration,
        KubernetesClusterNodePool kubernetesClusterNodePool,
        VirtualNetworkResource virtualNetworkResource)
    {
        if (kubernetesClusterNodePool.AvailabilityZones is not null &&
            kubernetesClusterNodePool.Type is KubernetesClusterNodePoolType.System or KubernetesClusterNodePoolType.User &&
            Azure.LocationsSupportingAvailabilityZones.Contains(location))
        {
            foreach (var availabilityZone in kubernetesClusterNodePool.AvailabilityZones)
            {
#pragma warning disable CA1308 // Normalize strings to uppercase
                var name = $"{kubernetesClusterNodePool.Type.ToString().ToLowerInvariant()}{index}az{availabilityZone}";
#pragma warning restore CA1308 // Normalize strings to uppercase
                var proximityPlacementGroup = new ProximityPlacementGroup(
                    $"proximityplacementgroup-{name}-{location}-{configuration.Environment}-",
                    new ProximityPlacementGroupArgs()
                    {
                        Location = location,
                        ProximityPlacementGroupType = ProximityPlacementGroupType.Standard,
                        ResourceGroupName = resourceGroup.Name,
                        Tags = configuration.GetTags(location),
                    });

                yield return GetManagedClusterAgentPoolProfileArgs(
                    name,
                    location,
                    configuration,
                    kubernetesClusterNodePool,
                    virtualNetworkResource,
                    availabilityZone,
                    proximityPlacementGroup);
            }
        }
        else
        {
            yield return GetManagedClusterAgentPoolProfileArgs(
#pragma warning disable CA1308 // Normalize strings to uppercase
                $"{kubernetesClusterNodePool.Type.ToString().ToLowerInvariant()}{index}",
#pragma warning restore CA1308 // Normalize strings to uppercase
                location,
                configuration,
                kubernetesClusterNodePool,
                virtualNetworkResource);
        }
    }

    private static ManagedClusterAgentPoolProfileArgs GetManagedClusterAgentPoolProfileArgs(
        string name,
        string location,
        IConfiguration configuration,
        KubernetesClusterNodePool kubernetesClusterNodePool,
        VirtualNetworkResource virtualNetworkResource,
        int? availabilityZone = null,
        ProximityPlacementGroup? proximityPlacementGroup = null)
    {
        var managedClusterAgentPoolProfileArgs = new ManagedClusterAgentPoolProfileArgs()
        {
            Count = kubernetesClusterNodePool.MinimumNodeCount,
            EnableAutoScaling = true,
            MaxCount = kubernetesClusterNodePool.MaximumNodeCount,
            MaxPods = kubernetesClusterNodePool.MaximumPods,
            MinCount = kubernetesClusterNodePool.MinimumNodeCount,
            Mode = kubernetesClusterNodePool.InternalMode,
            Name = name,
            NodeLabels = new Dictionary<string, string>()
            {
                { $"{configuration.ApplicationName}.com/application", configuration.ApplicationName },
                { $"{configuration.ApplicationName}.com/environment", configuration.Environment },
            },
            OsDiskSizeGB = kubernetesClusterNodePool.OsDiskSizeGB,
            OsDiskType = kubernetesClusterNodePool.InternalOSDiskType,
            OsSKU = OSSKU.Ubuntu,
            OsType = OSType.Linux,
            ScaleSetEvictionPolicy = kubernetesClusterNodePool.InternalScaleSetEvictionPolicy,
            Tags = configuration.GetTags(location),
            Type = AgentPoolType.VirtualMachineScaleSets,
            UpgradeSettings = new AgentPoolUpgradeSettingsArgs()
            {
                MaxSurge = kubernetesClusterNodePool.MaximumSurge,
            },
            VmSize = kubernetesClusterNodePool.VmSize,
            VnetSubnetID = virtualNetworkResource.SubnetId,
        };

        if (availabilityZone is not null)
        {
            managedClusterAgentPoolProfileArgs.AvailabilityZones =
                new string[] { availabilityZone.Value.ToString(CultureInfo.InvariantCulture) };
        }

        if (proximityPlacementGroup is not null)
        {
            managedClusterAgentPoolProfileArgs.ProximityPlacementGroupID = proximityPlacementGroup.Id;
        }

        if (kubernetesClusterNodePool.Type is KubernetesClusterNodePoolType.System)
        {
            managedClusterAgentPoolProfileArgs.NodeTaints = new List<string>()
            {
                { "CriticalAddonsOnly=true:NoSchedule" },
            };
        }

        return managedClusterAgentPoolProfileArgs;
    }
}

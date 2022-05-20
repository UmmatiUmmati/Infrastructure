namespace Ummati.Infrastructure.Resources;

using System.Globalization;
using System.Text;
using Pulumi;
using Pulumi.AzureNative.Compute;
using Pulumi.AzureNative.ContainerService.V20220101;
using Pulumi.AzureNative.ContainerService.V20220101.Inputs;
using Pulumi.AzureNative.Resources;
using Ummati.Infrastructure.Configuration;

public class KubernetesResource : ComponentResource<KubernetesResource>
{
    private const int PortsPerIP = 64_000;

    public KubernetesResource(
        string name,
        IConfiguration configuration,
        string location,
        ResourceGroup resourceGroup,
        MonitorResource monitorResource,
        IdentityResource identityResource,
        Output<string> subnetId,
        ComponentResourceOptions? options = null)
         : base(name, configuration, location, options)
    {
        ArgumentNullException.ThrowIfNull(resourceGroup);
        ArgumentNullException.ThrowIfNull(monitorResource);
        ArgumentNullException.ThrowIfNull(identityResource);
        ArgumentNullException.ThrowIfNull(subnetId);

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
                        subnetId));
                ++index;
            }
        }

        var totalNodeCountPossible = configuration.Kubernetes.NodePools.Select(x => x.MaximumNodeCountPossible).Sum();
        var outboundIPCount = Math.Max(1, totalNodeCountPossible / (PortsPerIP / configuration.Kubernetes.LoadBalancer.PortsPerNode));
        Log.Info($"{totalNodeCountPossible} nodes possible including surge.\r\n{configuration.Kubernetes.LoadBalancer.PortsPerNode} ports per node.\r\n{outboundIPCount} outbound load balancer IP's.");

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

                    // Manged NAT Gateways
                    // Can be used as an alternative to a load balancer for outbound connection.
                    // https://docs.microsoft.com/en-us/azure/aks/nat-gateway
                    // OutboundType = OutboundType.ManagedNATGateway,
                    // NatGatewayProfile = new ManagedClusterNATGatewayProfileArgs()
                    // {
                    //     IdleTimeoutInMinutes = configuration.Kubernetes.LoadBalancer.IdleTimeoutInMinutes,
                    //     ManagedOutboundIPProfile = new ManagedClusterManagedOutboundIPProfileArgs()
                    //     {
                    //         Count = outboundIPCount,
                    //     },
                    // },
                    LoadBalancerProfile = new ManagedClusterLoadBalancerProfileArgs()
                    {
                        AllocatedOutboundPorts = configuration.Kubernetes.LoadBalancer.PortsPerNode,
                        IdleTimeoutInMinutes = configuration.Kubernetes.LoadBalancer.IdleTimeoutInMinutes,
                        ManagedOutboundIPs = new ManagedClusterLoadBalancerProfileManagedOutboundIPsArgs()
                        {
                            Count = outboundIPCount,
                        },
                    },
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
        Output<string> subnetId)
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
                    subnetId,
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
                subnetId);
        }
    }

    private static ManagedClusterAgentPoolProfileArgs GetManagedClusterAgentPoolProfileArgs(
        string name,
        string location,
        IConfiguration configuration,
        KubernetesClusterNodePool kubernetesClusterNodePool,
        Output<string> subnetId,
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
            VnetSubnetID = subnetId,
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

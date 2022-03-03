namespace Ummati.Infrastructure.Test;

using System.Collections.Generic;
using Pulumi.AzureNative.ContainerService;

public class TestConfiguration : IConfiguration
{
    public string ApplicationName { get; init; } = default!;

    public string Environment { get; init; } = default!;

    public string CommonLocation { get; init; } = default!;

    public IEnumerable<string> Locations { get; init; } = default!;

    public IEnumerable<WeekDay> KubernetesMaintenanceDays { get; set; } = default!;

    public IEnumerable<int> KubernetesMaintenanceHourSlots { get; set; } = default!;

    public ManagedClusterSKUTier KubernetesSKUTier { get; set; } = default!;

    public UpgradeChannel KubernetesUpgradeChannel { get; set; } = default!;

    public IEnumerable<string> KubernetesSystemNodesAvailabilityZones { get; set; } = default!;

    public int KubernetesSystemNodesMaximumPods { get; set; } = default!;

    public int KubernetesSystemNodesMaximumNodeCount { get; set; } = default!;

    public string KubernetesSystemNodesMaximumSurge { get; set; } = default!;

    public int KubernetesSystemNodesMinimumNodeCount { get; set; } = default!;

    public int KubernetesSystemNodesOsDiskSizeGB { get; set; } = default!;

    public OSDiskType KubernetesSystemNodesOSDiskType { get; set; } = default!;

    public ScaleSetEvictionPolicy KubernetesSystemNodesScaleSetEvictionPolicy { get; set; } = default!;

    public string KubernetesSystemNodesVmSize { get; set; } = default!;

    public IEnumerable<string> KubernetesUserNodesAvailabilityZones { get; set; } = default!;

    public int KubernetesUserNodesMaximumPods { get; set; } = default!;

    public int KubernetesUserNodesMaximumNodeCount { get; set; } = default!;

    public string KubernetesUserNodesMaximumSurge { get; set; } = default!;

    public int KubernetesUserNodesMinimumNodeCount { get; set; } = default!;

    public int KubernetesUserNodesOsDiskSizeGB { get; set; } = default!;

    public OSDiskType KubernetesUserNodesOSDiskType { get; set; } = default!;

    public ScaleSetEvictionPolicy KubernetesUserNodesScaleSetEvictionPolicy { get; set; } = default!;

    public string KubernetesUserNodesVmSize { get; set; } = default!;

    public string ContainerImageName { get; init; } = default!;

    public double ContainerCpu { get; init; } = default!;

    public string ContainerMemory { get; init; } = default!;

    public int ContainerMaxReplicas { get; init; } = default!;

    public int ContainerMinReplicas { get; init; } = default!;

    public int ContainerConcurrentRequests { get; init; } = default!;
}

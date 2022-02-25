namespace Ummati.Infrastructure.Test;

using System.Collections.Generic;
using Pulumi.AzureNative.ContainerService;

public class TestConfiguration : IConfiguration
{
    public string ApplicationName { get; init; } = default!;

    public string Environment { get; init; } = default!;

    public string CommonLocation { get; init; } = default!;

    public IEnumerable<string> Locations { get; init; } = default!;

    public int KubernetesMaximumPods { get; set; } = default!;

    public int KubernetesMaximumNodeCount { get; set; } = default!;

    public string KubernetesMaximumSurge { get; set; } = default!;

    public int KubernetesMinimumNodeCount { get; set; } = default!;

    public int KubernetesOsDiskSizeGB { get; set; } = default!;

    public ScaleSetEvictionPolicy KubernetesScaleSetEvictionPolicy { get; set; } = default!;

    public IEnumerable<WeekDay> KubernetesMaintenanceDays { get; set; } = default!;

    public IEnumerable<int> KubernetesMaintenanceHourSlots { get; set; } = default!;

    public ManagedClusterSKUTier KubernetesSKUTier { get; set; } = default!;

    public UpgradeChannel KubernetesUpgradeChannel { get; set; } = default!;

    public string KubernetesVmSize { get; set; } = default!;

    public string ContainerImageName { get; init; } = default!;

    public double ContainerCpu { get; init; } = default!;

    public string ContainerMemory { get; init; } = default!;

    public int ContainerMaxReplicas { get; init; } = default!;

    public int ContainerMinReplicas { get; init; } = default!;

    public int ContainerConcurrentRequests { get; init; } = default!;
}

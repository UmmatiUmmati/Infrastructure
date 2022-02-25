namespace Ummati.Infrastructure;

using System.Collections.Generic;
using Pulumi.AzureNative.ContainerService;

public interface IConfiguration
{
    /// <summary>
    /// Gets the application name.
    /// </summary>
    string ApplicationName { get; }

    /// <summary>
    /// Gets the environment name e.g. development, production.
    /// </summary>
    string Environment { get; }

    /// <summary>
    /// Gets the location where common resources can be placed.
    /// </summary>
    string CommonLocation { get; }

    /// <summary>
    /// Gets one or more locations where resources can be duplicated.
    /// </summary>
    IEnumerable<string> Locations { get; }

    /// <summary>
    /// Gets the days that maintenance is allowed on the cluster.
    /// </summary>
    IEnumerable<WeekDay> KubernetesMaintenanceDays { get; }

    /// <summary>
    /// Gets the hours of the day that maintenance is allowed on the cluster.
    /// </summary>
    IEnumerable<int> KubernetesMaintenanceHourSlots { get; }

    /// <summary>
    /// Gets the maximum number of pods allowed on a node. Minimum is one and maximum is 250.
    /// </summary>
    int KubernetesMaximumPods { get; }

    /// <summary>
    /// Gets the maximum number of nodes allowed in the cluster. Maximum is 100.
    /// </summary>
    int KubernetesMaximumNodeCount { get; }

    /// <summary>
    /// Gets the maximum number of nodes to update at any one time. This can be a number e.g. 10 or a
    /// percentage e.g. 10%. 33% is recommended and you need enough IP addresses available on your subnet to support
    /// the extra nodes during the upgrade.
    /// </summary>
    string KubernetesMaximumSurge { get; }

    /// <summary>
    /// Gets the minimum number of nodes in the cluster. Minimum is zero. A minimum of three is recommended
    /// in production.
    /// </summary>
    int KubernetesMinimumNodeCount { get; }

    /// <summary>
    /// Gets the size of the disk used by the nodes operating system.
    /// </summary>
    int KubernetesOsDiskSizeGB { get; }

    /// <summary>
    /// Gets the policy used to scale down the number of nodes.
    /// Delete (Default) - Stops nodes when scaling down. This is faster to scale up.
    /// Deallocate - Stops and deletes nodes when scaling down. This is cheaper.
    /// </summary>
    ScaleSetEvictionPolicy KubernetesScaleSetEvictionPolicy { get; }

    /// <summary>
    /// Gets the tier of the cluster SKU. The Paid tier results in higher availability.
    /// </summary>
    ManagedClusterSKUTier KubernetesSKUTier { get; }

    /// <summary>
    /// Gets the upgrade channel used to upgrade the cluster automatically.
    /// </summary>
    UpgradeChannel KubernetesUpgradeChannel { get; }

    /// <summary>
    /// Gets the virtual machine size. DS3_v2 is the minimum recommended and DS4_v2 is recommended in production.
    /// </summary>
    string KubernetesVmSize { get; }

    string ContainerImageName { get; }

    double ContainerCpu { get; }

    string ContainerMemory { get; }

    int ContainerMaxReplicas { get; }

    int ContainerMinReplicas { get; }

    int ContainerConcurrentRequests { get; }
}

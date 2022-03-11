namespace Ummati.Infrastructure.Configuration;

using System.Collections.Generic;
using Pulumi.AzureNative.ContainerService;
using Ummati.Infrastructure.Configuration.Finished;

public class KubernetesClusterNodePool
{
    /// <summary>
    /// Gets or sets the availability zones that the node pool should reside in. If there are none, then no availability zones
    /// are specified.
    /// </summary>
    public IEnumerable<int> AvailabilityZones { get; set; } = default!;

    /// <summary>
    /// Gets or sets the maximum number of nodes allowed in the cluster. Maximum is 100.
    /// </summary>
    public int MaximumNodeCount { get; set; } = default!;

    /// <summary>
    /// Gets or sets the maximum number of pods allowed on a node. Minimum is one and maximum is 250.
    /// </summary>
    public int MaximumPods { get; set; } = default!;

    /// <summary>
    /// Gets or sets the maximum number of nodes to update at any one time. This can be a number e.g. 10 or a
    /// percentage e.g. 10%. 33% is recommended and you need enough IP addresses available on your subnet to support
    /// the extra nodes during the upgrade.
    /// </summary>
    public string MaximumSurge { get; set; } = default!;

    /// <summary>
    /// Gets or sets the minimum number of nodes in the cluster. Minimum is zero. A minimum of three is recommended
    /// in production.
    /// </summary>
    public int MinimumNodeCount { get; set; } = default!;

    /// <summary>
    /// Gets or sets the size of the disk used by the nodes operating system.
    /// </summary>
    public int OsDiskSizeGB { get; set; } = default!;

    /// <summary>
    /// Gets or sets the type of the disk used by the nodes. Ephemeral is cheaper and faster but requires a VM size which
    /// supports 32GB of temporary storage.
    /// </summary>
    public KubernetesNodeOSDiskType OSDiskType { get; set; } = default!;

    /// <summary>
    /// Gets or sets the policy used to scale down the number of nodes.
    /// Delete (Default) - Stops nodes when scaling down. This is faster to scale up.
    /// Deallocate - Stops and deletes nodes when scaling down. This is cheaper.
    /// </summary>
    public KubernetesNodeScaleSetEvictionPolicy ScaleSetEvictionPolicy { get; set; } = default!;

    /// <summary>
    /// Gets or sets the type of the node pool.
    /// </summary>
    public KubernetesClusterNodePoolType Type { get; set; } = default!;

    /// <summary>
    /// Gets or sets the virtual machine size. DS3_v2 is the minimum recommended and DS4_v2 is recommended in production.
    /// </summary>
    public string VmSize { get; set; } = default!;

    internal AgentPoolMode InternalMode =>
        this.Type switch
        {
            KubernetesClusterNodePoolType.System => AgentPoolMode.System,
            KubernetesClusterNodePoolType.User => AgentPoolMode.User,
            KubernetesClusterNodePoolType.Spot => AgentPoolMode.User,
            _ => throw new InvalidOperationException($"{nameof(KubernetesClusterNodePoolType)} '{this.Type}' not recognised."),
        };

    internal OSDiskType InternalOSDiskType =>
        this.OSDiskType switch
        {
            KubernetesNodeOSDiskType.Managed => Pulumi.AzureNative.ContainerService.OSDiskType.Managed,
            KubernetesNodeOSDiskType.Ephemeral => Pulumi.AzureNative.ContainerService.OSDiskType.Ephemeral,
            _ => throw new InvalidOperationException($"{nameof(KubernetesNodeOSDiskType)} '{this.OSDiskType}' not recognised."),
        };

    internal ScaleSetEvictionPolicy InternalScaleSetEvictionPolicy =>
        this.ScaleSetEvictionPolicy switch
        {
            KubernetesNodeScaleSetEvictionPolicy.Delete => Pulumi.AzureNative.ContainerService.ScaleSetEvictionPolicy.Delete,
            KubernetesNodeScaleSetEvictionPolicy.Deallocate => Pulumi.AzureNative.ContainerService.ScaleSetEvictionPolicy.Deallocate,
            _ => throw new InvalidOperationException($"{nameof(KubernetesNodeScaleSetEvictionPolicy)} '{this.ScaleSetEvictionPolicy}' not recognised."),
        };
}

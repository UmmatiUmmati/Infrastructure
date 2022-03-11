namespace Ummati.Infrastructure.Configuration;

using Pulumi.AzureNative.ContainerService;
using Ummati.Infrastructure.Configuration.Finished;

public class KubernetesCluster
{
    /// <summary>
    /// Gets or sets a collection of allowed days and times when scheduled maintenance on the cluster is allowed to occur.
    /// </summary>
    public IEnumerable<KubernetesClusterMaintenance> Maintenance { get; set; } = default!;

    /// <summary>
    /// Gets or sets the tier of the cluster SKU. The Paid tier results in higher availability.
    /// </summary>
    public KubernetesClusterSKUTier SKUTier { get; set; } = default!;

    /// <summary>
    /// Gets or sets the upgrade channel used to upgrade the cluster automatically.
    /// </summary>
    public KubernetesClusterUpgradeChannel UpgradeChannel { get; set; } = default!;

    /// <summary>
    /// Gets or sets the system node pool under which all Kubernetes Kubelet pods are run and no user workloads are run.
    /// </summary>
    public IEnumerable<KubernetesClusterNodePool> NodePools { get; set; } = default!;

    internal ManagedClusterSKUTier InternalSKUTier =>
        this.SKUTier switch
        {
            KubernetesClusterSKUTier.Free => ManagedClusterSKUTier.Free,
            KubernetesClusterSKUTier.Paid => ManagedClusterSKUTier.Paid,
            _ => throw new InvalidOperationException($"{nameof(KubernetesClusterSKUTier)} '{this.SKUTier}' not recognised."),
        };

    internal UpgradeChannel InternalUpgradeChannel =>
        this.UpgradeChannel switch
        {
            KubernetesClusterUpgradeChannel.Rapid => Pulumi.AzureNative.ContainerService.UpgradeChannel.Rapid,
            KubernetesClusterUpgradeChannel.Stable => Pulumi.AzureNative.ContainerService.UpgradeChannel.Stable,
            KubernetesClusterUpgradeChannel.Patch => Pulumi.AzureNative.ContainerService.UpgradeChannel.Patch,
            KubernetesClusterUpgradeChannel.NodeImage => Pulumi.AzureNative.ContainerService.UpgradeChannel.Node_image,
            KubernetesClusterUpgradeChannel.None => Pulumi.AzureNative.ContainerService.UpgradeChannel.None,
            _ => throw new InvalidOperationException($"{nameof(KubernetesClusterUpgradeChannel)} '{this.UpgradeChannel}' not recognised."),
        };
}

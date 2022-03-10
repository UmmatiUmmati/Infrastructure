namespace Ummati.Infrastructure.Configuration;

public enum KubernetesClusterNodePoolType
{
    /// <summary>
    /// System node pools are those under which only Kubernetes pods are run and no user workloads are run.
    /// </summary>
    System,

    /// <summary>
    /// User node pool are those under which all user workloads are run.
    /// </summary>
    User,

    /// <summary>
    /// Spot node pool are those which uses cheap but temporary Azure spot virtual machines which can be used to run
    /// batch workloads.
    /// </summary>
    Spot,
}

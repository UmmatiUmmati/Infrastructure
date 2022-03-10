namespace Ummati.Infrastructure.Configuration.Finished;

public enum KubernetesClusterUpgradeChannel
{
    Rapid,
    Stable,
    Patch,
    NodeImage,
    None,
}

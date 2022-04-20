namespace Ummati.Infrastructure.Configuration;

public enum KubernetesClusterUpgradeChannel
{
    Rapid,
    Stable,
    Patch,
    NodeImage,
    None,
}

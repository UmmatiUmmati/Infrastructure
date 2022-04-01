namespace Ummati.Infrastructure.Configuration;

public class KubernetesClusterLoadBalancer
{
    public int IdleTimeoutInMinutes { get; set; } = default!;

    public int PortsPerNode { get; set; } = default!;
}

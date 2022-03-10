namespace Ummati.Infrastructure.Configuration;

using Pulumi;

#pragma warning disable CA1724 // Conflicts with System.Configuration
public class Configuration : IConfiguration
#pragma warning restore CA1724 // Conflicts with System.Configuration
{
    private readonly Config config = new();

    public string ApplicationName => this.config.GetString(nameof(this.ApplicationName));

    public string Environment => this.config.GetString(nameof(this.Environment));

    public string CommonLocation => this.config.GetString(nameof(this.CommonLocation));

    public IEnumerable<string> Locations => this.config.GetFromJson<List<string>>(nameof(this.Locations));

    public KubernetesCluster Kubernetes => this.config.GetFromJson<KubernetesCluster>(nameof(this.Kubernetes));
}

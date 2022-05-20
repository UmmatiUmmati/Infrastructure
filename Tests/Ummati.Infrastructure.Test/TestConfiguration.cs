namespace Ummati.Infrastructure.Test;

using System.Collections.Generic;
using Ummati.Infrastructure.Configuration;

public class TestConfiguration : IConfiguration
{
    public string ApplicationName { get; set; } = default!;

    public string Environment { get; set; } = default!;

    public string CommonLocation { get; set; } = default!;

    public IEnumerable<string> Locations { get; set; } = default!;

    public KubernetesCluster Kubernetes { get; set; } = default!;
}

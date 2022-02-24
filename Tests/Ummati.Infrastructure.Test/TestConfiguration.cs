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

    public string KubernetesMaximumSurge { get; set; } = default!;

    public int KubernetesNodeCount { get; set; } = default!;

    public int KubernetesOsDiskSizeGB { get; set; } = default!;

    public ScaleSetEvictionPolicy KubernetesScaleSetEvictionPolicy { get; set; } = default!;

    public IEnumerable<WeekDay> KubernetesMaintenanceDays { get; set; } = default!;

    public IEnumerable<int> KubernetesMaintenanceHourSlots { get; } = default!;

    public string KubernetesVmSize { get; set; } = default!;

    public string ContainerImageName { get; init; } = default!;

    public double ContainerCpu { get; init; } = default!;

    public string ContainerMemory { get; init; } = default!;

    public int ContainerMaxReplicas { get; init; } = default!;

    public int ContainerMinReplicas { get; init; } = default!;

    public int ContainerConcurrentRequests { get; init; } = default!;

    public string GetAzureActiveDirectoryDescription() =>
        string.Join(System.Environment.NewLine, this.GetAzureActiveDirecoryTags());

    public List<string> GetAzureActiveDirecoryTags() =>
        this.GetTags("Azure Active Directory").Select(x => $"{x.Key}={x.Value}").ToList();

    public Dictionary<string, string> GetTags(string location) =>
        new()
        {
            { TagName.Application, this.ApplicationName },
            { TagName.Environment, this.Environment },
            { TagName.Location, location },
        };
}

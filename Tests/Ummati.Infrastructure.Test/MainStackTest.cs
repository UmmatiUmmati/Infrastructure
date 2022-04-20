namespace Ummati.Infrastructure.Test;

using System.Collections.Immutable;
using Pulumi;
using Pulumi.Utilities;
using Ummati.Infrastructure.Configuration;
using Ummati.Infrastructure.Stacks;
using Xunit;

public class MainStackTest
{
    [Fact]
    public async Task AllAzureActiveDirectoryResourcesHaveTagsAsync()
    {
        MainStack.Configuration = new TestConfiguration()
        {
            ApplicationName = "test-app",
            Environment = "test",
            CommonLocation = "northeurope",
            Locations = ImmutableArray.Create("northeurope", "canadacentral"),
            Kubernetes = new KubernetesCluster()
            {
                LoadBalancer = new KubernetesClusterLoadBalancer()
                {
                    IdleTimeoutInMinutes = 4,
                    PortsPerNode = 1024,
                },
                Maintenance = new KubernetesClusterMaintenance[]
                {
                    new KubernetesClusterMaintenance()
                    {
                        Days = new DayOfWeek[]
                        {
                            DayOfWeek.Saturday,
                            DayOfWeek.Sunday,
                        },
                        HourSlots = new int[]
                        {
                            2,
                            3,
                        },
                    },
                },
                NodePools = new KubernetesClusterNodePool[]
                {
                    new KubernetesClusterNodePool()
                    {
                        AvailabilityZones = new int[] { 1, 2, 3, },
                        MaximumNodeCount = 5,
                        MaximumPods = 30,
                        MaximumSurge = "33%",
                        MinimumNodeCount = 3,
                        OsDiskSizeGB = 100,
                        OSDiskType = KubernetesNodeOSDiskType.Ephemeral,
                        ScaleSetEvictionPolicy = KubernetesNodeScaleSetEvictionPolicy.Deallocate,
                        Type = KubernetesClusterNodePoolType.System,
                        VmSize = "Standard_D2ads_v5",
                    },
                },
                SKUTier = KubernetesClusterSKUTier.Paid,
                UpgradeChannel = KubernetesClusterUpgradeChannel.Stable,
            },
        };

        var resources = await Testing.RunAsync<MainStack>().ConfigureAwait(false);

        foreach (var resource in resources)
        {
            var tagsOutput = GetAzureActiveDirectoryTags(resource);
            if (tagsOutput is not null)
            {
                var tags = await OutputUtilities.GetValueAsync(tagsOutput).ConfigureAwait(false);

                Assert.NotNull(tags);
                Assert.Equal($"{TagName.Application}=test-app", tags![0]);
                Assert.Equal($"{TagName.Environment}=test", tags![1]);
                var location = tags![2];
                Assert.True(string.Equals($"{TagName.Location}=northeurope", location, StringComparison.Ordinal) ||
                    string.Equals($"{TagName.Location}=canadacentral", location, StringComparison.Ordinal));
            }
        }
    }

    [Fact]
    public async Task AllAzureResourcesHaveTagsAsync()
    {
        MainStack.Configuration = new TestConfiguration()
        {
            ApplicationName = "test-app",
            Environment = "test",
            CommonLocation = "northeurope",
            Locations = ImmutableArray.Create("northeurope", "canadacentral"),
            Kubernetes = new KubernetesCluster()
            {
                LoadBalancer = new KubernetesClusterLoadBalancer()
                {
                    IdleTimeoutInMinutes = 4,
                    PortsPerNode = 1024,
                },
                Maintenance = new KubernetesClusterMaintenance[]
                {
                    new KubernetesClusterMaintenance()
                    {
                        Days = new DayOfWeek[]
                        {
                            DayOfWeek.Saturday,
                            DayOfWeek.Sunday,
                        },
                        HourSlots = new int[]
                        {
                            2,
                            3,
                        },
                    },
                },
                NodePools = new KubernetesClusterNodePool[]
                {
                    new KubernetesClusterNodePool()
                    {
                        AvailabilityZones = new int[] { 1, 2, 3, },
                        MaximumNodeCount = 5,
                        MaximumPods = 30,
                        MaximumSurge = "33%",
                        MinimumNodeCount = 3,
                        OsDiskSizeGB = 100,
                        OSDiskType = KubernetesNodeOSDiskType.Ephemeral,
                        ScaleSetEvictionPolicy = KubernetesNodeScaleSetEvictionPolicy.Deallocate,
                        Type = KubernetesClusterNodePoolType.System,
                        VmSize = "Standard_D2ads_v5",
                    },
                },
                SKUTier = KubernetesClusterSKUTier.Paid,
                UpgradeChannel = KubernetesClusterUpgradeChannel.Stable,
            },
        };

        var resources = await Testing.RunAsync<MainStack>().ConfigureAwait(false);

        foreach (var resource in resources)
        {
            var tagsOutput = GetAzureTags(resource);
            if (tagsOutput is not null)
            {
                var tags = await OutputUtilities.GetValueAsync(tagsOutput).ConfigureAwait(false);

                Assert.NotNull(tags);
                Assert.Equal("test-app", tags![TagName.Application]);
                Assert.Equal("test", tags![TagName.Environment]);
                var location = tags![TagName.Location];
                Assert.True(string.Equals("northeurope", location, StringComparison.Ordinal) ||
                    string.Equals("canadacentral", location, StringComparison.Ordinal));
            }
        }
    }

    private static Output<ImmutableList<string>?>? GetAzureActiveDirectoryTags(Resource resource)
    {
        var tagsProperty = resource.GetType().GetProperty("Tags");
        if (tagsProperty?.PropertyType == typeof(Output<ImmutableList<string>?>))
        {
            return (Output<ImmutableList<string>?>?)tagsProperty.GetValue(resource);
        }

        return null;
    }

    private static Output<ImmutableDictionary<string, string>?>? GetAzureTags(Resource resource)
    {
        var tagsProperty = resource.GetType().GetProperty("Tags");
        if (tagsProperty?.PropertyType == typeof(Output<ImmutableDictionary<string, string>?>))
        {
            return (Output<ImmutableDictionary<string, string>?>?)tagsProperty.GetValue(resource);
        }

        return null;
    }
}

namespace Ummati.Infrastructure;

using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Pulumi;
using Pulumi.AzureNative.ContainerService;

#pragma warning disable CA1724 // Conflicts with System.Configuration
public class Configuration : IConfiguration
#pragma warning restore CA1724 // Conflicts with System.Configuration
{
    private readonly Config config = new();

    public string ApplicationName => this.GetString(nameof(this.ApplicationName));

    public string Environment => this.GetString(nameof(this.Environment));

    public string CommonLocation => this.GetString(nameof(this.CommonLocation));

    public IEnumerable<string> Locations => this.GetStringCollection(nameof(this.Locations));

    public IEnumerable<WeekDay> KubernetesMaintenanceDays =>
        this.GetCollection(
            nameof(this.KubernetesMaintenanceDays),
            new Dictionary<string, WeekDay>()
            {
                { nameof(WeekDay.Monday), WeekDay.Monday },
                { nameof(WeekDay.Tuesday), WeekDay.Tuesday },
                { nameof(WeekDay.Wednesday), WeekDay.Wednesday },
                { nameof(WeekDay.Thursday), WeekDay.Thursday },
                { nameof(WeekDay.Friday), WeekDay.Friday },
                { nameof(WeekDay.Saturday), WeekDay.Saturday },
                { nameof(WeekDay.Sunday), WeekDay.Sunday },
            });

    public IEnumerable<int> KubernetesMaintenanceHourSlots =>
        this.GetIntegerCollection(nameof(this.KubernetesMaintenanceHourSlots), minimum: 0, maximum: 24);

    public ManagedClusterSKUTier KubernetesSKUTier =>
        this.Get(
            nameof(this.KubernetesSKUTier),
            new Dictionary<string, ManagedClusterSKUTier>()
            {
                { nameof(ManagedClusterSKUTier.Free), ManagedClusterSKUTier.Free },
                { nameof(ManagedClusterSKUTier.Paid), ManagedClusterSKUTier.Paid },
            });

    public UpgradeChannel KubernetesUpgradeChannel =>
        this.Get(
            nameof(this.KubernetesUpgradeChannel),
            new Dictionary<string, UpgradeChannel>()
            {
                { nameof(UpgradeChannel.Node_image), UpgradeChannel.Node_image },
                { nameof(UpgradeChannel.None), UpgradeChannel.None },
                { nameof(UpgradeChannel.Patch), UpgradeChannel.Patch },
                { nameof(UpgradeChannel.Rapid), UpgradeChannel.Rapid },
                { nameof(UpgradeChannel.Stable), UpgradeChannel.Stable },
            });

    public IEnumerable<string> KubernetesSystemNodesAvailabilityZones =>
        this.GetIntegerCollection(nameof(this.KubernetesSystemNodesAvailabilityZones), minimum: 0, maximum: 3)
        .Select(x => x.ToString(CultureInfo.InvariantCulture));

    public int KubernetesSystemNodesMaximumPods => this.GetInteger(nameof(this.KubernetesSystemNodesMaximumPods), minimum: 1, maximum: 250);

    public int KubernetesSystemNodesMaximumNodeCount => this.GetInteger(nameof(this.KubernetesSystemNodesMaximumNodeCount), minimum: 1, maximum: 100);

    public string KubernetesSystemNodesMaximumSurge => this.GetString(nameof(this.KubernetesSystemNodesMaximumSurge), pattern: @"^(\d+)(\%?)$");

    public int KubernetesSystemNodesMinimumNodeCount => this.GetInteger(nameof(this.KubernetesSystemNodesMinimumNodeCount), minimum: 0, maximum: 99);

    public int KubernetesSystemNodesOsDiskSizeGB => this.GetInteger(nameof(this.KubernetesSystemNodesOsDiskSizeGB), minimum: 1);

    public OSDiskType KubernetesSystemNodesOSDiskType =>
        this.Get(
            nameof(this.KubernetesSystemNodesOSDiskType),
            new Dictionary<string, OSDiskType>()
            {
                { nameof(OSDiskType.Ephemeral), OSDiskType.Ephemeral },
                { nameof(OSDiskType.Managed), OSDiskType.Managed },
            });

    public ScaleSetEvictionPolicy KubernetesSystemNodesScaleSetEvictionPolicy =>
        this.Get(
            nameof(this.KubernetesSystemNodesScaleSetEvictionPolicy),
            new Dictionary<string, ScaleSetEvictionPolicy>()
            {
                { nameof(ScaleSetEvictionPolicy.Delete), ScaleSetEvictionPolicy.Delete },
                { nameof(ScaleSetEvictionPolicy.Deallocate), ScaleSetEvictionPolicy.Deallocate },
            });

    public string KubernetesSystemNodesVmSize => this.GetString(nameof(this.KubernetesSystemNodesVmSize));

    public IEnumerable<string> KubernetesUserNodesAvailabilityZones =>
        this.GetIntegerCollection(nameof(this.KubernetesUserNodesAvailabilityZones), minimum: 0, maximum: 3)
        .Select(x => x.ToString(CultureInfo.InvariantCulture));

    public int KubernetesUserNodesMaximumPods => this.GetInteger(nameof(this.KubernetesUserNodesMaximumPods), minimum: 1, maximum: 250);

    public int KubernetesUserNodesMaximumNodeCount => this.GetInteger(nameof(this.KubernetesUserNodesMaximumNodeCount), minimum: 1, maximum: 100);

    public string KubernetesUserNodesMaximumSurge => this.GetString(nameof(this.KubernetesUserNodesMaximumSurge), pattern: @"^(\d+)(\%?)$");

    public int KubernetesUserNodesMinimumNodeCount => this.GetInteger(nameof(this.KubernetesUserNodesMinimumNodeCount), minimum: 0, maximum: 99);

    public int KubernetesUserNodesOsDiskSizeGB => this.GetInteger(nameof(this.KubernetesUserNodesOsDiskSizeGB), minimum: 1);

    public OSDiskType KubernetesUserNodesOSDiskType =>
        this.Get(
            nameof(this.KubernetesUserNodesOSDiskType),
            new Dictionary<string, OSDiskType>()
            {
                { nameof(OSDiskType.Ephemeral), OSDiskType.Ephemeral },
                { nameof(OSDiskType.Managed), OSDiskType.Managed },
            });

    public ScaleSetEvictionPolicy KubernetesUserNodesScaleSetEvictionPolicy =>
        this.Get(
            nameof(this.KubernetesUserNodesScaleSetEvictionPolicy),
            new Dictionary<string, ScaleSetEvictionPolicy>()
            {
                { nameof(ScaleSetEvictionPolicy.Delete), ScaleSetEvictionPolicy.Delete },
                { nameof(ScaleSetEvictionPolicy.Deallocate), ScaleSetEvictionPolicy.Deallocate },
            });

    public string KubernetesUserNodesVmSize => this.GetString(nameof(this.KubernetesUserNodesVmSize));

    public string ContainerImageName => this.GetString(nameof(this.ContainerImageName));

    public double ContainerCpu => this.GetDouble(nameof(this.ContainerCpu));

    public string ContainerMemory => this.GetString(nameof(this.ContainerMemory));

    public int ContainerMaxReplicas => this.GetInteger(nameof(this.ContainerMaxReplicas));

    public int ContainerMinReplicas => this.GetInteger(nameof(this.ContainerMinReplicas));

    public int ContainerConcurrentRequests => this.GetInteger(nameof(this.ContainerConcurrentRequests));

    private static T GetFromMap<T>(string value, Dictionary<string, T> map)
    {
        if (map.TryGetValue(value, out var result))
        {
            return result;
        }

        throw new InvalidOperationException($"{typeof(T).Name} with value '{value}' not recognised.");
    }

    private static void AssertIsBetween<T>(string key, T value, T? minimum = null, T? maximum = null)
        where T : struct, IComparable<T>
    {
        if (minimum.HasValue && value.CompareTo(minimum.Value) < 0)
        {
            throw new InvalidOperationException($"{key} with value '{value}' must be more than {minimum}");
        }

        if (maximum.HasValue && value.CompareTo(maximum.Value) > 0)
        {
            throw new InvalidOperationException($"{key} with value '{value}' must be less than {maximum}");
        }
    }

    private double GetDouble(string key, double? minimum = null, double? maximum = null)
    {
        var value = double.Parse(this.config.Require(key), CultureInfo.InvariantCulture);
        AssertIsBetween(key, value, minimum, maximum);
        return value;
    }

    private int GetInteger(string key, int? minimum = null, int? maximum = null)
    {
        var value = int.Parse(this.config.Require(key), CultureInfo.InvariantCulture);
        if (minimum.HasValue && value < minimum)
        {
            throw new InvalidOperationException($"{key} must be more than {minimum}");
        }

        if (maximum.HasValue && value > maximum)
        {
            throw new InvalidOperationException($"{key} must be les than {maximum}");
        }

        return value;
    }

    private string GetString(string key, string? pattern = null)
    {
        var value = this.config.Require(key);

        if (!string.IsNullOrWhiteSpace(pattern))
        {
            if (!Regex.IsMatch(value, pattern))
            {
                throw new InvalidOperationException(
                    $"{key} with value '{value}' must match the pattern {pattern}.");
            }
        }

        return value;
    }

    private IEnumerable<string> GetStringCollection(string key) =>
        this.config
            .RequireObject<JsonElement>(key)
            .EnumerateArray()
            .Select(x => x.GetString()!)
            .Where(x => x is not null);

    private IEnumerable<int> GetIntegerCollection(string key, int? minimum = null, int? maximum = null) =>
        this.GetStringCollection(key).Select(x =>
        {
            var value = int.Parse(x, CultureInfo.InvariantCulture);
            AssertIsBetween(key, value, minimum, maximum);
            return value;
        });

    private IEnumerable<T> GetCollection<T>(string key, Dictionary<string, T> map) =>
        this.GetStringCollection(key).Select(x => GetFromMap(x, map));

    private T Get<T>(string key, Dictionary<string, T> map) => GetFromMap(this.config.Require(key), map);
}

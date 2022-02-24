namespace Ummati.Infrastructure;

using System.Globalization;
using System.Text.Json;
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

    public int KubernetesMaximumPods => this.GetInteger(nameof(this.KubernetesMaximumPods), minimum: 1, maximum: 250);

    public string KubernetesMaximumSurge => this.GetString(nameof(this.KubernetesMaximumSurge));

    public int KubernetesNodeCount => this.GetInteger(nameof(this.KubernetesNodeCount), minimum: 0, maximum: 100);

    public int KubernetesOsDiskSizeGB => this.GetInteger(nameof(this.KubernetesOsDiskSizeGB), minimum: 1);

    public ScaleSetEvictionPolicy KubernetesScaleSetEvictionPolicy =>
        this.Get(
            nameof(this.KubernetesScaleSetEvictionPolicy),
            new Dictionary<string, ScaleSetEvictionPolicy>()
            {
                { nameof(ScaleSetEvictionPolicy.Delete), ScaleSetEvictionPolicy.Delete },
                { nameof(ScaleSetEvictionPolicy.Deallocate), ScaleSetEvictionPolicy.Deallocate },
            });

    public string KubernetesVmSize => this.GetString(nameof(this.KubernetesVmSize));

    public string ContainerImageName => this.GetString(nameof(this.ContainerImageName));

    public double ContainerCpu => this.GetDouble(nameof(this.ContainerCpu));

    public string ContainerMemory => this.GetString(nameof(this.ContainerMemory));

    public int ContainerMaxReplicas => this.GetInteger(nameof(this.ContainerMaxReplicas));

    public int ContainerMinReplicas => this.GetInteger(nameof(this.ContainerMinReplicas));

    public int ContainerConcurrentRequests => this.GetInteger(nameof(this.ContainerConcurrentRequests));

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

    private static T GetFromMap<T>(string value, Dictionary<string, T> map)
    {
        if (map.ContainsKey(value))
        {
            return map[value];
        }

        throw new InvalidOperationException($"{typeof(T).Name} with value '{value}' not recognised.");
    }

    private double GetDouble(string key, double? minimum = null, double? maximum = null)
    {
        var value = double.Parse(this.config.Require(key), CultureInfo.InvariantCulture);
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

    private string GetString(string key) => this.config.Require(key);

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
            if (minimum.HasValue && value < minimum)
            {
                throw new InvalidOperationException($"{key} must be more than {minimum}");
            }

            if (maximum.HasValue && value > maximum)
            {
                throw new InvalidOperationException($"{key} must be les than {maximum}");
            }

            return value;
        });

    private IEnumerable<T> GetCollection<T>(string key, Dictionary<string, T> map) =>
        this.GetStringCollection(key).Select(x => GetFromMap(x, map));

    private T Get<T>(string key, Dictionary<string, T> map) => GetFromMap(this.config.Require(key), map);
}

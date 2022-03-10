namespace Ummati.Infrastructure.Configuration;

using Pulumi.AzureNative.ContainerService;

/// <summary>
/// Allowed days and times when scheduled maintenance on a cluster is allowed to occur.
/// </summary>
public class KubernetesClusterMaintenance
{
    /// <summary>
    /// Gets or sets the days that maintenance is allowed on the cluster.
    /// </summary>
    public IEnumerable<DayOfWeek> Days { get; set; } = default!;

    /// <summary>
    /// Gets or sets the hours of the day that maintenance is allowed on the cluster.
    /// </summary>
    public IEnumerable<int> HourSlots { get; set; } = default!;

    internal IEnumerable<WeekDay> InternalDays =>
        this.Days.Select(dayOfWeek => dayOfWeek switch
        {
            DayOfWeek.Monday => WeekDay.Monday,
            DayOfWeek.Tuesday => WeekDay.Tuesday,
            DayOfWeek.Wednesday => WeekDay.Wednesday,
            DayOfWeek.Thursday => WeekDay.Thursday,
            DayOfWeek.Friday => WeekDay.Friday,
            DayOfWeek.Saturday => WeekDay.Saturday,
            DayOfWeek.Sunday => WeekDay.Sunday,
            _ => throw new InvalidOperationException($"{nameof(DayOfWeek)} '{dayOfWeek}' not recognised."),
        });
}

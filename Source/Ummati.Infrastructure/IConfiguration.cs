namespace Ummati.Infrastructure;

using System.Collections.Generic;

public interface IConfiguration
{
    string ApplicationName { get; }

    string CommonLocation { get; }

    IEnumerable<string> Locations { get; }

    string Environment { get; }

    string ContainerImageName { get; }

    double ContainerCpu { get; }

    string ContainerMemory { get; }

    int ContainerMaxReplicas { get; }

    int ContainerMinReplicas { get; }

    int ContainerConcurrentRequests { get; }

    string GetAzureActiveDirectoryDescription();

#pragma warning disable CA1002 // Do not expose generic lists
    List<string> GetAzureActiveDirecoryTags();
#pragma warning restore CA1002 // Do not expose generic lists

    Dictionary<string, string> GetTags(string location);
}

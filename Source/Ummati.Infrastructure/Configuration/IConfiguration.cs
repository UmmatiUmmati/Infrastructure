namespace Ummati.Infrastructure.Configuration;

using System.Collections.Generic;

public interface IConfiguration
{
    /// <summary>
    /// Gets the application name.
    /// </summary>
    string ApplicationName { get; }

    /// <summary>
    /// Gets the environment name e.g. development, production.
    /// </summary>
    string Environment { get; }

    /// <summary>
    /// Gets the location where common resources can be placed.
    /// </summary>
    string CommonLocation { get; }

    /// <summary>
    /// Gets one or more locations where resources can be duplicated.
    /// </summary>
    IEnumerable<string> Locations { get; }

    KubernetesCluster Kubernetes { get; }
}

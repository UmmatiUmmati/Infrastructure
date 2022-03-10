namespace Ummati.Infrastructure.Resources;

using System;
using Pulumi;
using Pulumi.AzureNative.OperationalInsights;
using Pulumi.AzureNative.OperationalInsights.Inputs;
using Pulumi.AzureNative.Resources;
using Ummati.Infrastructure.Configuration;

public class CommonResource : ComponentResource
{
    public CommonResource(
        string name,
        IConfiguration configuration,
        string location,
        ResourceGroup resourceGroup,
        ComponentResourceOptions? options = null)
#pragma warning disable CA1062 // Validate arguments of public methods
        : base($"{configuration.ApplicationName}:{nameof(CommonResource)}", name, options)
#pragma warning restore CA1062 // Validate arguments of public methods
    {
        Validate(name, location, resourceGroup);

        var workspace = new Workspace(
            $"log-analytics-{location}-{configuration.Environment}-",
            new WorkspaceArgs()
            {
                Location = location,
                ResourceGroupName = resourceGroup.Name,
                RetentionInDays = 30,
                Sku = new WorkspaceSkuArgs()
                {
                    Name = WorkspaceSkuNameEnum.PerGB2018,
                },
                Tags = configuration.GetTags(location),
            });

        this.WorkspaceId = workspace.Id;

        this.RegisterOutputs();
    }

    public Output<string> WorkspaceId { get; set; }

    private static void Validate(string name, string location, ResourceGroup resourceGroup)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(location);
        ArgumentNullException.ThrowIfNull(resourceGroup);

        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException($"'{nameof(name)}' cannot be empty.", nameof(name));
        }

        if (string.IsNullOrEmpty(location))
        {
            throw new ArgumentException($"'{nameof(location)}' cannot be empty.", nameof(location));
        }
    }
}

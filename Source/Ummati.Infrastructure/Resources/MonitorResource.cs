namespace Ummati.Infrastructure.Resources;

using Pulumi;
using Pulumi.AzureNative.OperationalInsights;
using Pulumi.AzureNative.OperationalInsights.Inputs;
using Pulumi.AzureNative.Resources;
using Ummati.Infrastructure.Configuration;

public class MonitorResource : ComponentResource<MonitorResource>
{
    public MonitorResource(
        string name,
        IConfiguration configuration,
        string location,
        ResourceGroup resourceGroup,
        ComponentResourceOptions? options = null)
        : base(name, configuration, location, options)
    {
        ArgumentNullException.ThrowIfNull(resourceGroup);

        var workspace = new Workspace(
            $"workspace-{location}-{configuration.Environment}-",
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
}

namespace Ummati.Infrastructure.Stacks;

using System.Collections.Immutable;
using System.Text;
using Pulumi;
using Pulumi.AzureAD;
using Pulumi.AzureNative.ContainerService;
using Pulumi.AzureNative.ContainerService.Inputs;
using Pulumi.AzureNative.Network;
using Pulumi.AzureNative.Network.Inputs;
using Pulumi.AzureNative.OperationalInsights;
using Pulumi.AzureNative.OperationalInsights.Inputs;
using Pulumi.AzureNative.Resources;

#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public class AzureKubernetesStack : Stack
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
    public AzureKubernetesStack()
    {
        if (Configuration is null)
        {
            Configuration = new Configuration();
        }

        var commonResourceGroup = GetResourceGroup("common", Configuration.CommonLocation);
        var workspace = GetWorkspace(Configuration.CommonLocation, commonResourceGroup);

        var outputs = new List<Output<string>>();
        foreach (var location in Configuration.Locations)
        {
            var applicationName = $"{Configuration.ApplicationName}-application-{location}-{Configuration.Environment}";
            var application = new Application(
                applicationName,
                new ApplicationArgs()
                {
                    DisplayName = applicationName,
                    SupportUrl = "https://github.com/UmmatiUmmati/Infrastructure",
                    Tags = GetAzureActiveDirecoryTags(),
                });
            var servicePrincipal = new ServicePrincipal(
                $"{Configuration.ApplicationName}-service-principal-{location}-{Configuration.Environment}",
                new ServicePrincipalArgs()
                {
                    ApplicationId = application.ApplicationId,
                    Description = GetAzureActiveDirectoryDescription(),
                    Notes = GetAzureActiveDirectoryDescription(),
                    Tags = GetAzureActiveDirecoryTags(),
                });
            var servicePrincipalPassword = new ServicePrincipalPassword(
                $"{Configuration.ApplicationName}-service-principal-password-{location}-{Configuration.Environment}",
                new ServicePrincipalPasswordArgs()
                {
                    // This cannot be changed after deployment.
                    EndDate = new DateTime(2999, 1, 1).ToRFC3339String(),
                    ServicePrincipalId = servicePrincipal.Id,
                });

            var resourceGroup = GetResourceGroup("kubernetes", location);
            var virtualNetwork = new VirtualNetwork(
                $"virtualnetwork-{location}-{Configuration.Environment}-",
                new VirtualNetworkArgs()
                {
                    AddressSpace = new AddressSpaceArgs()
                    {
                        AddressPrefixes =
                        {
                            "10.0.0.0/16",
                        },
                    },
                    Location = location,
                    ResourceGroupName = resourceGroup.Name,
                    Tags = GetTags(location),
                });
            var subnet = new Subnet(
                $"subnet-{location}-{Configuration.Environment}",
                new Pulumi.AzureNative.Network.SubnetArgs()
                {
                    AddressPrefix = $"10.0.0.0/23",
                    ResourceGroupName = resourceGroup.Name,
                    VirtualNetworkName = virtualNetwork.Name,
                });
            var networkWatcher = new NetworkWatcher(
                $"networkwatcher-{location}-{Configuration.Environment}-",
                new NetworkWatcherArgs()
                {
                    Location = location,
                    ResourceGroupName = resourceGroup.Name,
                    Tags = GetTags(location),
                });

            var kubernetesCluster = new ManagedCluster(
                $"kubernetes-{location}-{Configuration.Environment}-",
                new ManagedClusterArgs()
                {
                    ResourceGroupName = resourceGroup.Name,
                    AgentPoolProfiles = new InputList<ManagedClusterAgentPoolProfileArgs>()
                    {
                        new ManagedClusterAgentPoolProfileArgs()
                        {
                            Count = 1, // Maximum 100
                            MaxPods = 250, // Maximum 250, default 30
                            Mode = AgentPoolMode.System,
                            Name = $"default",
                            OsDiskSizeGB = 30,
                            OsType = "Linux",
                            Tags = GetTags(location),
                            Type = AgentPoolType.VirtualMachineScaleSets,
                            VmSize = "Standard_B2s",
                            VnetSubnetID = subnet.Id,
                        },
                    },
                    DnsPrefix = "AzureNativeprovider",
                    EnableRBAC = true,
                    Tags = GetTags(location),

                    // KubernetesVersion = "1.22.4", // You can only upgrade one minor version at a time.
                    NodeResourceGroup = $"{Configuration.ApplicationName}-kubernetesnodes-{location}-{Configuration.Environment}",
                    NetworkProfile = new ContainerServiceNetworkProfileArgs()
                    {
                        NetworkPlugin = NetworkPlugin.Azure,
                        DnsServiceIP = "10.0.2.254",
                        ServiceCidr = "10.0.2.0/24",
                        DockerBridgeCidr = "172.17.0.1/16",
                    },
                    ServicePrincipalProfile = new ManagedClusterServicePrincipalProfileArgs
                    {
                        ClientId = application.ApplicationId,
                        Secret = servicePrincipalPassword.Value,
                    },

                    // AddonProfiles = new InputMap<ManagedClusterAddonProfileArgs>()
                    // {
                    //     {
                    //         "omsAgent",
                    //         new ManagedClusterAddonProfileArgs()
                    //         {
                    //             Enabled = true,
                    //             Config = new InputMap<string>()
                    //             {
                    //                 { "logAnalyticsWorkspaceId", workspace.Id },
                    //             },
                    //         },
                    //     },
                    // },
                });

            outputs.Add(GetKubeConfig(resourceGroup.Name, kubernetesCluster.Name));
        }

        this.KubeConfigs = Output.All(outputs.Select(x => x));
    }

    public static IConfiguration Configuration { get; set; } = default!;

    [Output]
    public Output<ImmutableArray<string>> KubeConfigs { get; private set; }

    private static Output<string> GetKubeConfig(Output<string> resourceGroupName, Output<string> clusterName)
    {
        var output = ListManagedClusterUserCredentials.Invoke(
            new ListManagedClusterUserCredentialsInvokeArgs()
            {
                ResourceGroupName = resourceGroupName,
                ResourceName = clusterName,
            });
        return output
            .Apply(credentials =>
            {
                try
                {
                    var base64EncodedKubeConfig = credentials.Kubeconfigs.First().Value;
                    var kubeConfig = Convert.FromBase64String(base64EncodedKubeConfig);
                    return Encoding.UTF8.GetString(kubeConfig);
                }
                catch (NullReferenceException)
                {
                    // Returned in tests.
                    return string.Empty;
                }
            })
            .Apply(Output.CreateSecret);
    }

    private static string GetAzureActiveDirectoryDescription() =>
        string.Join(Environment.NewLine, GetAzureActiveDirecoryTags());

    private static List<string> GetAzureActiveDirecoryTags() =>
        GetTags("Azure Active Directory").Select(x => $"{x.Key}={x.Value}").ToList();

    private static Dictionary<string, string> GetTags(string location) =>
        new()
        {
            { TagName.Application, Configuration.ApplicationName },
            { TagName.Environment, Configuration.Environment },
            { TagName.Location, location },
        };

    private static ResourceGroup GetResourceGroup(string name, string location) =>
        new(
            $"{Configuration.ApplicationName}-{name}-{location}-{Configuration.Environment}-",
            new ResourceGroupArgs()
            {
                Location = location,
                Tags = GetTags(location),
            });

    private static Workspace GetWorkspace(string location, ResourceGroup resourceGroup) =>
        new(
            $"log-analytics-{location}-{Configuration.Environment}-",
            new WorkspaceArgs()
            {
                Location = location,
                ResourceGroupName = resourceGroup.Name,
                RetentionInDays = 30,
                Sku = new WorkspaceSkuArgs()
                {
                    Name = WorkspaceSkuNameEnum.PerGB2018,
                },
                Tags = GetTags(location),
            });
}

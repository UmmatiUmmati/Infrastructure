namespace Ummati.Infrastructure.Resources;

using System.Text;
using Pulumi;
using Pulumi.AzureNative.ContainerService;
using Pulumi.AzureNative.ContainerService.Inputs;
using Pulumi.AzureNative.Resources;

public class KubernetesResource : ComponentResource
{
    public KubernetesResource(
        string name,
        IConfiguration configuration,
        string location,
        ResourceGroup resourceGroup,
        IdentityResource identityResource,
        VirtualNetworkResource virtualNetworkResource,
        ComponentResourceOptions? options = null)
#pragma warning disable CA1062 // Validate arguments of public methods
        : base($"{configuration.ApplicationName}:{nameof(KubernetesResource)}", name, options)
#pragma warning restore CA1062 // Validate arguments of public methods
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(location);
        ArgumentNullException.ThrowIfNull(resourceGroup);
        ArgumentNullException.ThrowIfNull(identityResource);
        ArgumentNullException.ThrowIfNull(virtualNetworkResource);

        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException($"'{nameof(name)}' cannot be empty.", nameof(name));
        }

        if (string.IsNullOrEmpty(location))
        {
            throw new ArgumentException($"'{nameof(location)}' cannot be empty.", nameof(location));
        }

        var kubernetesCluster = new ManagedCluster(
            $"kubernetes-{location}-{configuration.Environment}-",
            new ManagedClusterArgs()
            {
                ResourceGroupName = resourceGroup.Name,
                AgentPoolProfiles = new InputList<ManagedClusterAgentPoolProfileArgs>()
                {
                    new ManagedClusterAgentPoolProfileArgs()
                    {
                        // Recommended minimum of 3.
                        Count = 1, // Maximum 100
                        MaxPods = 250, // Maximum 250, default 30
                        Mode = AgentPoolMode.System,
                        Name = $"default",
                        OsDiskSizeGB = 30,
                        OsType = "Linux",
                        Tags = configuration.GetTags(location),
                        Type = AgentPoolType.VirtualMachineScaleSets,

                        // DS3_v2 is the minimum recommended and DS4_v2 is recommended.
                        VmSize = "Standard_B2s",
                        VnetSubnetID = virtualNetworkResource.SubnetId,
                    },
                },
                DnsPrefix = "AzureNativeprovider",
                EnableRBAC = true,
                Tags = configuration.GetTags(location),

                // KubernetesVersion = "1.22.4", // You can only upgrade one minor version at a time.
                NodeResourceGroup = $"{configuration.ApplicationName}-kubernetesnodes-{location}-{configuration.Environment}",
                NetworkProfile = new ContainerServiceNetworkProfileArgs()
                {
                    NetworkPlugin = NetworkPlugin.Azure,
                    DnsServiceIP = "10.0.2.254",
                    ServiceCidr = "10.0.2.0/24",
                    DockerBridgeCidr = "172.17.0.1/16",
                },
                ServicePrincipalProfile = new ManagedClusterServicePrincipalProfileArgs
                {
                    ClientId = identityResource.ClientId,
                    Secret = identityResource.ClientSecret,
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

        this.KubeConfig = GetKubeConfig(resourceGroup.Name, kubernetesCluster.Name);

        this.RegisterOutputs();
    }

    public Output<string> KubeConfig { get; set; }

    private static Output<string> GetKubeConfig(
        Output<string> resourceGroupName,
        Output<string> clusterName)
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
}

namespace Ummati.Infrastructure.Stacks;

using System.Collections.Immutable;
using FluentValidation;
using Pulumi;
using Pulumi.AzureNative.Resources;
using Ummati.Infrastructure.Configuration;
using Ummati.Infrastructure.Resources;
using Ummati.Infrastructure.Validators;

#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public class AzureKubernetesStack : Stack
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
    public AzureKubernetesStack()
    {
        if (Configuration is null)
        {
            Configuration = new Configuration();
            var configurationValidator = new ConfigurationValidator();
            var validationResult = configurationValidator.Validate(Configuration);
            if (!validationResult.IsValid)
            {
                Log.Error($"Validation of configuration failed.{Environment.NewLine}{validationResult}");
                configurationValidator.ValidateAndThrow(Configuration);
            }
        }

        var commonResourceGroup = GetResourceGroup("common", Configuration.CommonLocation);
        var commonResource = new CommonResource(
            $"common-{Configuration.CommonLocation}-{Configuration.Environment}-",
            Configuration,
            Configuration.CommonLocation,
            commonResourceGroup);

        var kubernetesResources = new List<KubernetesResource>();
        foreach (var location in Configuration.Locations)
        {
            var identityResource = new IdentityResource(
                $"identity-{location}-{Configuration.Environment}-",
                Configuration,
                location);

            var resourceGroup = GetResourceGroup("kubernetes", location);

            var virtualNetworkResource = new VirtualNetworkResource(
                $"virtualnetwork-{location}-{Configuration.Environment}-",
                Configuration,
                location,
                resourceGroup);

            kubernetesResources.Add(
            new KubernetesResource(
                $"kubernetes-{location}-{Configuration.Environment}-",
                Configuration,
                location,
                resourceGroup,
                commonResource,
                identityResource,
                virtualNetworkResource));
        }

        this.KubeConfigs = Output.All(kubernetesResources.Select(x => x.KubeConfig));
        this.KubeFqdns = Output.All(kubernetesResources.Select(x => x.KubeFqdn));
    }

    public static IConfiguration Configuration { get; set; } = default!;

    [Output]
    public Output<ImmutableArray<string>> KubeConfigs { get; private set; }

    [Output]
    public Output<ImmutableArray<string>> KubeFqdns { get; private set; }

    private static ResourceGroup GetResourceGroup(string name, string location) =>
        new(
            $"{Configuration.ApplicationName}-{name}-{location}-{Configuration.Environment}-",
            new ResourceGroupArgs()
            {
                Location = location,
                Tags = Configuration.GetTags(location),
            });
}

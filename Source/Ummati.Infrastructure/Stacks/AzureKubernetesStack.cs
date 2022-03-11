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

        var outputs = new List<Output<string>>();
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

            var kubernetesResource = new KubernetesResource(
                $"kubernetes-{location}-{Configuration.Environment}-",
                Configuration,
                location,
                resourceGroup,
                commonResource,
                identityResource,
                virtualNetworkResource);

            outputs.Add(kubernetesResource.KubeConfig);
        }

        this.KubeConfigs = Output.All(outputs.Select(x => x));
    }

    public static IConfiguration Configuration { get; set; } = default!;

    [Output]
    public Output<ImmutableArray<string>> KubeConfigs { get; private set; }

    private static ResourceGroup GetResourceGroup(string name, string location) =>
        new(
            $"{Configuration.ApplicationName}-{name}-{location}-{Configuration.Environment}-",
            new ResourceGroupArgs()
            {
                Location = location,
                Tags = Configuration.GetTags(location),
            });
}

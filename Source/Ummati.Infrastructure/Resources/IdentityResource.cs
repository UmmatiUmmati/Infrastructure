namespace Ummati.Infrastructure.Resources;

using Pulumi;
using Pulumi.AzureAD;

public class IdentityResource : ComponentResource
{
    public IdentityResource(
        string name,
        IConfiguration configuration,
        string location,
        ComponentResourceOptions? options = null)
#pragma warning disable CA1062 // Validate arguments of public methods
        : base($"{configuration.ApplicationName}:{nameof(IdentityResource)}", name, options)
#pragma warning restore CA1062 // Validate arguments of public methods
    {
        Validate(name, location);

        var azureActiveDirectoryTags = configuration.GetAzureActiveDirecoryTags();
        var azureActiveDirectoryDescription = configuration.GetAzureActiveDirectoryDescription();

        var applicationName = $"{configuration.ApplicationName}-application-{location}-{configuration.Environment}";
        var application = new Application(
            applicationName,
            new ApplicationArgs()
            {
                DisplayName = applicationName,
                SupportUrl = "https://github.com/UmmatiUmmati/Infrastructure",
                Tags = azureActiveDirectoryTags,
            });
        var servicePrincipal = new ServicePrincipal(
            $"{configuration.ApplicationName}-service-principal-{location}-{configuration.Environment}",
            new ServicePrincipalArgs()
            {
                ApplicationId = application.ApplicationId,
                Description = azureActiveDirectoryDescription,
                Notes = azureActiveDirectoryDescription,
                Tags = azureActiveDirectoryTags,
            });
        var servicePrincipalPassword = new ServicePrincipalPassword(
            $"{configuration.ApplicationName}-service-principal-password-{location}-{configuration.Environment}",
            new ServicePrincipalPasswordArgs()
            {
                // This cannot be changed after deployment.
                EndDate = new DateTime(2999, 1, 1).ToRFC3339String(),
                ServicePrincipalId = servicePrincipal.Id,
            });

        this.ClientId = application.ApplicationId;
        this.ClientSecret = servicePrincipalPassword.Value;

        this.RegisterOutputs();
    }

    public Output<string> ClientId { get; set; }

    public Output<string> ClientSecret { get; set; }

    private static void Validate(string name, string location)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(location);

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

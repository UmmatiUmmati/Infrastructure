namespace Ummati.Infrastructure.Resources;

using Pulumi;
using Pulumi.AzureAD;
using Ummati.Infrastructure.Configuration;

public class IdentityResource : ComponentResource<IdentityResource>
{
    public IdentityResource(
        string name,
        IConfiguration configuration,
        string location,
        ComponentResourceOptions? options = null)
        : base(name, configuration, location, options)
    {
        var azureActiveDirectoryTags = configuration.GetAzureActiveDirecoryTags();
        var azureActiveDirectoryDescription = configuration.GetAzureActiveDirectoryDescription();

        var applicationName = $"{configuration.ApplicationName}-{name}-application-{location}-{configuration.Environment}";
        var application = new Application(
            applicationName,
            new ApplicationArgs()
            {
                DisplayName = applicationName,
                SupportUrl = "https://github.com/UmmatiUmmati/Infrastructure",
                Tags = azureActiveDirectoryTags,
            });
        var servicePrincipal = new ServicePrincipal(
            $"{configuration.ApplicationName}-{name}-serviceprincipal-{location}-{configuration.Environment}",
            new ServicePrincipalArgs()
            {
                ApplicationId = application.ApplicationId,
                Description = azureActiveDirectoryDescription,
                Notes = azureActiveDirectoryDescription,
                Tags = azureActiveDirectoryTags,
            });
        var servicePrincipalPassword = new ServicePrincipalPassword(
            $"{configuration.ApplicationName}-{name}-serviceprincipalpassword-{location}-{configuration.Environment}",
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
}

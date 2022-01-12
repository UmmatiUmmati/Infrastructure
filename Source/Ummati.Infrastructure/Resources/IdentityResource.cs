namespace Ummati.Infrastructure.Resources;

using Pulumi;

public class IdentityResource : ComponentResource
{
    public IdentityResource(string name, ComponentResourceOptions? options = null)
        : base($"{Configuration.ApplicationName}:{nameof(IdentityResource)}", name, options)
    {
    }

    public static IConfiguration Configuration { get; set; } = default!;
}

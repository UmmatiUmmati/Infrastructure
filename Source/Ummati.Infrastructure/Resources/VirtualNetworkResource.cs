namespace Ummati.Infrastructure.Resources;

using Pulumi;

public class VirtualNetworkResource : ComponentResource
{
    public VirtualNetworkResource(string name, ComponentResourceOptions? options = null)
        : base($"{Configuration.ApplicationName}:{nameof(VirtualNetworkResource)}", name, options)
    {
    }

    public static IConfiguration Configuration { get; set; } = default!;
}
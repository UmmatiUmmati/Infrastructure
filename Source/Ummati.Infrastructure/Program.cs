namespace Ummati.Infrastructure;

using Pulumi;
using Ummati.Infrastructure.Stacks;

public static class Program
{
    public static Task<int> Main() => Deployment.RunAsync<AzureContainerAppStack>();
}

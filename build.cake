#addin nuget:?package=SimpleExec&version=10.0.0

using System.Text.Json;
using SimpleExec;

var target = Argument("Target", "Default");
var configuration =
    HasArgument("Configuration") ? Argument<string>("Configuration") :
    EnvironmentVariable("Configuration", "Release");
var stack =
    HasArgument("Stack") ? Argument<string>("Stack") :
    EnvironmentVariable("Stack", "development");
var servicePrincipalName =
    HasArgument("ServicePrincipalName") ? Argument<string>("ServicePrincipalName") :
    EnvironmentVariable("ServicePrincipalName");

var artefactsDirectory = Directory("./Artefacts");

Task("Clean")
    .Description("Cleans the artefacts, bin and obj directories.")
    .Does(() =>
    {
        CleanDirectory(artefactsDirectory);
        DeleteDirectories(GetDirectories("**/bin"), new DeleteDirectorySettings() { Force = true, Recursive = true });
        DeleteDirectories(GetDirectories("**/obj"), new DeleteDirectorySettings() { Force = true, Recursive = true });
    });

Task("Restore")
    .Description("Restores NuGet packages.")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        DotNetRestore();
    });

Task("Build")
    .Description("Builds the solution.")
    .IsDependentOn("Restore")
    .Does(() =>
    {
        DotNetBuild(
            ".",
            new DotNetBuildSettings()
            {
                Configuration = configuration,
                NoRestore = true,
            });
    });

Task("Test")
    .Description("Runs unit tests and outputs test results to the artefacts directory.")
    .DoesForEach(GetFiles("./Tests/**/*.csproj"), project =>
    {
        DotNetTest(
            project.ToString(),
            new DotNetTestSettings()
            {
                Blame = true,
                Collectors = new string[] { "Code Coverage", "XPlat Code Coverage" },
                Configuration = configuration,
                Loggers = new string[]
                {
                    $"trx;LogFileName={project.GetFilenameWithoutExtension()}.trx",
                    $"html;LogFileName={project.GetFilenameWithoutExtension()}.html",
                },
                NoBuild = true,
                NoRestore = true,
                ResultsDirectory = artefactsDirectory,
            });
    });

Task("UpdateServicePrincipal")
    .Description("")
    .Does(async () =>
    {
        var subscriptionId = await GetSubscriptionIdAsync();
        await DeleteServicePrincipalAsync(servicePrincipalName);
        var servicePrincipal = await CreateServicePrincipalAsync(servicePrincipalName, subscriptionId);
        await AddAzureActiveDirectoryPermissionsAsync(servicePrincipal.ObjectId);

        Information($"ObjectId: {servicePrincipal.ObjectId}");
        Information($"ClientId: {servicePrincipal.ClientId}");
        Information($"ClientSecret: {servicePrincipal.ClientSecret}");
        Information($"TenantId: {servicePrincipal.TenantId}");
        Information($"SubscriptionId: {subscriptionId}");

        await SetPulumiConfigAsync("azuread:clientId", servicePrincipal.ClientId, secret: true);
        await SetPulumiConfigAsync("azuread:clientSecret", servicePrincipal.ClientSecret, secret: true);
        await SetPulumiConfigAsync("azuread:tenantId", servicePrincipal.TenantId, secret: true);
           
        await SetPulumiConfigAsync("azure-native:clientId", servicePrincipal.ClientId, secret: true);
        await SetPulumiConfigAsync("azure-native:clientSecret", servicePrincipal.ClientSecret, secret: true);
        await SetPulumiConfigAsync("azure-native:tenantId", servicePrincipal.TenantId, secret: true);
        await SetPulumiConfigAsync("azure-native:subscriptionId", subscriptionId, secret: true);
    });

Task("UpdateAzureLocations")
    .Description("")
    .Does(async () =>
    {
        var json = await ReadAsync("az", "account list-locations");
        System.IO.File.WriteAllText(GetFiles("**/AzureLocations.json").First().ToString(), json);
    });

Task("Default")
    .Description("Cleans, restores NuGet packages, builds the solution and then runs unit tests.")
    .IsDependentOn("Build")
    .IsDependentOn("Test");

RunTarget(target);

Task<string> GetSubscriptionIdAsync() => ReadAsync("az", "account show --query id --output tsv");

async Task<JsonElement.ArrayEnumerator> GetServicePrincipalsAsync(string name)
{
    var json = await ReadAsync("az", $"ad sp list --display-name \"{name}\"");
    var document = JsonDocument.Parse(json).RootElement;
    return document.EnumerateArray();
}

async Task DeleteServicePrincipalAsync(string name)
{
    foreach (var item in await GetServicePrincipalsAsync(name))
    {
        var clientId = item.GetProperty("appId").GetString();
        await ReadAsync("az", $"ad sp delete --id \"{clientId}\"");
    }
}

async Task<ServicePrincipal> CreateServicePrincipalAsync(string name, string subscriptionId)
{
    var json = await ReadAsync("az", $"ad sp create-for-rbac --name \"{name}\" --role Contributor");
    var document = JsonDocument.Parse(json).RootElement;
    var clientId = document.GetProperty("appId").GetString();
    var clientSecret = document.GetProperty("password").GetString();
    var tenantId = document.GetProperty("tenant").GetString();

    var servicePrincipal = (await GetServicePrincipalsAsync(name)).First();
    var objectId = servicePrincipal.GetProperty("appId").GetString();

    return new ServicePrincipal(objectId, clientId, clientSecret, tenantId);
}

async Task AddAzureActiveDirectoryPermissionsAsync(string objectId)
{
    var azureActiveDirectoryGraphApi = "00000002-0000-0000-c000-000000000000";
    var permission = "1cda74f2-2616-4834-b122-5cb1b07f8a59=Role"; // Application.ReadWrite.All
    // var permission = "824c81eb-e3f8-4ee6-8f6d-de7f50d565b7=Role"; // Application.ReadWrite.OwnedBy
    await ReadAsync("az", $"ad app permission add --id \"{objectId}\" --api \"{azureActiveDirectoryGraphApi}\" --api-permissions \"{permission}\"");
    // await ReadAsync("az", $"ad app permission grant --id \"{objectId}\" --api \"{azureActiveDirectoryGraphApi}\"");
    await RetryReadAsync("az", $"ad app permission admin-consent --id \"{objectId}\"");
}

async Task SetPulumiConfigAsync(string key, string value, bool secret = false)
{
    var workingDirectory = GetFiles("**/Pulumi.yaml").Single().GetDirectory().ToString();
    var secretSwitch = secret ? " --secret" : "";
    await ReadAsync(
        "pulumi",
        $"config set \"{key}\" \"{value}\" --stack \"{stack}\"{secretSwitch}",
        workingDirectory);
}

async Task<string> ReadAsync(string name, string arguments, string workingDirectory = null)
{
    Console.ForegroundColor = ConsoleColor.Blue;
    Information($"{name} {arguments}");
    Console.ForegroundColor = ConsoleColor.Gray;
    var (standardOutput, _) = await Command.ReadAsync(name, arguments, workingDirectory);
    if (!string.IsNullOrEmpty(standardOutput))
    {
        Information(standardOutput);
    }
    return standardOutput;
}

async Task<string> RetryReadAsync(string name, string arguments, string workingDirectory = null)
{
    while (true)
    {
        try
        {
            return await ReadAsync(name, arguments, workingDirectory);
            break;
        }
        catch (ExitCodeReadException exitCodeReadException)
        {
            Error(exitCodeReadException.StandardError);
            await System.Threading.Tasks.Task.Delay(1000);
        }
    }
}

public record class ServicePrincipal(string ObjectId, string ClientId, string ClientSecret, string TenantId);

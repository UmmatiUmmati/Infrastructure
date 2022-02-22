using System.Text.Json;

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
    .Does(() =>
    {
        var subscriptionId = GetSubscriptionId();
        DeleteServicePrincipal(servicePrincipalName);
        var (objectId, clientId, clientSecret, tenantId) = CreateServicePrincipal(servicePrincipalName, subscriptionId);
        CreateRoleAssignment(subscriptionId, objectId, "Managed Application Contributor Role");

        Information($"ObjectId: {objectId}");
        Information($"ClientId: {clientId}");
        Information($"ClientSecret: {clientSecret}");
        Information($"TenantId: {tenantId}");
        Information($"SubscriptionId: {subscriptionId}");

        SetPulumiConfig("azuread:clientId", clientId, secret: true);
        SetPulumiConfig("azuread:clientSecret", clientSecret, secret: true);
        SetPulumiConfig("azuread:tenantId", tenantId, secret: true);

        SetPulumiConfig("azure-native:clientId", clientId, secret: true);
        SetPulumiConfig("azure-native:clientSecret", clientSecret, secret: true);
        SetPulumiConfig("azure-native:tenantId", tenantId, secret: true);
        SetPulumiConfig("azure-native:subscriptionId", subscriptionId, secret: true);
    });

Task("Default")
    .Description("Cleans, restores NuGet packages, builds the solution and then runs unit tests.")
    .IsDependentOn("Build")
    .IsDependentOn("Test");

RunTarget(target);

string GetSubscriptionId()
{
    StartProcess(
        "powershell",
        new ProcessSettings()
            .WithArguments(x => x
                .Append("az")
                .Append("account")
                .Append("show")
                .AppendSwitch("--query", "id")
                .AppendSwitch("--output", "tsv"))
            .SetRedirectStandardOutput(true),
            out var lines);
    return lines.First();
}

JsonElement.ArrayEnumerator GetServicePrincipals(string name)
{
    StartProcess(
        "powershell",
        new ProcessSettings()
            .WithArguments(x => x
                .Append("az")
                .Append("ad")
                .Append("sp")
                .Append("list")
                .AppendSwitchQuoted("--display-name", name))
            .SetRedirectStandardOutput(true),
            out var lines);
    var document = JsonDocument.Parse(string.Join(string.Empty, lines)).RootElement;
    return document.EnumerateArray();
}

void DeleteServicePrincipal(string name)
{
    foreach (var item in GetServicePrincipals(name))
    {
        var clientId = item.GetProperty("appId").GetString();
        StartProcess(
            "powershell",
            new ProcessSettings()
                .WithArguments(x => x
                    .Append("az")
                    .Append("ad")
                    .Append("sp")
                    .Append("delete")
                    .AppendSwitchQuoted("--id", clientId)));
    }
}

(string objectId, string clientId, string clientSecret, string tenantId) CreateServicePrincipal(string name, string subscriptionId)
{
    StartProcess(
        "powershell",
        new ProcessSettings()
            .WithArguments(x => x
                .Append("az")
                .Append("ad")
                .Append("sp")
                .Append("create-for-rbac")
                .AppendSwitchQuoted("--name", name)
                .AppendSwitchQuoted("--role", "Contributor"))
            .SetRedirectStandardOutput(true),
            out var lines);

    var document = JsonDocument.Parse(string.Join(string.Empty, lines)).RootElement;
    var clientId = document.GetProperty("appId").GetString();
    var clientSecret = document.GetProperty("password").GetString();
    var tenantId = document.GetProperty("tenant").GetString();

    var servicePrincipal = GetServicePrincipals(name).First();
    var objectId = servicePrincipal.GetProperty("appId").GetString();

    return (objectId, clientId, clientSecret, tenantId);
}

void CreateRoleAssignment(string subscriptionId, string objectId, string role)
{
    StartProcess(
        "powershell",
        new ProcessSettings()
            .WithArguments(x => x
                .Append("az")
                .Append("role")
                .Append("assignment")
                .Append("create")
                .AppendSwitchQuoted("--assignee", objectId)
                .AppendSwitch("--role", $"'{role}'")
                .AppendSwitchQuoted("--subscription", subscriptionId)));

}

void SetPulumiConfig(string key, string value, bool secret = false)
{
    StartProcess(
        "pulumi",
        new ProcessSettings()
            .UseWorkingDirectory(GetFiles("**/Pulumi.yaml").Single().GetDirectory())
            .WithArguments(builder =>
            {
                builder
                    .Append("config")
                    .Append("set")
                    .Append(key)
                    .AppendQuoted(value)
                    .AppendSwitchQuoted("--stack", stack);
                if (secret)
                {
                    builder.Append("--secret");
                }
            }));
}

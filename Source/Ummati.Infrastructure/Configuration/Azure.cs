namespace Ummati.Infrastructure.Configuration;

using System.Collections.Generic;
using System.Text.Json;
using Ummati.Infrastructure.Assets;

internal static class Azure
{
    public static IEnumerable<string> Locations =>
        JsonSerializer.Deserialize<AzureLocation[]>(Resources.AzureLocations)!.Select(x => x.Name);

    // Ideally these should be returned by the Azure CLI, so they can appear in AzureLocations.json.
    // See https://github.com/Azure/azure-cli/issues/21579
    public static IEnumerable<string> LocationsSupportingAvailabilityZones =>
        new string[]
        {
            "brazilsouth",
            "francecentral",
            "southafrica",
            "northaustraliaeast",
            "canadacentral",
            "germanywestcentral",
            "centralindia",
            "centralus",
            "northeurope",
            "japaneast",
            "eastus",
            "norwayeast",
            "koreacentral",
            "eastus2",
            "uksouth",
            "southeastasia",
            "southcentralus",
            "westeurope",
            "eastasia",
            "usgovvirginia",
            "swedencentral",
            "chinanorth3",
            "westus2",
            "westus3",
        };

    private record struct AzureLocation(string Name);
}

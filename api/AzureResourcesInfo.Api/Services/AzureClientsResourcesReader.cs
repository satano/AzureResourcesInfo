using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.CosmosDB;
using Azure.ResourceManager.CosmosDB.Models;
using Azure.ResourceManager.Resources;
using AzureResourcesInfo.Api.Dto;

namespace AzureResourcesInfo.Api.Services
{
    internal class AzureClientsResourcesReader : IAzureResourcesReader
    {
        private readonly ILogger<AzureClientsResourcesReader> _logger;
        private readonly ArmClient _client;

        public AzureClientsResourcesReader(
            IConfiguration config,
            ILogger<AzureClientsResourcesReader> logger)
        {
            _logger = logger;

            string tenantId = config["Azure:TenantId"] ?? string.Empty;
            ArgumentException.ThrowIfNullOrEmpty(tenantId, "Azure:TenantId");
            string clientId = config["Azure:ClientId"] ?? string.Empty;
            ArgumentException.ThrowIfNullOrEmpty(clientId, "Azure:ClientId");
            string clientSecret = config["Azure:ClientSecret"] ?? string.Empty;
            ArgumentException.ThrowIfNullOrEmpty(clientSecret, "Azure:ClientSecret");
            string subscriptionId = config["Azure:SubscriptionId"] ?? string.Empty;
            ArgumentException.ThrowIfNullOrEmpty(subscriptionId, "Azure:SubscriptionId");

            ClientSecretCredential credential = new(tenantId, clientId, clientSecret);
            _client = new ArmClient(credential, subscriptionId);
        }

        private static readonly Uri PortalBaseUri = new("https://portal.azure.com/#resource/subscriptions");

        public static (Uri resourceUrl, Uri resourceGroupUrl) GetUrls(ResourceIdentifier id)
        {
            const string resourceUrlTemplate = "/{0}/resourceGroups/{1}/providers/{2}/{3}";
            const string resourceGroupUrlTemplate = "/{0}/resourceGroups/{1}";
            return (
                new(PortalBaseUri, string.Format(resourceUrlTemplate, id.SubscriptionId, id.ResourceGroupName, id.ResourceType, id.Name)),
                new(PortalBaseUri, string.Format(resourceGroupUrlTemplate, id.SubscriptionId, id.ResourceGroupName))
            );
        }

        async Task<IEnumerable<SubscriptionDto>> IAzureResourcesReader.GetSubscriptions(CancellationToken cancellationToken)
        {
            List<SubscriptionDto> result = [];
            AsyncPageable<SubscriptionResource> subscriptions = _client.GetSubscriptions().GetAllAsync(cancellationToken);
            await foreach (SubscriptionResource subscription in subscriptions)
            {
                SubscriptionData data = subscription.Data;
                result.Add(new SubscriptionDto
                {
                    SubscriptionId = data.SubscriptionId,
                    TenantId = data.TenantId ?? Guid.Empty,
                    Name = data.DisplayName,
                    State = data.State?.ToString() ?? string.Empty,
                });
            }
            return result;
        }

        async Task<IEnumerable<CosmosDbDto>> IAzureResourcesReader.GetCosmosDbAccounts(CancellationToken cancellationToken)
        {
            List<CosmosDbDto> result = [];
            AsyncPageable<CosmosDBAccountResource> cosmosDbs = _client.GetDefaultSubscription().GetCosmosDBAccountsAsync(cancellationToken);
            await foreach (CosmosDBAccountResource cosmosDb in cosmosDbs)
            {
                CosmosDBAccountData data = cosmosDb.Data;
                (Uri resourceUrl, Uri resourceGroupUrl) urls = GetUrls(cosmosDb.Id);
                CosmosDbDto dto = new()
                {
                    Id = cosmosDb.Id.ToString(),
                    ResourceUrl = urls.resourceUrl,
                    ResourceGroup = cosmosDb.Id.ResourceGroupName ?? string.Empty,
                    ResourceGroupUrl = urls.resourceGroupUrl,
                    Name = data.Name,
                    Location = data.Location.DisplayName ?? data.Location.Name,
                    DefaultConsistencyLevel = data.ConsistencyPolicy.DefaultConsistencyLevel.ToString(),
                    MultiRegionWrites = data.EnableMultipleWriteLocations ?? false
                };

                PolicyAssignmentCollection policies = cosmosDb.GetPolicyAssignments();
                List<string> locations = [];
                foreach (CosmosDBAccountLocation location in data.Locations
                    .OrderBy(loc => loc.FailoverPriority)
                    .ThenBy(loc => loc.LocationName))
                {
                    string locationName = location.LocationName?.DisplayName ?? location.LocationName?.Name ?? string.Empty;
                    locations.Add(location.IsZoneRedundant == true ? $"{locationName} (ZRS)" : locationName);
                }
                dto.Locations = locations;
                if (data.BackupPolicy is ContinuousModeBackupPolicy continuousBackupPolicy)
                {
                    dto.Backup = CosmosDbDto.BackupPolicy.CreateContinuous();
                }
                else if (data.BackupPolicy is PeriodicModeBackupPolicy periodicBackupPolicy)
                {
                    dto.Backup = CosmosDbDto.BackupPolicy.CreatePeriodic(
                        periodicBackupPolicy.PeriodicModeProperties.BackupIntervalInMinutes ?? 0,
                        periodicBackupPolicy.PeriodicModeProperties.BackupRetentionIntervalInHours ?? 0,
                        periodicBackupPolicy.PeriodicModeProperties.BackupStorageRedundancy.ToString() ?? string.Empty
                    );
                }
                result.Add(dto);
            }
            return result.OrderBy(item => item.Name);
        }
    }
}

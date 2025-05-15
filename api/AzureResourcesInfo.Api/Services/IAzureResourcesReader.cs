using AzureResourcesInfo.Api.Dto;

namespace AzureResourcesInfo.Api.Services
{
    internal interface IAzureResourcesReader
    {
        Task<IEnumerable<SubscriptionDto>> GetSubscriptions(CancellationToken cancellationToken);
        Task<IEnumerable<CosmosDbDto>> GetCosmosDbAccounts(CancellationToken cancellationToken);
    }
}

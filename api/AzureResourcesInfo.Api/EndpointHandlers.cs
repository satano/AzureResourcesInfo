using AzureResourcesInfo.Api.Dto;
using AzureResourcesInfo.Api.Services;

namespace AzureResourcesInfo.Api
{
    internal class EndpointHandlers
    {
        public static async Task<IEnumerable<SubscriptionDto>> GetSubscriptions(
            IAzureResourcesReader resourceReader,
            CancellationToken cancellationToken)
            => await resourceReader.GetSubscriptions(cancellationToken);

        public static async Task<IEnumerable<CosmosDbDto>> GetCosmosDbAccounts(
            IAzureResourcesReader resourceReader,
            CancellationToken cancellationToken)
            => await resourceReader.GetCosmosDbAccounts(cancellationToken);
    }
}

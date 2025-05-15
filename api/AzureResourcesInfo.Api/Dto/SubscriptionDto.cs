namespace AzureResourcesInfo.Api.Dto
{
    internal class SubscriptionDto
    {
        public string SubscriptionId { get; set; } = string.Empty;
        public Guid TenantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
    }
}

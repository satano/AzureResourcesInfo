namespace AzureResourcesInfo.Api.Dto
{
    internal class CosmosDbDto
    {
        public class BackupPolicy
        {
            public static BackupPolicy CreateContinuous() => new()
            {
                Type = "Continuous",
                Properties = null
            };

            public static BackupPolicy CreatePeriodic(int intervalInMinutes, int retentionInHours, string redundancy) => new()
            {
                Type = "Periodic",
                Properties = new PeriodicBackupPolicyProperties()
                {
                    IntervalInMinutes = intervalInMinutes,
                    RetentionInHours = retentionInHours,
                    Redundancy = redundancy
                }
            };

            public string Type { get; set; } = string.Empty;
            public object? Properties { get; set; }
        }

        public class PeriodicBackupPolicyProperties
        {
            public int IntervalInMinutes { get; set; }
            public int RetentionInHours { get; set; }
            public string Redundancy { get; set; } = string.Empty;
        }

        public string Id { get; set; } = string.Empty;
        public Uri? ResourceUrl { get; set; }
        public string ResourceGroup { get; set; } = string.Empty;
        public Uri? ResourceGroupUrl { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;

        public IEnumerable<string> Locations { get; set; } = [];
        public string DefaultConsistencyLevel { get; set; } = string.Empty;
        public bool MultiRegionWrites { get; set; } = false;
        public BackupPolicy Backup { get; set; } = new();
    }
}

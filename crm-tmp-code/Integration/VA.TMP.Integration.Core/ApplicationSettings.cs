namespace VA.TMP.Integration.Core
{
    public class ApplicationSettings
    {
        public string ApiVersion { get; set; }
        public string AppId { get; set; }
        public string BaseUrl { get; set; }
        public string GroupResourcesFetchXml { get; set; }
        public string KeyVaultSecretName { get; set; }
        public int SchedulingPackagesCacheDuration { get; set; }
        public string SchedulingPackagesFetchXml { get; set; }
        public string SchedulingPackagesGroupResourcesFetchXml { get; set; }
        public string Scope { get; set; }
        public string SingleResourcesFetchXml { get; set; }
        public string TenantId { get; set; }
        public int TimeOut { get; set; }
        public int VISNCacheDuration { get; set; }
    }
}

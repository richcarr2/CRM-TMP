namespace VA.TMP.Integration.Plugins.Helpers
{
    public class ApiIntegrationSettings
    {
        public string Resource { get; set; }

        public string AppId { get; set; }

        public string Secret { get; set; }

        public string Authority { get; set; }

        public string TenantId { get; set; }

        public string SubscriptionId { get; set; }

        public string BaseUrl { get; set; }

        public string Uri { get; set; }

        public bool IsProdApi { get; set; }

        public string SubscriptionIdEast { get; set; }

        public string SubscriptionIdSouth { get; set; }

        public string LogicAppUri { get; set; }

        public string CernerKey { get; set; }
    }

    public class ApiIntegrationSettingsNameValuePair
    {
        public string Name { get; set; }

        public string Value { get; set; }
    }
}

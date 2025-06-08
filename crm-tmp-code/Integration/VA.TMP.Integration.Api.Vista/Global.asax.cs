using System.Configuration;
using System.Web.Http;

namespace VA.TMP.Integration.Api.Vista
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration.Active.InstrumentationKey = ConfigurationManager.AppSettings["InstrumentationKey"];
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}

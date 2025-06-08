using System.Net;
using System.Net.Http;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;

namespace VA.TMP.Integration.Api.HealthShare.Controllers
{
    public class PingController : ApiController
    {
        [SwaggerOperation("Ping")]
        public HttpResponseMessage Get()
        {
            return Request.CreateResponse(HttpStatusCode.OK, "Pong");
        }
    }
}

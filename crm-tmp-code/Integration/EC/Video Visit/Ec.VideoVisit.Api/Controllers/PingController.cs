using System.Net;
using System.Net.Http;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;

namespace Ec.VideoVisit.Api.Controllers
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

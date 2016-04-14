using System.Web.Http;

namespace MicroService4Net.Example.Controllers
{
    public class ExampleController : ApiController
    {
        [Route("Example")]
        public IHttpActionResult GetExample()
        {
            return Ok(new { Msg = "Example" });
        }
    }
}

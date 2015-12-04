using System.Web.Http;
using Newtonsoft.Json.Linq;

namespace MicroService4Net.Example.Controllers
{
    public class ExampleController : ApiController
    {
        private static readonly string ExampleField;

        static ExampleController()
        {
            ExampleField = "Example";
        }

        [Route("Example")]
        public IHttpActionResult GetExample()
        {
            var x = new {Msg = ExampleField };

            return Ok(x);
        }

        [Route("Example")]
        public IHttpActionResult PostExample([FromBody]JObject x)
        {
            return Ok(x);
        }


    }
}

using System.Web.Http;
using System.Web.Http.Cors;

namespace MicroService4Net.Example.Controllers
{
    [EnableCors("*","*","*")]
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
            return Ok(ExampleField);
        }



    }
}

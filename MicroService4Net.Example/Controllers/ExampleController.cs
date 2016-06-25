using System.Web.Http;

namespace MicroService4Net.Example.Controllers
{
    public class ExampleController : ApiController
    {
        [Route("Example")]
        public string GetExample()
        {
            return "Example";
        }
    }
}

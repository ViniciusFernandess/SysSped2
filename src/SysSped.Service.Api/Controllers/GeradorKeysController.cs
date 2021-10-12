using SysSped.Domain.Core.Services;
using System.Web.Http;

namespace SysSped.Service.Api.Controllers
{
    [Route("api/[controller]")]
    public class GeradorKeysController : ApiController
    {
        [HttpGet]
        public string Get()
        {
            return new GeradorKeysService().Gerar("abcd");
        }
    }
}

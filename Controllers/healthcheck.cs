using cv_iidx_api;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;

namespace cv_iidx_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthCheckController : ControllerBase
    {

        private readonly ILogger<cviidxController> _logger;

        public HealthCheckController(ILogger<cviidxController> logger)
        {
            _logger = logger;
        }

        

        [HttpGet(Name = "healthcheck")]
        public IEnumerable<string> Get()
        {
            return new List<string>() {"yeah im alive" };
        }
    }
}

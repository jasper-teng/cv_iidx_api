using cv_iidx_api;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.Drawing.Printing;

namespace cv_iidx_api.Controllers
{

    public class BodyRes()
    {
        public string imgpath { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    public class cviidxController : ControllerBase
    {

        private readonly ILogger<cviidxController> _logger;

        public cviidxController(ILogger<cviidxController> logger)
        {
            _logger = logger;
        }

        

        [HttpGet(Name = "cviidx")]
        public async Task<IEnumerable<IIDXcvResults>> GetAsync([FromQuery]BodyRes uri)
        {
            Console.WriteLine("send to cv");

            Console.WriteLine(uri);

            SingletonIIDXComputerVisionContainer parser = SingletonIIDXComputerVisionContainer.Instance;

            IIDXcvResults yrmom = new IIDXcvResults() { Artist = "your mother" };
            List<IIDXcvResults> res2 = [yrmom];
            try
            {
                IIDXcvResults results = await parser.ParseImage(uri.imgpath);

                //get the songlist

                return new List<IIDXcvResults> { results };
            }
            catch (Exception ex) { 

            }

            return res2;
        }
    }
}

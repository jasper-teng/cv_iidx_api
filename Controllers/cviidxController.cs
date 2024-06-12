using cv_iidx_api;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;

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

        

        [HttpPost(Name = "cviidx")]
        public IEnumerable<IIDXcvResults> Post([FromBody]BodyRes temp)
        {
            Console.WriteLine("send to cv");

            var askdj = temp;


            IIDXcvResults yrmom = new IIDXcvResults() { Artist = "your mother" };

            List<IIDXcvResults> res = [yrmom];

            //testing stuff

            Song tempsong = new Song()
            {
                Artist = "Falsion",
                Title = "Feel The Beat",
                Charts = new Dictionary<string, List<Chart>>()
            };

            tempsong.Charts.Add("Single", new List<Chart> { new Chart() {Difficulty = Difficulty.Normal }, new Chart( ) { Difficulty = Difficulty.Another }, new Chart() { Difficulty = Difficulty.Leggendaria, notes = "1509", wr = "3013", avg = "2672", coef = "0.738874" } } );


            //get the songlist
            var x = BPIandScore.CalculateBPI(tempsong, 2923, Enum.Parse<Difficulty>("Leggendaria"));

            return res;
        }
    }
}

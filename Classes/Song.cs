namespace cv_iidx_api
{
    public class Song
    {
        public String Title { get; set; }
        public String Artist { get; set; }

        public String Genre { get; set; }

        public Dictionary<String, List<Chart>> Charts { get; set; }
    }
}

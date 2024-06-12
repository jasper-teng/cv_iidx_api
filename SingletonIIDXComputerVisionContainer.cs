using Sdcb.PaddleInference;
using Sdcb.PaddleOCR.Models;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models.Local;
using OpenCvSharp;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using Compunet.YoloV8;
using Compunet.YoloV8.Plotting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using FuzzySharp;

namespace cv_iidx_api
{
    public class SingletonIIDXComputerVisionContainer : ISingletonCVContainer
    {

        private string ResultScreenModel = "1";
        private string ScoreModel = "1";
        private FullOcrModel PaddleOCR = LocalFullModels.JapanV4;
        private List<Song> SongList = new List<Song>();
        private List<string> SongArtistList = new List<String>();
        private YoloV8Predictor ScoreCardPredictor;
        private YoloV8Predictor NumbersPredictor;
        private PaddleOcrAll PaddleOcrPredictor;


        //maybe get this from args or package it in the same directory
        private string scorecardModelFilePath = "resscreen.onnx";
        private string numbersModelFilePath = "score.onnx";
/*
        private string imageFilePath = "C:\\Users\\Jasper\\Downloads\\IMG_4269.png";
        private string outImageFilePath = "C:\\Users\\Jasper\\Downloads\\fuck.png";*/

        private SingletonIIDXComputerVisionContainer()
        {
            Console.WriteLine("instantiating singleton of computer vision related code");

            //create the songlist and keep it in memory

            string filepath = "merge2.json";

            string jsonstring = File.ReadAllText(filepath);

            SongList = JsonConvert.DeserializeObject<List<Song>>(jsonstring);

            foreach(var i in SongList)
            {
                SongArtistList.Add(i.Title + " | " + i.Artist);
            }

            Console.WriteLine(SongList);
            //do a lot of stuff

            //generate the models here

            //scorecard
            ScoreCardPredictor = YoloV8Predictor.Create(scorecardModelFilePath);
            //numbers of the score
            NumbersPredictor = YoloV8Predictor.Create(numbersModelFilePath);

            //paddleocr reading
            PaddleOcrPredictor = new PaddleOcrAll(PaddleOCR, PaddleDevice.Onnx()) { 
                AllowRotateDetection = true,
                Enable180Classification = false,
            };

            var ouh = ParseImage();

            //run the thing for debug
        }


        private static Image CropImage(Image image, Rectangle rectangle)
        {
            Image copy = image.Clone(copy => copy.Crop(rectangle));
            return copy;
        }
        
        public async Task<IIDXcvResults> ParseImage()
        {
            try
            {/*
                //image conversion
                Image img = Image.Load(imageFilePath);

                var ScoreCardResult = ScoreCardPredictor.Detect(imageFilePath); //imagefilepath should be like whatever
                                                                                //you can pass in an Image class object also.

                //work with the ScoreCardResult here.
                //get 2 crops, SongCrop and ScoreCrop.
                var a = ScoreCardResult.Boxes.Where(x => x.Class.Name == "songcard");


                Image SongCrop = CropImage(img, ScoreCardResult.Boxes.Where(x => x.Class.Name == "songcard").First().Bounds);
                Image ScoreCrop = CropImage(img, ScoreCardResult.Boxes.Where(x => x.Class.Name == "score").First().Bounds);

                //read score

                Task<int> ScoreTask = ReadScore(ScoreCrop);
                int score = await ScoreTask;

                Task<string> SongTask = ReadSong(SongCrop);
                string song = await SongTask;

                Song GuessedSong = GuessSong(song);
                Console.WriteLine(GuessedSong);

                //thoughts about moving the calculation stuff to the request handler instead. Because this singleton is a thread by itself it may be better to move it over to a different thread for calcs

                return BPIandScore.CalculateBPI(GuessedSong, score, Difficulty.Another);
                return new IIDXcvResults();*/
                return new IIDXcvResults();
            }
            catch (Exception e) {

                //do exceptions here
                //TODO:

                return new IIDXcvResults();
            }


        }

        private Song GuessSong(string song)
        {
            var GuessResults = FuzzySharp.Process.ExtractTop(song, SongArtistList, limit: 10); //use extract top temporarily for debugging check
            var SongGuess = GuessResults.First();


            return SongList[SongArtistList.FindIndex(x => x.Equals(SongGuess.Value))];
        }

        async Task<int> ReadScore(Image score)
        {
            var ScoreCardResults = NumbersPredictor.Detect(score);
            //sort this motherfucker by X pos

            var boxes = ScoreCardResults.Boxes;
            List<String> sortedBoxes = boxes.OrderBy(x => x.Bounds.X).Select(x => x.Class.Name).ToList();

            //Bruh
            Dictionary<string, int> numberword = new Dictionary<string, int> { { "zero", 0 }, { "one", 1 }, { "two", 2 }, { "three", 3 }, { "four", 4 }, { "five", 5 }, { "six", 6 }, { "seven", 7 }, { "eight", 8 }, { "nine", 9 } };

            string concatnums = "";
            foreach (var box in sortedBoxes) {
                concatnums += numberword[box];
            }
            return int.Parse(concatnums);
        }

        async Task<string> ReadSong(Image song)
        {
            //stupid workaround because imagesharp and opencvsharp do not mix together
            //can be optimized but not rn
            string randomname = Guid.NewGuid().ToString();
            song.SaveAsPng(randomname + ".png");
            string songstring = "";
            using (Mat src = Cv2.ImRead(randomname + ".png"))
            {
                File.Delete(randomname + ".png");
                PaddleOcrResult result = PaddleOcrPredictor.Run(src);
                foreach (var x in result.Regions)
                {//filter to clean up results for fuzzy search
                    if (x.Text == "SP" || x.Text == "DP" || x.Text == "BEGINNER" || x.Text == "HYPER" || x.Text == "ANOTHER" || x.Text == "NORMAL" || x.Text == "LEGGENDARIA" || x.Text == "NOTES")
                    {
                        continue;
                    }
                    songstring += x.Text;
                    songstring += " ";
                }
                
            }

            return songstring;
        }

        private static SingletonIIDXComputerVisionContainer instance = new SingletonIIDXComputerVisionContainer();
        public static SingletonIIDXComputerVisionContainer Instance => instance;

    }

}

using System.Reflection.Metadata.Ecma335;
using System.Linq;

namespace cv_iidx_api
{

    public enum Difficulty
    {
        Beginner,
        Normal,
        Hyper,
        Another,
        Leggendaria,
        BlackAnother
    }

    static public class BPIandScore
    {
        private static float PikaGreatFunction(float theoreticalmax, float exscore)
        {

            if (exscore == theoreticalmax)
            {
                return ((float)(theoreticalmax * 0.8));
            }
            else
            {
                return ((float)(1.0 + ((exscore / theoreticalmax) - 0.5) / (1.0 - exscore / theoreticalmax)));
            }

        }
        public static IIDXcvResults CalculateBPI(Song song, int exscore, Difficulty difficulty) //adaptation of sera's rust implementation
        {
            Chart TargetChart = song.Charts["Single"].FirstOrDefault(x => x.Difficulty == difficulty);

            if (TargetChart == null)
            {
                return new IIDXcvResults();
            }

            //BPI data can be expressed in 3 ways
            //1) properly
            //2) doesn't exist
            //3) -1

            float wr = TargetChart.wr == null ? -1 : int.Parse(TargetChart.wr);
            float kavg = TargetChart.avg == null ? -1 : int.Parse(TargetChart.avg);
            float theoreticalmax = int.Parse(TargetChart.notes) * 2;

            //stupid tenerary chain
            float coef = (float)(TargetChart.coef == null ? (decimal)1.175 : TargetChart.coef == "-1" ? (decimal)1.175 : decimal.Parse(TargetChart.coef));

            if (exscore > theoreticalmax)
            {

            }

            IIDXcvResults res = new IIDXcvResults() { Score = exscore };

            // calc grade and target

            List<int> TargetGrades = new List<int>(); //list of exact scores required to get a certain grade
            List<string> Grades = ["F", "E", "D", "C", "B", "A", "AA", "AAA", "MAX"];

            for (int i = 1; i < 10; i++)
            {
                TargetGrades.Add((int)Math.Ceiling(theoreticalmax * (i / 9.0)));
            }

            //the list acts as a number line
            int gradeindex = 0;
            while (exscore > TargetGrades[gradeindex]) {
                gradeindex++;
            }

            res.Grade = Grades[gradeindex - 1];

            //deal with F case
            if (gradeindex == 0)
            {
                res.Grade = "F";
            }

            //get closest number
            int ClosestScore = TargetGrades.OrderBy(x => Math.Abs(x - exscore)).First();
            res.Border = $"{Grades[TargetGrades.FindIndex(x => x == ClosestScore)]}{(ClosestScore > exscore ? '-' : '+')}{Math.Abs(ClosestScore - exscore)}";


            //do bpi stuff now
            if (kavg > 0 && wr > 0)
            {
                //calc pgf values
                float ScorePrime = PikaGreatFunction(theoreticalmax, exscore);
                float WorldPrime = PikaGreatFunction(theoreticalmax, wr);
                float KavgPrime = PikaGreatFunction(theoreticalmax, kavg);

                float bruh = exscore / theoreticalmax;
                //normalize score
                float ScoreNormal = ScorePrime / KavgPrime;
                float WorldNormal = WorldPrime / KavgPrime;
                float inner = 0;
                //handle pos and neg bpi values
                if(exscore > kavg) {
                    inner = (float)(100.0 * Math.Pow( (Math.Log(ScoreNormal) / Math.Log(WorldNormal) ) ,coef) * 100.0);
                }
                else
                {
                    inner = (float)(-100.0 * Math.Pow(-(Math.Log(ScoreNormal) / Math.Log(WorldNormal)), coef) * 100.0);
                }

                //truncate to 2 decimal places
                res.BPI = Math.Max(-15, Math.Round(decimal.Round((decimal)(inner / 100), 2, MidpointRounding.AwayFromZero), 2));
            };

            //truncate to 2 decimal places
            res.Percentage = Math.Round(decimal.Round((decimal)(exscore / theoreticalmax * 100), 2, MidpointRounding.AwayFromZero), 2).ToString() + "%";

            res.Artist = song.Artist;
            res.SongTitle = song.Title;


            return res;
        }

    }
}

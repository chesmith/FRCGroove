namespace FRCGroove.Lib.Models.FRCv2
{
    public class EventRanking
    {
        public int rank { get; set; }
        public int teamNumber { get; set; }
        public double sortOrder1 { get; set; }
        public double sortOrder2 { get; set; }
        public double sortOrder3 { get; set; }
        public double sortOrder4 { get; set; }
        public double sortOrder5 { get; set; }
        public double sortOrder6 { get; set; }
        public int wins { get; set; }
        public int losses { get; set; }
        public int ties { get; set; }
        public double qualAverage { get; set; }
        public int dq { get; set; }
        public int matchesPlayed { get; set; }
    }
}
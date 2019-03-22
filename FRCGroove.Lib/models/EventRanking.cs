namespace FRCGroove.Lib.models
{
    public class EventRanking
    {
        public int rank { get; set; }
        public int teamNumber { get; set; }
        public double sortOrder1 { get; set; }
        public int sortOrder2 { get; set; }
        public int sortOrder3 { get; set; }
        public int sortOrder4 { get; set; }
        public int sortOrder5 { get; set; }
        public int sortOrder6 { get; set; }
        public int wins { get; set; }
        public int losses { get; set; }
        public int ties { get; set; }
        public double qualAverage { get; set; }
        public int dq { get; set; }
        public int matchesPlayed { get; set; }
    }
}
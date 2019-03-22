namespace FRCGroove.Win.models
{
    public class DistrictRank
    {
        public string districtCode { get; set; }
        public int teamNumber { get; set; }
        public int rank { get; set; }
        public int totalPoints { get; set; }
        public string event1Code { get; set; }
        public int event1Points { get; set; }
        public string event2Code { get; set; }
        public int? event2Points { get; set; }
        public object districtCmpCode { get; set; }
        public object districtCmpPoints { get; set; }
        public int teamAgePoints { get; set; }
        public int adjustmentPoints { get; set; }
        public bool qualifiedDistrictCmp { get; set; }
        public bool qualifiedFirstCmp { get; set; }
    }
}
using FRCGroove.Lib.Models.Statboticsv2;

namespace FRCGroove.Lib.Models.TBAv3
{
    public class TBATeam
    {
        public string key { get; set; }
        public int team_number { get; set; }
        public string nickname { get; set; }
        public string name { get; set; }
        public string school_name { get; set; }
        public string city { get; set; }
        public string state_prov { get; set; }
        public string country { get; set; }
        public string address { get; set; }
        public string postal_code { get; set; }
        public string gmaps_place_id { get; set; }
        public string gmaps_url { get; set; }
        public int lat { get; set; }
        public int lng { get; set; }
        public string location_name { get; set; }
        public string website { get; set; }
        public int rookie_year { get; set; }
        public string motto { get; set; }

        public int eventRank { get; set; } = -1;
        //public int districtRank { get; set; } = -1;

        public TBAStats Stats { get; set; }
        public EPA epa { get; set; }

        public string champsDivision { get; set; }
        public string pitLocation { get; set; }
    }
}

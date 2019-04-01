using RestSharp.Deserializers;

namespace FRCGroove.Lib.models
{
    public class RegisteredTeam
    {
        public int teamNumber { get; set; }

        public string nameFull { get; set; }
        public string nameShort { get; set; }
        public string schoolName { get; set; }
        public string city { get; set; }
        public string stateProv { get; set; }
        public string country { get; set; }
        public string website { get; set; }
        public int rookieYear { get; set; }
        public string robotName { get; set; }
        public string districtCode { get; set; }
        public string homeCMP { get; set; }

        public int eventRank { get; set; } = -1;
        public int districtRank { get; set; } = -1;

        public TBAStats Stats { get; set; }
        public Match NextMatch { get; set; }

        public int number { get { return teamNumber; } }
    }
}

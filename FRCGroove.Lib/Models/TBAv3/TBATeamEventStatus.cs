namespace FRCGroove.Lib.Models.TBAv3
{
    public class TBATeamEventStatus
    {
        public Alliance alliance { get; set; }
        //public string alliance_status_str { get; set; }
        //public string last_match_key { get; set; }
        //public string next_match_key { get; set; }
        //public string overall_status_str { get; set; }
        public Playoff playoff { get; set; }
        //public string playoff_status_str { get; set; }
        public Qual qual { get; set; }

        public class Alliance
        {
            //public object backup { get; set; }
            public string name { get; set; }
            public int number { get; set; }
            public int pick { get; set; }
        }

        //public class CurrentLevelRecord
        //{
        //    public int losses { get; set; }
        //    public int ties { get; set; }
        //    public int wins { get; set; }
        //}

        public class Playoff
        {
            //public CurrentLevelRecord current_level_record { get; set; }
            public string double_elim_round { get; set; }
            //public string level { get; set; }
            //public int playoff_type { get; set; }
            //public Record record { get; set; }
            public string status { get; set; }
        }

        public class Qual
        {
            public int num_teams { get; set; }
            public Ranking ranking { get; set; }
            //public List<SortOrderInfo> sort_order_info { get; set; }
            //public string status { get; set; }
        }

        public class Ranking
        {
            //public int dq { get; set; }
            //public int matches_played { get; set; }
            //public object qual_average { get; set; }
            public int rank { get; set; }
            //public Record record { get; set; }
            //public List<double> sort_orders { get; set; }
            //public string team_key { get; set; }
        }

        //public class Record
        //{
        //    public int losses { get; set; }
        //    public int ties { get; set; }
        //    public int wins { get; set; }
        //}

        //public class SortOrderInfo
        //{
        //    public string name { get; set; }
        //    public int precision { get; set; }
        //}
    }
}

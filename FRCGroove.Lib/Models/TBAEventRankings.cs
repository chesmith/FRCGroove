using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRCGroove.Lib.Models
{
    public class TBAEventRankings
    {
        public List<TBARanking> rankings { get; set; }
        //public List<ExtraStatsInfo> extra_stats_info { get; set; }
        //public List<SortOrderInfo> sort_order_info { get; set; }
    }

    public class TBARanking
    {
        public int matches_played { get; set; }
        public int qual_average { get; set; }
        public List<int> extra_stats { get; set; }
        public List<int> sort_orders { get; set; }
        public Record record { get; set; }
        public int rank { get; set; }
        public int dq { get; set; }
        public string team_key { get; set; }
    }

    public class Record
    {
        public int losses { get; set; }
        public int wins { get; set; }
        public int ties { get; set; }
    }


    //public class ExtraStatsInfo
    //{
    //    public int precision { get; set; }
    //    public string name { get; set; }
    //}

    //public class SortOrderInfo
    //{
    //    public int precision { get; set; }
    //    public string name { get; set; }
    //}
}

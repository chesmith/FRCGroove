using System.Collections.Generic;

namespace FRCGroove.Lib.Models
{
    public class DistrictRankListing
    {
        public List<DistrictRank> districtRanks { get; set; }
        public int rankingCountTotal { get; set; }
        public int rankingCountPage { get; set; }
        public int pageCurrent { get; set; }
        public int pageTotal { get; set; }
    }
}

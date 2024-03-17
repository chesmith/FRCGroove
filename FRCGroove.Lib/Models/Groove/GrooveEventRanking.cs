using FRCGroove.Lib.Models.TBAv3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FRCGroove.Lib.Models.Groove
{
    public class GrooveEventRanking
    {
        public int teamNumber { get; set; }
        public int rank { get; set; }

        public GrooveEventRanking(TBARanking ranking)
        {
            teamNumber = Int32.Parse(Regex.Replace(ranking.team_key, "[^0-9,-]+", ""));
            rank = ranking.rank;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRCGroove.Lib.Models
{
    public class ScoutingAssignment
    {
        public int MatchNumber { get; set; }
        public DateTime MatchScheduleTime { get; set; }
        public DateTime MatchEstimateTime { get; set; }
        public List<RegisteredTeam> Teams { get; set; }
    }
}

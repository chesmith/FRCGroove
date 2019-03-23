using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using FRCGroove.Lib.models;

namespace FRCGroove.Web.Models
{
    public class Dashboard
    {
        public List<RegisteredTeam> Teams { get; set; }
        public double ScheduleOffset { get; set; }
        public int EventRank { get; set; } = -1;    //TODO: move to RegisteredTeam
        public int DistrictRank { get; set; } = -1; //TODO: ditto
        public List<Match> Matches { get; set; }
        public TBAStats Stats { get; set; }
    }
}
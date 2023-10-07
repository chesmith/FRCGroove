using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FRCGroove.Web.Models
{
    public class SelectionTeamData
    {
        public string EventCode { get; set; }
        public int TeamNumber { get; set; }
        public string TeamName { get; set; }
        public int TBAMatchCount { get; set; }
        public int DQCount { get; set; }
        public int AutoMobility { get; set; }
        public int AutoDock { get; set; }
        public int AutoEngage { get; set; }
        public int EndgameDock { get; set; }
        public int EndgameEngage { get; set; }
        public int EndgamePark { get; set; }
        public double OPR { get; set; }
        public double DPR { get; set; }
        public double CCWM { get; set; }
        public int Rank { get; set; }
    }
}
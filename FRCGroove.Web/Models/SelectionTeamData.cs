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
        public int AutoTaxi { get; set; }
        public int ClimbLow { get; set; }
        public int ClimbMid { get; set; }
        public int ClimbHigh { get; set; }
        public int ClimbTraversal { get; set; }
        public int CargoMatchCount { get; set; }
        public int CargoAutoHigh { get; set; }
        public int CargoAutoLow { get; set; }
        public int CargoTeleopHigh { get; set; }
        public int CargoTeleopLow { get; set; }
        public double OPR { get; set; }
        public double DPR { get; set; }
        public double CCWM { get; set; }
        public int Rank { get; set; }
    }
}
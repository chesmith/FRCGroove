using System;
using System.Collections.Generic;

namespace FRCGroove.Lib.models
{
    public class Match
    {
        public string description { get; set; }
        public string field { get; set; }
        public string tournamentLevel { get; set; }
        public DateTime startTime { get; set; }
        public int matchNumber { get; set; }
        public List<Team> teams { get; set; }

        //hybrid schedule only - these will not be available in standard schedule
        public DateTime actualStartTime { get; set; }
        public DateTime postResultTime { get; set; }
        public int scoreRedFinal { get; set; }
        public int scoreRedFoul { get; set; }
        public int scoreRedAuto { get; set; }
        public int scoreBlueFinal { get; set; }
        public int scoreBlueFoul { get; set; }
        public int scoreBlueAuto { get; set; }
    }
}

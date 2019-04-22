using System;
using System.Collections.Generic;

namespace FRCGroove.Lib.Models
{
    public class Match
    {
        static private Dictionary<int, string> playoffTitles = new Dictionary<int, string>()
        {
            {1, "Quarterfinal 1-1"},
            {2, "Quarterfinal 2-1"},
            {3, "Quarterfinal 3-1"},
            {4, "Quarterfinal 4-1"},
            {5, "Quarterfinal 1-2"},
            {6, "Quarterfinal 2-2"},
            {7, "Quarterfinal 3-2"},
            {8, "Quarterfinal 4-2"},
            {9, "Quarterfinal 1-3"},
            {10, "Quarterfinal 2-3"},
            {11, "Quarterfinal 3-3"},
            {12, "Quarterfinal 4-3"},
            {13, "Semifinal 1-1"},
            {14, "Semifinal 2-1"},
            {15, "Semifinal 1-2"},
            {16, "Semifinal 2-2"},
            {17, "Semifinal 1-3"},
            {18, "Semifinal 2-3"},
            {19, "Final 1"},
            {20, "Final 2"},
            {21, "Final 3"},
            {22, "Final 4"}
        };

        static private Dictionary<int, string> playoffIds = new Dictionary<int, string>()
        {
            {1, "qf1m1"},
            {2, "qf2m1"},
            {3, "qf3m1"},
            {4, "qf4m1"},
            {5, "qf1m2"},
            {6, "qf2m2"},
            {7, "qf3m2"},
            {8, "qf4m2"},
            {9, "qf1m3"},
            {10, "qf2m3"},
            {11, "qf3m3"},
            {12, "qf4m3"},
            {13, "sf1m1"},
            {14, "sf2m1"},
            {15, "sf1m2"},
            {16, "sf2m2"},
            {17, "sf1m3"},
            {18, "sf2m3"},
            {19, "f1m1"},
            {20, "f1m2"},
            {21, "f1m3"},
            {22, "f1m4"}
        };
        public string description { get; set; }
        public string field { get; set; }
        public string tournamentLevel { get; set; }
        public DateTime startTime { get; set; }
        public int matchNumber { get; set; }
        public List<Team> teams { get; set; }

        //hybrid schedule only - these will not be available in standard schedule
        public DateTime? actualStartTime { get; set; }
        public DateTime? postResultTime { get; set; }
        public int? scoreRedFinal { get; set; }
        public int? scoreRedFoul { get; set; }
        public int? scoreRedAuto { get; set; }
        public int? scoreBlueFinal { get; set; }
        public int? scoreBlueFoul { get; set; }
        public int? scoreBlueAuto { get; set; }

        public string eventCode { get; set; }

        public string title
        {
            get
            {
                if (description.StartsWith("Einstein"))
                {
                    if (matchNumber <= 15)
                    {
                        return $"Semifinal {matchNumber}";
                    }
                    else
                        return $"Final {matchNumber - 15}";
                }
                else
                {
                    if (tournamentLevel == "Playoff")
                    {
                        return playoffTitles[this.matchNumber];
                    }
                    return $"{tournamentLevel} {matchNumber}";
                }
            }
        }

        public string matchDetailsUrl
        {
            get
            {
                if(tournamentLevel == "Qualification")
                {
                    return "https://www.thebluealliance.com/match/2019" + eventCode.ToLower() + "_qm" + matchNumber;
                }
                else
                {
                    //TODO: probably some way to do this by mods and remainders
                    return "https://www.thebluealliance.com/match/2019" + eventCode.ToLower() + "_" + playoffIds[matchNumber];
                }
            }
        }
    }
}

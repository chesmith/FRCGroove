using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRCGroove.Lib.Models
{
    public class PlayoffBracket
    {
        public Dictionary<string, int> brackets;

        public PlayoffBracket(List<Alliance> alliances, List<Match> matches)
        {
            if (alliances != null && matches != null)
            {
                foreach (Alliance alliance in alliances)
                {
                    alliance.LoadTeams();
                }

                brackets = new Dictionary<string, int>();

                List<Match> sf1 = matches.Where(m => m.title == "Semifinal 1-1").ToList();
                List<Match> sf2 = matches.Where(m => m.title == "Semifinal 2-1").ToList();
                List<Match> f = matches.Where(m => m.title == "Final 1").ToList();

                brackets["qf1-red"] = 1;
                brackets["qf1-blue"] = 8;
                brackets["qf2-red"] = 4;
                brackets["qf2-blue"] = 5;
                brackets["qf3-red"] = 2;
                brackets["qf3-blue"] = 7;
                brackets["qf4-red"] = 3;
                brackets["qf4-blue"] = 6;

                brackets["sf1-red"] = alliances.Where(a => sf1.Exists(m => m.teams.Exists(t => t.number == a.captain && t.station.StartsWith("Red")))).Select(a => a.number).FirstOrDefault();
                brackets["sf1-blue"] = alliances.Where(a => sf1.Exists(m => m.teams.Exists(t => t.number == a.captain && t.station.StartsWith("Blue")))).Select(a => a.number).FirstOrDefault();
                brackets["sf2-red"] = alliances.Where(a => sf2.Exists(m => m.teams.Exists(t => t.number == a.captain && t.station.StartsWith("Red")))).Select(a => a.number).FirstOrDefault();
                brackets["sf2-blue"] = alliances.Where(a => sf2.Exists(m => m.teams.Exists(t => t.number == a.captain && t.station.StartsWith("Blue")))).Select(a => a.number).FirstOrDefault();

                brackets["f-red"] = alliances.Where(a => f.Exists(m => m.teams.Exists(t => t.number == a.captain && t.station.StartsWith("Red")))).Select(a => a.number).FirstOrDefault();
                brackets["f-blue"] = alliances.Where(a => f.Exists(m => m.teams.Exists(t => t.number == a.captain && t.station.StartsWith("Blue")))).Select(a => a.number).FirstOrDefault();
            }
        }
    }
}

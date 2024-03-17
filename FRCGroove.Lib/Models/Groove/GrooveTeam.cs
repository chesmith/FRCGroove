using FRCGroove.Lib.Models.FRCv2;
using FRCGroove.Lib.Models.Statboticsv2;
using FRCGroove.Lib.Models.TBAv3;

namespace FRCGroove.Lib.Models.Groove
{
    public class GrooveTeam
    {
        public int number { get; set; }
        public string name { get; set; }

        public int eventRank { get; set; } = -1;

        public TBAStats Stats { get; set; }
        public EPA epa { get; set; }

        public string champsDivision { get; set; }
        public string pitLocation { get; set; }

        public GrooveTeam() { }

        public GrooveTeam(TBATeam team)
        {
            number = team.team_number;
            name = team.nickname;

            // TODO: These are assumed to be enriched at some point earlier - okay to reference as we do here?
            eventRank = team.eventRank;
            Stats = team.Stats;
            epa = team.epa;
        }

        public GrooveTeam(RegisteredTeam team)
        {
            number = team.teamNumber;
            name = team.nameShort;
        }
    }
}

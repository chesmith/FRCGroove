using FRCGroove.Lib.Models.Groove;
using FRCGroove.Lib.Models.TBAv3;
using System.Collections.Generic;

namespace FRCGroove.Web.Models
{
    public class TeamListing
    {
        // TODO: "TBATeams" is a temporary bridge while I shift things to TBA
        public List<GrooveTeam> Teams { get; set; }
        public List<TBATeam> TBATeams { get; set; }

        public List<int> Watchlist { get; set; }
    }
}
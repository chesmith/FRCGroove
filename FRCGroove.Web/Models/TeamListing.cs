using FRCGroove.Lib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FRCGroove.Web.Models
{
    public class TeamListing
    {
        // TODO: "TBATeams" is a temporary bridge while I shift things to TBA
        public List<RegisteredTeam> Teams { get; set; }
        public List<TBATeam> TBATeams { get; set; }

        public List<int> Watchlist { get; set; }
    }
}
using FRCGroove.Lib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FRCGroove.Web.Models
{
    public class TeamListing
    {
        public List<RegisteredTeam> Teams { get; set; }
        public List<int> Watchlist { get; set; }
    }
}
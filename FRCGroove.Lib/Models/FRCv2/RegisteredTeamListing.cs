using System.Collections.Generic;

namespace FRCGroove.Lib.Models.FRCv2
{
    public class RegisteredTeamListing
    {
        public List<RegisteredTeam> teams { get; set; }
        public int teamCountTotal { get; set; }
        public int teamCountPage { get; set; }
        public int pageCurrent { get; set; }
        public int pageTotal { get; set; }
    }
}

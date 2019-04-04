using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRCGroove.Lib.models
{
    public class Alliance
    {
        public int number { get; set; }
        public string name { get; set; }
        public int captain { get; set; }
        public int round1 { get; set; }
        public int round2 { get; set; }
        public object round3 { get; set; }
        public object backup { get; set; }
        public object backupReplaced { get; set; }

        public List<RegisteredTeam> teams { get; set; }

        public void LoadTeams()
        {
            teams = new List<RegisteredTeam>();
            //TODO:?
            //teams.Add(FRCEventsAPI.GetTeam(captain));
            //teams.Add(FRCEventsAPI.GetTeam(round1));
            //teams.Add(FRCEventsAPI.GetTeam(round2));

            teams.Add(new RegisteredTeam() { teamNumber = captain });
            teams.Add(new RegisteredTeam() { teamNumber = round1 });
            teams.Add(new RegisteredTeam() { teamNumber = round2 });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRCGroove.Lib.Models.Statbotics
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);
    public class Breakdown
    {
        public double total_points { get; set; }
        public double auto_points { get; set; }
        public double teleop_points { get; set; }
        public double endgame_points { get; set; }
        public double auto_rp { get; set; }
        public double coral_rp { get; set; }
        public double barge_rp { get; set; }
        public double tiebreaker_points { get; set; }
        public double auto_leave_points { get; set; }
        public double auto_coral { get; set; }
        public double auto_coral_points { get; set; }
        public double teleop_coral { get; set; }
        public double teleop_coral_points { get; set; }
        public double coral_l1 { get; set; }
        public double coral_l2 { get; set; }
        public double coral_l3 { get; set; }
        public double coral_l4 { get; set; }
        public double total_coral_points { get; set; }
        public double processor_algae { get; set; }
        public double processor_algae_points { get; set; }
        public double net_algae { get; set; }
        public double net_algae_points { get; set; }
        public double total_algae_points { get; set; }
        public double total_game_pieces { get; set; }
        public double barge_points { get; set; }
        public double rp_1 { get; set; }
        public double rp_2 { get; set; }
        public double rp_3 { get; set; }
    }

    public class Competing
    {
        public bool this_week { get; set; }
        public string next_event_key { get; set; }
        public string next_event_name { get; set; }
        public int? next_event_week { get; set; }
    }

    public class Country
    {
        public int rank { get; set; }
        public double percentile { get; set; }
        public int team_count { get; set; }
    }

    public class District
    {
        public int rank { get; set; }
        public double percentile { get; set; }
        public int team_count { get; set; }
    }

    public class Epa
    {
        public TotalPoints total_points { get; set; }
        public double unitless { get; set; }
        public double norm { get; set; }
        public List<double> conf { get; set; }
        public Breakdown breakdown { get; set; }
        public Stats stats { get; set; }
        public Ranks ranks { get; set; }
    }

    public class Ranks
    {
        public Total total { get; set; }
        public Country country { get; set; }
        public State state { get; set; }
        public District district { get; set; }
    }

    public class Record
    {
        public int wins { get; set; }
        public int losses { get; set; }
        public int ties { get; set; }
        public int count { get; set; }
        public double winrate { get; set; }
    }

    public class Statbotics_v3
    {
        public int team { get; set; }
        public int year { get; set; }
        public string name { get; set; }
        public string country { get; set; }
        public string state { get; set; }
        public string district { get; set; }
        public Epa epa { get; set; }
        public Record record { get; set; }
        public int? district_points { get; set; }
        public int? district_rank { get; set; }
        public Competing competing { get; set; }
    }

    public class State
    {
        public int rank { get; set; }
        public double percentile { get; set; }
        public int team_count { get; set; }
    }

    public class Stats
    {
        public double start { get; set; }
        public double pre_champs { get; set; }
        public double max { get; set; }
    }

    public class Total
    {
        public int rank { get; set; }
        public double percentile { get; set; }
        public int team_count { get; set; }
    }

    public class TotalPoints
    {
        public double mean { get; set; }
        public double sd { get; set; }
    }

}

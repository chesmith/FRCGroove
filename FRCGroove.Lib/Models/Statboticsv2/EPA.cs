using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRCGroove.Lib.Models.Statboticsv2
{
    public class EPA
    {
        public int year { get; set; }
        public int team { get; set; }
        public bool offseason { get; set; }
        public string name { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string district { get; set; }
        public bool is_competing { get; set; }
        public double? epa_start { get; set; }
        public double? epa_pre_champs { get; set; }
        public double? epa_end { get; set; }
        public double? epa_mean { get; set; }
        public double? epa_max { get; set; }
        public double? epa_diff { get; set; }
        public double? auto_epa_start { get; set; }
        public double? auto_epa_pre_champs { get; set; }
        public double? auto_epa_end { get; set; }
        public double? auto_epa_mean { get; set; }
        public double? auto_epa_max { get; set; }
        public double? teleop_epa_start { get; set; }
        public double? teleop_epa_pre_champs { get; set; }
        public double? teleop_epa_end { get; set; }
        public double? teleop_epa_mean { get; set; }
        public double? teleop_epa_max { get; set; }
        public double? endgame_epa_start { get; set; }
        public double? endgame_epa_pre_champs { get; set; }
        public double? endgame_epa_end { get; set; }
        public double? endgame_epa_mean { get; set; }
        public double? endgame_epa_max { get; set; }
        public double? rp_1_epa_start { get; set; }
        public double? rp_1_epa_pre_champs { get; set; }
        public double? rp_1_epa_end { get; set; }
        public double? rp_1_epa_mean { get; set; }
        public double? rp_1_epa_max { get; set; }
        public double? rp_2_epa_start { get; set; }
        public double? rp_2_epa_pre_champs { get; set; }
        public double? rp_2_epa_end { get; set; }
        public double? rp_2_epa_mean { get; set; }
        public double? rp_2_epa_max { get; set; }
        public double? unitless_epa_end { get; set; }
        public double? norm_epa_end { get; set; }
        public int? wins { get; set; }
        public int? losses { get; set; }
        public int? ties { get; set; }
        public int? count { get; set; }
        public double? winrate { get; set; }
        public int? full_wins { get; set; }
        public int? full_losses { get; set; }
        public int? full_ties { get; set; }
        public int? full_count { get; set; }
        public double? full_winrate { get; set; }
        public int? total_epa_rank { get; set; }
        public double? total_epa_percentile { get; set; }
        public int? total_team_count { get; set; }
        public int? country_epa_rank { get; set; }
        public double? country_epa_percentile { get; set; }
        public int? country_team_count { get; set; }
        public int? state_epa_rank { get; set; }
        public double? state_epa_percentile { get; set; }
        public int? state_team_count { get; set; }
        public int? district_epa_rank { get; set; }
        public double? district_epa_percentile { get; set; }
        public int? district_team_count { get; set; }
    }
}

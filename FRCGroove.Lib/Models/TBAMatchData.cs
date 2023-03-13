using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRCGroove.Lib.Models
{
    public class TBAAlliance
    {
        public List<string> dq_team_keys { get; set; }
        public int score { get; set; }
        public List<object> surrogate_team_keys { get; set; }
        public List<string> team_keys { get; set; }
        public int adjustPoints { get; set; }
        public int autoCargoLowerBlue { get; set; }
        public int autoCargoLowerFar { get; set; }
        public int autoCargoLowerNear { get; set; }
        public int autoCargoLowerRed { get; set; }
        public int autoCargoPoints { get; set; }
        public int autoCargoTotal { get; set; }
        public int autoCargoUpperBlue { get; set; }
        public int autoCargoUpperFar { get; set; }
        public int autoCargoUpperNear { get; set; }
        public int autoCargoUpperRed { get; set; }
        public int autoPoints { get; set; }
        public int autoTaxiPoints { get; set; }
        public bool cargoBonusRankingPoint { get; set; }
        public int endgamePoints { get; set; }
        public string endgameRobot1 { get; set; }
        public string endgameRobot2 { get; set; }
        public string endgameRobot3 { get; set; }
        public int foulCount { get; set; }
        public int foulPoints { get; set; }
        public bool hangarBonusRankingPoint { get; set; }
        public int matchCargoTotal { get; set; }
        public bool quintetAchieved { get; set; }
        public int rp { get; set; }
        public string taxiRobot1 { get; set; }
        public string taxiRobot2 { get; set; }
        public string taxiRobot3 { get; set; }
        public int techFoulCount { get; set; }
        public int teleopCargoLowerBlue { get; set; }
        public int teleopCargoLowerFar { get; set; }
        public int teleopCargoLowerNear { get; set; }
        public int teleopCargoLowerRed { get; set; }
        public int teleopCargoPoints { get; set; }
        public int teleopCargoTotal { get; set; }
        public int teleopCargoUpperBlue { get; set; }
        public int teleopCargoUpperFar { get; set; }
        public int teleopCargoUpperNear { get; set; }
        public int teleopCargoUpperRed { get; set; }
        public int teleopPoints { get; set; }
        public int totalPoints { get; set; }
    }

    public class Alliances
    {
        public TBAAlliance blue { get; set; }
        public TBAAlliance red { get; set; }
    }

    public class ScoreBreakdown
    {
        public TBAAlliance blue { get; set; }
        public TBAAlliance red { get; set; }
    }

    public class TBAMatchData
    {
        Dictionary<string, string> _champsCodes = new Dictionary<string, string>
        {
            { "CARVER", "carv" },
            { "GALILEO", "gal" },
            { "HOPPER", "hop" },
            { "NEWTON", "new" },
            { "ROEBLING", "roe" },
            { "TURING", "tur" }
        };

        public int actual_time { get; set; }
        public DateTime actual_timeDT
        {
            get
            {
                DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                dateTime = dateTime.AddSeconds(this.actual_time).ToLocalTime();
                return dateTime;
            }
        }
        public Alliances alliances { get; set; }
        public string comp_level { get; set; }
        public string event_key { get; set; }
        public string key { get; set; }
        public int match_number { get; set; }
        public int post_result_time { get; set; }
        public int predicted_time { get; set; }
        public DateTime predicted_timeDT
        {
            get
            {
                DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                dateTime = dateTime.AddSeconds(this.predicted_time).ToLocalTime();
                return dateTime;
            }
        }
        public ScoreBreakdown score_breakdown { get; set; }
        public int set_number { get; set; }
        public int time { get; set; }
        public DateTime timeDT
        {
            get
            {
                DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                dateTime = dateTime.AddSeconds(this.time).ToLocalTime();
                return dateTime;
            }
        }

        public string winning_alliance { get; set; }

        public Scoring TransformScoring()
        {
            Scoring scoring = new Scoring();
            scoring.teams = new string[] {
                alliances.blue.team_keys[0], alliances.blue.team_keys[1], alliances.blue.team_keys[2],
                alliances.red.team_keys[0], alliances.red.team_keys[1], alliances.red.team_keys[2]
            };
            scoring.taxi.Add(alliances.blue.team_keys[0], score_breakdown.blue.taxiRobot1);
            scoring.taxi.Add(alliances.blue.team_keys[1], score_breakdown.blue.taxiRobot2);
            scoring.taxi.Add(alliances.blue.team_keys[2], score_breakdown.blue.taxiRobot3);
            scoring.endgame.Add(alliances.blue.team_keys[0], score_breakdown.blue.endgameRobot1);
            scoring.endgame.Add(alliances.blue.team_keys[1], score_breakdown.blue.endgameRobot2);
            scoring.endgame.Add(alliances.blue.team_keys[2], score_breakdown.blue.endgameRobot3);
            scoring.taxi.Add(alliances.red.team_keys[0], score_breakdown.red.taxiRobot1);
            scoring.taxi.Add(alliances.red.team_keys[1], score_breakdown.red.taxiRobot2);
            scoring.taxi.Add(alliances.red.team_keys[2], score_breakdown.red.taxiRobot3);
            scoring.endgame.Add(alliances.red.team_keys[0], score_breakdown.red.endgameRobot1);
            scoring.endgame.Add(alliances.red.team_keys[1], score_breakdown.red.endgameRobot2);
            scoring.endgame.Add(alliances.red.team_keys[2], score_breakdown.red.endgameRobot3);

            return scoring;
        }

        public string matchDetailsUrl
        {
            get { return "https://www.thebluealliance.com/match/" + key; }
        }

        public string title
        {
            get
            {
                switch (comp_level)
                {
                    case "qm": return $"Qualification {match_number}";
                    case "qf": return $"Quarterfinal {set_number}-{match_number}";
                    case "sf": return $"Semifinal {set_number}";
                    case "f": return $"Final {match_number}";
                }

                return string.Empty;
            }
        }

        public string sortTitle
        {
            get
            {
                switch (comp_level)
                {
                    case "qm": return $"01 {match_number:00}";
                    case "qf": return $"02 {set_number:00}-{match_number:00}";
                    case "sf": return $"03 {set_number:00}-{match_number:00}";
                    case "f": return $"04 {match_number:00}";
                }

                return string.Empty;
            }
        }
    }

    public class Scoring
    {
        public string[] teams { get; set; }
        public Dictionary<string, string> taxi { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> endgame { get; set; } = new Dictionary<string, string>();
    }
}

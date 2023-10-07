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
        public int autoPoints { get; set; }
        public int autoTaxiPoints { get; set; }
        public int endgamePoints { get; set; }
        public int foulCount { get; set; }
        public int foulPoints { get; set; }
        public int rp { get; set; }
        public int techFoulCount { get; set; }
        public int teleopPoints { get; set; }
        public int totalPoints { get; set; }

        //prediction (statbotics)
        public int predictedPoints { get; set; }

        //2023 specific
        public bool activationBonusAchieved { get; set; }
        public string autoBridgeState { get; set; }
        public int autoChargeStationPoints { get; set; }
        public string autoChargeStationRobot1 { get; set; }
        public string autoChargeStationRobot2 { get; set; }
        public string autoChargeStationRobot3 { get; set; }
        public bool autoDocked { get; set; }
        public int autoGamePieceCount { get; set; }
        public int autoGamePiecePoints { get; set; }
        public int autoMobilityPoints { get; set; }
        public int coopGamePieceCount { get; set; }
        public bool coopertitionCriteriaMet { get; set; }
        public string endGameBridgeState { get; set; }
        public int endGameChargeStationPoints { get; set; }
        public string endGameChargeStationRobot1 { get; set; }
        public string endGameChargeStationRobot2 { get; set; }
        public string endGameChargeStationRobot3 { get; set; }
        public int endGameParkPoints { get; set; }
        public int linkPoints { get; set; }
        public string mobilityRobot1 { get; set; }
        public string mobilityRobot2 { get; set; }
        public string mobilityRobot3 { get; set; }
        public bool sustainabilityBonusAchieved { get; set; }
        public int teleopGamePieceCount { get; set; }
        public int teleopGamePiecePoints { get; set; }
        public int totalChargeStationPoints { get; set; }
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
            scoring.automobility.Add(alliances.blue.team_keys[0], score_breakdown.blue.mobilityRobot1);
            scoring.automobility.Add(alliances.blue.team_keys[1], score_breakdown.blue.mobilityRobot1);
            scoring.automobility.Add(alliances.blue.team_keys[2], score_breakdown.blue.mobilityRobot1);
            scoring.autodock.Add(alliances.blue.team_keys[0], score_breakdown.blue.autoChargeStationRobot1 + score_breakdown.blue.autoBridgeState);
            scoring.autodock.Add(alliances.blue.team_keys[1], score_breakdown.blue.autoChargeStationRobot2 + score_breakdown.blue.autoBridgeState);
            scoring.autodock.Add(alliances.blue.team_keys[2], score_breakdown.blue.autoChargeStationRobot3 + score_breakdown.blue.autoBridgeState);
            scoring.endgame.Add(alliances.blue.team_keys[0], score_breakdown.blue.endGameChargeStationRobot1 + score_breakdown.blue.endGameBridgeState);
            scoring.endgame.Add(alliances.blue.team_keys[1], score_breakdown.blue.endGameChargeStationRobot2 + score_breakdown.blue.endGameBridgeState);
            scoring.endgame.Add(alliances.blue.team_keys[2], score_breakdown.blue.endGameChargeStationRobot3 + score_breakdown.blue.endGameBridgeState);

            scoring.automobility.Add(alliances.red.team_keys[0], score_breakdown.red.mobilityRobot1);
            scoring.automobility.Add(alliances.red.team_keys[1], score_breakdown.red.mobilityRobot1);
            scoring.automobility.Add(alliances.red.team_keys[2], score_breakdown.red.mobilityRobot1);
            scoring.autodock.Add(alliances.red.team_keys[0], score_breakdown.red.autoChargeStationRobot1 + score_breakdown.red.autoBridgeState);
            scoring.autodock.Add(alliances.red.team_keys[1], score_breakdown.red.autoChargeStationRobot2 + score_breakdown.red.autoBridgeState);
            scoring.autodock.Add(alliances.red.team_keys[2], score_breakdown.red.autoChargeStationRobot3 + score_breakdown.red.autoBridgeState);
            scoring.endgame.Add(alliances.red.team_keys[0], score_breakdown.red.endGameChargeStationRobot1 + score_breakdown.red.endGameBridgeState);
            scoring.endgame.Add(alliances.red.team_keys[1], score_breakdown.red.endGameChargeStationRobot2 + score_breakdown.red.endGameBridgeState);
            scoring.endgame.Add(alliances.red.team_keys[2], score_breakdown.red.endGameChargeStationRobot3 + score_breakdown.red.endGameBridgeState);

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
                    case "qm": return $"01 {match_number:000}";
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
        public Dictionary<string, string> automobility { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> autodock { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> endgame { get; set; } = new Dictionary<string, string>();
    }
}

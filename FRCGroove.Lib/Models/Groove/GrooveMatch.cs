using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security.AntiXss;
using FRCGroove.Lib.Models.FRCv2;
using FRCGroove.Lib.Models.TBAv3;

namespace FRCGroove.Lib.Models.Groove
{
    public class GrooveMatch
    {
        public string eventKey { get; set; }    // follows TBA format
        public string competitionLevel { get; set; }    // Qualification, Playoff, Final
        public int setNumber { get; set; }
        public int matchNumber { get; set; }
        public string matchKey { get; set; }
        public DateTime timeScheduled { get; set; } = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        public long timeScheduledUnix { get { return ((DateTimeOffset)timeScheduled).ToUnixTimeSeconds(); } }
        public DateTime timeActual { get; set; } = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        public long timeActualUnix { get { return ((DateTimeOffset)timeActual).ToUnixTimeSeconds(); } }
        public bool hasStarted { get { return timeActual > new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc); } }
        public string winningAlliance { get; set; }
        public Dictionary<string, Alliance> alliances { get; set; } = new Dictionary<string, Alliance>();

        public string matchDetailsUrl
        {
            get { return "https://www.thebluealliance.com/match/" + matchKey; }
        }

        public string title
        {
            get
            {
                switch (competitionLevel)
                {
                    case "Qualification": return $"Qualification {matchNumber}";
                    case "Playoff":
                        switch (setNumber)
                        {
                            case 1: return "Upper Bracket R-1 M-1";
                            case 2: return "Upper Bracket R-1 M-2";
                            case 3: return "Upper Bracket R-1 M-3";
                            case 4: return "Upper Bracket R-1 M-4";
                            case 5: return "Lower Bracket R-2 M-5";
                            case 6: return "Lower Bracket R-2 M-6";
                            case 7: return "Upper Bracket R-2 M-7";
                            case 8: return "Upper Bracket R-2 M-8";
                            case 9: return "Lower Bracket R-3 M-9";
                            case 10: return "Lower Bracket R-3 M-10";
                            case 11: return "Upper Bracket R-4 M-11";
                            case 12: return "Lower Bracket R-4 M-12";
                            case 13: return "Lower Bracket R-5 M-13";
                        }
                        break;
                    case "Final": return $"Final {matchNumber}";
                }

                return string.Empty;
            }
        }

        public string sortTitle
        {
            get
            {
                switch (competitionLevel)
                {
                    case "Qualification": return $"01 {matchNumber:000}";
                    case "Playoff": return $"03 {setNumber:00}-{matchNumber:00}";
                    case "Final": return $"04 {matchNumber:00}";
                }

                return string.Empty;
            }
        }

        public class Alliance
        {
            public List<string> teamKeys { get; set; }
            public List<string> dqTeamKeys { get; set; }
            public List<string> surrogateTeamKeys { get; set; }

            public int score { get; set; }
            public int rp { get; set; }
            public int totalPoints { get; set; }

            public int predictedPoints { get; set; }
        }

        public GrooveMatch(TBAMatchData match)
        {
            eventKey = match.event_key;
            competitionLevel = (match.comp_level == "qm" ? "Qualification" : (match.comp_level == "sf" ? "Playoff" : "Final"));
            setNumber = match.set_number;
            matchNumber = match.match_number;
            matchKey = match.key;
            timeScheduled = match.timeDT;
            timeActual = match.actual_timeDT;
            winningAlliance = match.winning_alliance;

            alliances["blue"] = new Alliance()
            {
                teamKeys = match.alliances.blue.team_keys,
                dqTeamKeys = match.alliances.blue.dq_team_keys,
                surrogateTeamKeys = match.alliances.blue.surrogate_team_keys,

                score = match.alliances.blue.score,
                rp = (match.score_breakdown != null ? match.score_breakdown.blue.rp : -1),
                totalPoints = (match.score_breakdown != null ? match.score_breakdown.blue.totalPoints : -1),
                predictedPoints = match.alliances.blue.predictedPoints
            };

            alliances["red"] = new Alliance()
            {
                teamKeys = match.alliances.red.team_keys,
                dqTeamKeys = match.alliances.red.dq_team_keys,
                surrogateTeamKeys = match.alliances.red.surrogate_team_keys,

                score = match.alliances.red.score,
                rp = (match.score_breakdown != null ? match.score_breakdown.red.rp : -1),
                totalPoints = (match.score_breakdown != null ? match.score_breakdown.red.totalPoints : -1),
                predictedPoints = match.alliances.red.predictedPoints
            };
        }

        public GrooveMatch(FRCMatch match)
        {
            eventKey = $"{DateTime.Now.Year}{match.eventCode.ToLower()}";
            if (match.tournamentLevel == "Qualification")
            {
                competitionLevel = "Qualification";
                setNumber = 1;
            }
            else if (match.description.StartsWith("Match"))
            {
                competitionLevel = "Playoff";
                setNumber = Int32.Parse(match.description.Substring(6, 2).Trim());
            }
            else
            {
                competitionLevel = "Final";
                setNumber = 1;
            }
            matchNumber = match.matchNumber;
            if (competitionLevel == "Qualification")
                matchKey = $"{eventKey}_qm{matchNumber}";
            else if (competitionLevel == "Playoff")
                matchKey = $"{eventKey}_sf{setNumber}m{matchNumber}";
            else
                matchKey = $"{eventKey}_f{setNumber}m{matchNumber}";

            timeScheduled = match.startTime;
            timeActual = match.actualStartTime.Value;

            alliances["blue"] = new Alliance()
            {
                teamKeys = match.teams.Where(t => t.station.StartsWith("Blue")).Select(t => $"frc{t.teamNumber}").ToList(),
                dqTeamKeys = match.teams.Where(t => t.station.StartsWith("Blue") && t.dq.Value).Select(t => $"frc{t.teamNumber}").ToList(),
                surrogateTeamKeys = match.teams.Where(t => t.station.StartsWith("Blue") && t.surrogate).Select(t => $"frc{t.teamNumber}").ToList(),

                score = match.scoreBlueFinal.Value,
                rp = 0, // TODO (normalize - prob have to use ScoreDetails in v3)
                totalPoints = match.scoreBlueFinal.Value,
                predictedPoints = 0 // TODO (normalize - never did predicted score with FRC)
            };

            alliances["red"] = new Alliance()
            {
                teamKeys = match.teams.Where(t => t.station.StartsWith("Red")).Select(t => $"frc{t.teamNumber}").ToList(),
                dqTeamKeys = match.teams.Where(t => t.station.StartsWith("Red") && t.dq.Value).Select(t => $"frc{t.teamNumber}").ToList(),
                surrogateTeamKeys = match.teams.Where(t => t.station.StartsWith("Red") && t.surrogate).Select(t => $"frc{t.teamNumber}").ToList(),

                score = match.scoreRedFinal.Value,
                rp = 0, // TODO (normalize - prob have to use ScoreDetails in v3)
                totalPoints = match.scoreRedFinal.Value,
                predictedPoints = 0 // TODO (normalize - never did predicted score with FRC)
            };

            if (alliances["blue"].score > alliances["red"].score)
                winningAlliance = "blue";
            else if (alliances["blue"].score < alliances["red"].score)
                winningAlliance = "red";
            // else it was a tie and this should remain empty
        }
    }
}

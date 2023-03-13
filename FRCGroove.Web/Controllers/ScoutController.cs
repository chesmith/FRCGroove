using GenericParsing;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using FRCGroove.Lib.Models;
using FRCGroove.Lib;
using FRCGroove.Web.Models;
using System.Text;

namespace FRCGroove.Web.Controllers
{
    public class ScoutController : Controller
    {
        private readonly string _appDataFolder = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();

        // GET: Scout
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult LogCargo(string team, string match, string period, string goal, int amount, int clientTotal)
        {
            // team = team number
            // match = match number (assumes quals only)
            // period = "auto" or "teleop"
            // goal = "low" or "high"

            string address = System.Web.HttpContext.Current.Request.UserHostAddress;
            string b = System.Web.HttpContext.Current.Request.Headers["X-Forwarded-For"];
            string c = System.Web.HttpContext.Current.Request.Headers["REMOTE_ADDR"];

            string path = Path.Combine(_appDataFolder, "data.csv");
            string data = $"{DateTime.Now:G},{team},{match},{period},{goal},{amount},{clientTotal},{address},{b},{c}\r\n";
            System.IO.File.AppendAllText(path, data);

            return new JsonResult() { Data = new { team = team, match = match, period = period, goal = goal, count = amount, clientTotal = clientTotal } };
        }

        public String Data(int? team, int? match)
        {
            //if (team != null && match != null)
            //{
            //    DataTable table;
            //    using (GenericParserAdapter parser = new GenericParserAdapter(Path.Combine(_appDataFolder, "data.csv")))
            //    {
            //        parser.ColumnDelimiter = ',';
            //        parser.FirstRowSetsExpectedColumnCount = true;
            //        parser.FirstRowHasHeader = false;

            //        table = parser.GetDataTable();
            //    }

            //    var data = table.AsEnumerable().Where(r => ((string)r[1] == team.ToString() && (string)r[2] == match.ToString()));
            //    int autohigh = data.Where(r => (r[3].Equals("auto") && r[4].Equals("high"))).Sum(r => Int32.Parse((string)r[5]));
            //    int autolow = data.Where(r => (r[3].Equals("auto") && r[4].Equals("low"))).Sum(r => Int32.Parse((string)r[5]));
            //    int teleophigh = data.Where(r => (r[3].Equals("teleop") && r[4].Equals("high"))).Sum(r => Int32.Parse((string)r[5]));
            //    int teleoplow = data.Where(r => (r[3].Equals("teleop") && r[4].Equals("low"))).Sum(r => Int32.Parse((string)r[5]));

            //    var result = new { autohigh = autohigh, autolow = autolow, teleophigh = teleophigh, teleoplow = teleoplow };

            //    return JsonConvert.SerializeObject(result);
            //}
            //else if (team != null && match == null)
            //{
            //    DataTable table;
            //    using (GenericParserAdapter parser = new GenericParserAdapter(Path.Combine(_appDataFolder, "data.csv")))
            //    {
            //        parser.ColumnDelimiter = ',';
            //        parser.FirstRowSetsExpectedColumnCount = true;
            //        parser.FirstRowHasHeader = false;

            //        table = parser.GetDataTable();
            //    }

            //    var data = table.AsEnumerable().Where(r => ((string)r[1] == team.ToString()));
            //    int autohigh = data.Where(r => (r[3].Equals("auto") && r[4].Equals("high"))).Sum(r => Int32.Parse((string)r[5]));
            //    int autolow = data.Where(r => (r[3].Equals("auto") && r[4].Equals("low"))).Sum(r => Int32.Parse((string)r[5]));
            //    int teleophigh = data.Where(r => (r[3].Equals("teleop") && r[4].Equals("high"))).Sum(r => Int32.Parse((string)r[5]));
            //    int teleoplow = data.Where(r => (r[3].Equals("teleop") && r[4].Equals("low"))).Sum(r => Int32.Parse((string)r[5]));

            //    int matches = data.Select(r => r[2]).Distinct().Count();

            //    var result = new { autohigh = (double)autohigh / (double)matches, autolow = (double)autolow / (double)matches, teleophigh = (double)teleophigh / (double)matches, teleoplow = (double)teleoplow / (double)matches };

            //    return JsonConvert.SerializeObject(result);
            //}
            //else
            //{
            //    return System.IO.File.ReadAllText(Path.Combine(_appDataFolder, "data.csv"));
            //}

            StringBuilder sb = new StringBuilder();
            List<string> allEventCodes = new List<string>();
            //var districts = FRCEventsAPI.GetDistrictListing();
            //foreach(var d in districts)
            //{
                var events = FRCEventsAPI.GetEventListing();
                //var events = FRCEventsAPI.GetDistrictEventListing(d.code);
                foreach (var e in events)
                {
                    var matches = TBAAPI.GetMatches("2023" + e.code.ToLower());
                    foreach(var m in matches)
                    {
                        if (m.score_breakdown != null)
                        {
                            if (m.score_breakdown.blue.endgameRobot1 == "Traversal"
                                && m.score_breakdown.blue.endgameRobot2 == "Traversal"
                                && m.score_breakdown.blue.endgameRobot3 == "Traversal")
                            {
                                //sb.AppendLine($"{d.code},{e.code},{m.match_number},blue,{String.Join(",", m.alliances.blue.team_keys)}<br />");
                                sb.AppendLine($"{e.code},{m.comp_level},{m.match_number},blue,{String.Join(",", m.alliances.blue.team_keys)}<br />");
                            }

                            if (m.score_breakdown.red.endgameRobot1 == "Traversal"
                                && m.score_breakdown.red.endgameRobot2 == "Traversal"
                                && m.score_breakdown.red.endgameRobot3 == "Traversal")
                            {
                                //sb.AppendLine($"{d.code},{e.code},{m.match_number},red,{String.Join(",", m.alliances.red.team_keys)}<br />");
                                sb.AppendLine($"{e.code},{m.comp_level},{m.match_number},red,{String.Join(",", m.alliances.red.team_keys)}<br />");
                            }
                        }
                    }
                }
            //}

            return sb.ToString();
        }

        public ActionResult PreScout(string tbaEventCode)
        {
            //for each team, gather previous event codes
            //List<TBATeam> teams = TBAAPI.GetEventTeams(tbaEventCode);
            List<string> allEventCodes = new List<string>();
            List<RegisteredTeam> teams = FRCEventsAPI.GetEventTeamListing(tbaEventCode.Substring(4).ToUpper());
            
            Dictionary<int, List<string>> teamEvents = new Dictionary<int, List<string>>();
            foreach (RegisteredTeam team in teams)
            {
                List<string> eventCodes = TBAAPI.GetTeamEvents("frc" + team.teamNumber);
                teamEvents.Add(team.teamNumber, eventCodes);
                allEventCodes.AddRange(eventCodes);
            }
            //List<string> allcodes = teamEvents.Select(t => t.Value.Select(s => s)).Distinct().ToList();
            allEventCodes = allEventCodes.Distinct().ToList();

            //for each event code, generate a cache of data
            Dictionary<string, Dictionary<int, SelectionTeamData>> cacheTeamData = new Dictionary<string, Dictionary<int, SelectionTeamData>>();
            foreach (string eventCode in allEventCodes)
            {
                List<TBAMatchData> matches = TBAAPI.GetMatches(eventCode);
                List<TBAMatchData> quals = matches.Where(m => m.comp_level == "qm" && m.score_breakdown != null).ToList();
                Dictionary<int, SelectionTeamData> teamData = GatherTeamData(eventCode, quals);

                cacheTeamData.Add(eventCode, teamData);
            }

            //for each team, build lists of SelectionTeamData
            Dictionary<int, List<SelectionTeamData>> results = new Dictionary<int, List<SelectionTeamData>>();
            foreach (RegisteredTeam team in teams)
            {
                List<SelectionTeamData> data = new List<SelectionTeamData>();
                List<string> events = teamEvents[team.teamNumber];
                foreach (string eventCode in events)
                {
                    if (cacheTeamData[eventCode].ContainsKey(team.teamNumber))
                    {
                        data.Add(cacheTeamData[eventCode][team.teamNumber]);
                    }
                }

                results.Add(team.teamNumber, data);
            }

            //load stats
            foreach (string eventCode in allEventCodes)
            {
                TBAStatsCollection stats = TBAAPI.GetStats(eventCode);
                List<EventRanking> eventRankings = FRCEventsAPI.GetEventRankings(eventCode.Substring(4));
                foreach (int teamNumber in results.Keys)
                {
                    SelectionTeamData data = results[teamNumber].Where(d => d.EventCode == eventCode).FirstOrDefault();
                    if(data != null)
                    {
                        data.OPR = stats.oprs["frc" + teamNumber];
                        data.DPR = stats.dprs["frc" + teamNumber];
                        data.CCWM = stats.ccwms["frc" + teamNumber];
                        data.Rank = eventRankings.Where(r => r.teamNumber == teamNumber).FirstOrDefault().rank;
                    }
                }
            }

            return View(results);
        }

        public ActionResult List(string tbaEventCode)
        {
            List<Match> matches = FRCEventsAPI.GetEventSchedule(tbaEventCode.Substring(4).ToUpper());
            DateTime firstMatchDate = matches[0].startTime.Date;
            if(tbaEventCode.Contains("cmp"))
                matches = matches.Where(m => m.startTime.Date == firstMatchDate || m.startTime.Date <= firstMatchDate.AddDays(1)).ToList();
            else
                matches = matches.Where(m => m.startTime.Date == firstMatchDate).ToList();

            List<TBATeam> teams = TBAAPI.GetEventTeams(tbaEventCode);
            Dictionary<int, string> results = new Dictionary<int, string>();
            //bool toggle = false;
            foreach(TBATeam team in teams)
            {
                int[] a = { 1, 3, 5, 7 };
                int[] b = { 2, 4, 6, 8 };
                int[] c = { 1, 3, 5, 8 };

                List<Match> teamMatches = matches.Where(m => m.teams.Select(t => t.number).Contains(team.team_number)).ToList();

                //results.Add(team.team_number, String.Join(", ", teamMatches.Select(m => m.matchNumber)));


                //selectMatches = String.Join(", ", teamMatches[a[0]].matchNumber, teamMatches[a[1]].matchNumber, teamMatches[a[2]].matchNumber, teamMatches[a[3]].matchNumber);
                //selectMatches = String.Join(", ", teamMatches[b[0]].matchNumber, teamMatches[b[1]].matchNumber, teamMatches[b[2]].matchNumber, teamMatches[b[3]].matchNumber);
                //selectMatches = String.Join(", ", teamMatches[c[0]].matchNumber, teamMatches[c[1]].matchNumber, teamMatches[c[2]].matchNumber, teamMatches[c[3]].matchNumber);

                //toggle = !toggle;
                //if(toggle)
                //    selectMatches = String.Join(",", teamMatches[a[0]].matchNumber, teamMatches[a[1]].matchNumber, teamMatches[a[2]].matchNumber, teamMatches[a[3]].matchNumber);
                //else
                //    selectMatches = String.Join(",", teamMatches[b[0]].matchNumber, teamMatches[b[1]].matchNumber, teamMatches[b[2]].matchNumber, teamMatches[b[3]].matchNumber);

                
                string selectMatches = String.Join(", ", teamMatches[c[0]].matchNumber, teamMatches[c[1]].matchNumber, teamMatches[c[2]].matchNumber, teamMatches[c[3]].matchNumber);
                results.Add(team.team_number, selectMatches);
            }

            return View(TransposeResults(matches, results));
        }

        public SortedDictionary<int, ScoutingAssignment> TransposeResults(List<Match> schedule, Dictionary<int, string> input)
        {
            SortedDictionary<int, ScoutingAssignment> results = new SortedDictionary<int, ScoutingAssignment>();

            foreach(int teamNumber in input.Keys)
            {
                RegisteredTeam team = FRCEventsAPI.TeamListingCache[teamNumber];

                string[] matches = input[teamNumber].Split(',');
                foreach(string match in matches)
                {
                    int matchNumber = Int32.Parse(match);
                    Match m = schedule.Where(s => s.matchNumber == matchNumber).FirstOrDefault();
                    if (results.ContainsKey(matchNumber))
                    {
                        ScoutingAssignment s = results[matchNumber];
                        s.Teams.Add(team);
                    }
                    else
                    {
                        ScoutingAssignment s = new ScoutingAssignment()
                        {
                            MatchNumber = matchNumber,
                            MatchScheduleTime = m.startTime,
                            Teams = new List<RegisteredTeam>() { team }
                        };
                        results.Add(matchNumber, s);
                    }
                }
            }

            return results;
        }

        public ActionResult Selection(string tbaEventCode, string sort = "")
        {
            List<TBAMatchData> matches = TBAAPI.GetMatches(tbaEventCode);
            List<TBAMatchData> quals = matches.Where(m => m.comp_level == "qm" && m.score_breakdown != null).ToList();

            Dictionary<int, SelectionTeamData> teamData = GatherTeamData(tbaEventCode, quals);

            List<EventRanking> eventRankings = FRCEventsAPI.GetEventRankings(tbaEventCode.Substring(4));

            TBAStatsCollection stats = TBAAPI.GetStats(tbaEventCode);
            Dictionary<int, int[]> cargoCounts = GetCargoCounts();
            foreach (int teamNumber in teamData.Keys)
            {
                SelectionTeamData data = teamData[teamNumber];
                data.OPR = stats.oprs["frc" + teamNumber];
                data.DPR = stats.dprs["frc" + teamNumber];
                data.CCWM = stats.ccwms["frc" + teamNumber];
                data.Rank = eventRankings.Where(r => r.teamNumber == teamNumber).FirstOrDefault().rank;
                if (cargoCounts != null && cargoCounts.ContainsKey(teamNumber))
                {
                    int[] cargo = cargoCounts[teamNumber];
                    data.CargoAutoHigh = cargo[0];
                    data.CargoAutoLow = cargo[1];
                    data.CargoTeleopHigh = cargo[2];
                    data.CargoTeleopLow = cargo[3];
                    data.CargoMatchCount = cargo[4];
                }
            }

            Dictionary<int, SelectionTeamData> result;

            if (sort.ToLower() == "team")
                result = teamData.OrderBy(t => t.Value.TeamNumber).ToDictionary(t => t.Key, t => t.Value);
            else if (sort.ToLower() == "opr")
                result = teamData.OrderByDescending(t => t.Value.OPR).ToDictionary(t => t.Key, t => t.Value);
            else if (sort.ToLower() == "dpr")
                result = teamData.OrderBy(t => t.Value.DPR).ToDictionary(t => t.Key, t => t.Value);
            else if (sort.ToLower() == "ccwm")
                result = teamData.OrderByDescending(t => t.Value.CCWM).ToDictionary(t => t.Key, t => t.Value);
            else if (sort.ToLower() == "rank")
                result = teamData.OrderBy(t => t.Value.Rank).ToDictionary(t => t.Key, t => t.Value);
            else if (sort.ToLower() == "pts")
                result = teamData.OrderByDescending(t => (t.Value.CargoAutoHigh * 4 + t.Value.CargoAutoLow * 2 + t.Value.CargoTeleopHigh * 2 + t.Value.CargoTeleopLow)).ToDictionary(t => t.Key, t => t.Value);
            else
                result = teamData.OrderByDescending(t => t.Value.CCWM).ThenByDescending(t => (t.Value.CargoAutoHigh * 4 + t.Value.CargoAutoLow * 2 + t.Value.CargoTeleopHigh * 2 + t.Value.CargoTeleopLow)).ToDictionary(t => t.Key, t => t.Value);

            return View(result);
        }

        private static Dictionary<int, SelectionTeamData> GatherTeamData(string tbaEventCode, List<TBAMatchData> quals)
        {
            Dictionary<int, SelectionTeamData> teamData = new Dictionary<int, SelectionTeamData>();

            foreach (TBAMatchData match in quals)
            {
                Scoring scoring = match.TransformScoring();

                foreach (string team in scoring.teams)
                {
                    int teamNumber = Int32.Parse(team.Substring(3));
                    
                    SelectionTeamData data;
                    if (!teamData.TryGetValue(teamNumber, out data))
                    {
                        data = new SelectionTeamData() { EventCode = tbaEventCode, TeamNumber = teamNumber, TeamName = FRCEventsAPI.TeamListingCache[teamNumber].nameShort };
                        teamData[teamNumber] = data;
                    }

                    data.AutoTaxi += (scoring.taxi[team] == "Yes" ? 1 : 0);
                    data.ClimbLow += (scoring.endgame[team] == "Low" ? 1 : 0);
                    data.ClimbMid += (scoring.endgame[team] == "Mid" ? 1 : 0);
                    data.ClimbHigh += (scoring.endgame[team] == "High" ? 1 : 0);
                    data.ClimbTraversal += (scoring.endgame[team] == "Traversal" ? 1 : 0);

                    data.DQCount += (match.alliances.blue.dq_team_keys.Contains(team) || match.alliances.red.dq_team_keys.Contains(team) ? 1 : 0);

                    data.TBAMatchCount++;
                }
            }

            return teamData;

            #region originalcode
            //foreach (TBAMatchData match in quals)
            //{
            //    SelectionTeamData teamBlue1;
            //    int teamNumberBlue1 = Int32.Parse(match.alliances.blue.team_keys[0].Substring(3));

            //    if (teamData.ContainsKey(teamNumberBlue1))
            //        teamBlue1 = teamData[teamNumberBlue1];
            //    else
            //    {
            //        teamBlue1 = new SelectionTeamData() { TeamNumber = teamNumberBlue1, TeamName = FRCEventsAPI.TeamListingCache[teamNumberBlue1].nameShort };
            //        teamData[teamNumberBlue1] = teamBlue1;
            //    }

            //    teamBlue1.DQCount += (match.alliances.blue.dq_team_keys.Contains("frc" + teamNumberBlue1) ? 1 : 0);

            //    teamBlue1.TBAMatchCount++;
            //    teamBlue1.AutoTaxi += (match.score_breakdown.blue.taxiRobot1 == "Yes" ? 1 : 0);
            //    teamBlue1.ClimbLow += (match.score_breakdown.blue.endgameRobot1 == "Low" ? 1 : 0);
            //    teamBlue1.ClimbMid += (match.score_breakdown.blue.endgameRobot1 == "Mid" ? 1 : 0);
            //    teamBlue1.ClimbHigh += (match.score_breakdown.blue.endgameRobot1 == "High" ? 1 : 0);
            //    teamBlue1.ClimbTraversal += (match.score_breakdown.blue.endgameRobot1 == "Traversal" ? 1 : 0);

            //    SelectionTeamData teamBlue2;
            //    int teamNumberBlue2 = Int32.Parse(match.alliances.blue.team_keys[1].Substring(3));

            //    if (teamData.ContainsKey(teamNumberBlue2))
            //        teamBlue2 = teamData[teamNumberBlue2];
            //    else
            //    {
            //        teamBlue2 = new SelectionTeamData() { TeamNumber = teamNumberBlue2, TeamName = FRCEventsAPI.TeamListingCache[teamNumberBlue2].nameShort };
            //        teamData[teamNumberBlue2] = teamBlue2;
            //    }

            //    teamBlue2.DQCount += (match.alliances.blue.dq_team_keys.Contains("frc" + teamNumberBlue2) ? 1 : 0);

            //    teamBlue2.TBAMatchCount++;
            //    teamBlue2.AutoTaxi += (match.score_breakdown.blue.taxiRobot2 == "Yes" ? 1 : 0);
            //    teamBlue2.ClimbLow += (match.score_breakdown.blue.endgameRobot2 == "Low" ? 1 : 0);
            //    teamBlue2.ClimbMid += (match.score_breakdown.blue.endgameRobot2 == "Mid" ? 1 : 0);
            //    teamBlue2.ClimbHigh += (match.score_breakdown.blue.endgameRobot2 == "High" ? 1 : 0);
            //    teamBlue2.ClimbTraversal += (match.score_breakdown.blue.endgameRobot2 == "Traversal" ? 1 : 0);

            //    SelectionTeamData teamBlue3;
            //    int teamNumberBlue3 = Int32.Parse(match.alliances.blue.team_keys[2].Substring(3));

            //    if (teamData.ContainsKey(teamNumberBlue3))
            //        teamBlue3 = teamData[teamNumberBlue3];
            //    else
            //    {
            //        teamBlue3 = new SelectionTeamData() { TeamNumber = teamNumberBlue3, TeamName = FRCEventsAPI.TeamListingCache[teamNumberBlue3].nameShort };
            //        teamData[teamNumberBlue3] = teamBlue3;
            //    }

            //    teamBlue3.DQCount += (match.alliances.blue.dq_team_keys.Contains("frc" + teamNumberBlue3) ? 1 : 0);

            //    teamBlue3.TBAMatchCount++;
            //    teamBlue3.AutoTaxi += (match.score_breakdown.blue.taxiRobot3 == "Yes" ? 1 : 0);
            //    teamBlue3.ClimbLow += (match.score_breakdown.blue.endgameRobot3 == "Low" ? 1 : 0);
            //    teamBlue3.ClimbMid += (match.score_breakdown.blue.endgameRobot3 == "Mid" ? 1 : 0);
            //    teamBlue3.ClimbHigh += (match.score_breakdown.blue.endgameRobot3 == "High" ? 1 : 0);
            //    teamBlue3.ClimbTraversal += (match.score_breakdown.blue.endgameRobot3 == "Traversal" ? 1 : 0);

            //    SelectionTeamData teamRed1;
            //    int teamNumberRed1 = Int32.Parse(match.alliances.red.team_keys[0].Substring(3));

            //    if (teamData.ContainsKey(teamNumberRed1))
            //        teamRed1 = teamData[teamNumberRed1];
            //    else
            //    {
            //        teamRed1 = new SelectionTeamData() { TeamNumber = teamNumberRed1, TeamName = FRCEventsAPI.TeamListingCache[teamNumberRed1].nameShort };
            //        teamData[teamNumberRed1] = teamRed1;
            //    }

            //    teamRed1.DQCount += (match.alliances.red.dq_team_keys.Contains("frc" + teamNumberRed1) ? 1 : 0);

            //    teamRed1.TBAMatchCount++;
            //    teamRed1.AutoTaxi += (match.score_breakdown.red.taxiRobot1 == "Yes" ? 1 : 0);
            //    teamRed1.ClimbLow += (match.score_breakdown.red.endgameRobot1 == "Low" ? 1 : 0);
            //    teamRed1.ClimbMid += (match.score_breakdown.red.endgameRobot1 == "Mid" ? 1 : 0);
            //    teamRed1.ClimbHigh += (match.score_breakdown.red.endgameRobot1 == "High" ? 1 : 0);
            //    teamRed1.ClimbTraversal += (match.score_breakdown.red.endgameRobot1 == "Traversal" ? 1 : 0);

            //    SelectionTeamData teamRed2;
            //    int teamNumberRed2 = Int32.Parse(match.alliances.red.team_keys[1].Substring(3));

            //    if (teamData.ContainsKey(teamNumberRed2))
            //        teamRed2 = teamData[teamNumberRed2];
            //    else
            //    {
            //        teamRed2 = new SelectionTeamData() { TeamNumber = teamNumberRed2, TeamName = FRCEventsAPI.TeamListingCache[teamNumberRed2].nameShort };
            //        teamData[teamNumberRed2] = teamRed2;
            //    }

            //    teamRed2.DQCount += (match.alliances.red.dq_team_keys.Contains("frc" + teamNumberRed2) ? 1 : 0);

            //    teamRed2.TBAMatchCount++;
            //    teamRed2.AutoTaxi += (match.score_breakdown.red.taxiRobot2 == "Yes" ? 1 : 0);
            //    teamRed2.ClimbLow += (match.score_breakdown.red.endgameRobot2 == "Low" ? 1 : 0);
            //    teamRed2.ClimbMid += (match.score_breakdown.red.endgameRobot2 == "Mid" ? 1 : 0);
            //    teamRed2.ClimbHigh += (match.score_breakdown.red.endgameRobot2 == "High" ? 1 : 0);
            //    teamRed2.ClimbTraversal += (match.score_breakdown.red.endgameRobot2 == "Traversal" ? 1 : 0);

            //    SelectionTeamData teamRed3;
            //    int teamNumberRed3 = Int32.Parse(match.alliances.red.team_keys[2].Substring(3));

            //    if (teamData.ContainsKey(teamNumberRed3))
            //        teamRed3 = teamData[teamNumberRed3];
            //    else
            //    {
            //        teamRed3 = new SelectionTeamData() { TeamNumber = teamNumberRed3, TeamName = FRCEventsAPI.TeamListingCache[teamNumberRed3].nameShort };
            //        teamData[teamNumberRed3] = teamRed3;
            //    }

            //    teamRed3.DQCount += (match.alliances.red.dq_team_keys.Contains("frc" + teamNumberRed3) ? 1 : 0);

            //    teamRed3.TBAMatchCount++;
            //    teamRed3.AutoTaxi += (match.score_breakdown.red.taxiRobot3 == "Yes" ? 1 : 0);
            //    teamRed3.ClimbLow += (match.score_breakdown.red.endgameRobot3 == "Low" ? 1 : 0);
            //    teamRed3.ClimbMid += (match.score_breakdown.red.endgameRobot3 == "Mid" ? 1 : 0);
            //    teamRed3.ClimbHigh += (match.score_breakdown.red.endgameRobot3 == "High" ? 1 : 0);
            //    teamRed3.ClimbTraversal += (match.score_breakdown.red.endgameRobot3 == "Traversal" ? 1 : 0);
            //}
            #endregion
        }

        private int[] GetCargoCounts(int team)
        {
            DataTable table;
            using (GenericParserAdapter parser = new GenericParserAdapter(Path.Combine(_appDataFolder, "data.csv")))
            {
                parser.ColumnDelimiter = ',';
                parser.FirstRowSetsExpectedColumnCount = true;
                parser.FirstRowHasHeader = false;

                table = parser.GetDataTable();
            }

            var data = table.AsEnumerable().Where(r => ((string)r[1] == team.ToString()));
            int autohigh = data.Where(r => (r[3].Equals("auto") && r[4].Equals("high"))).Sum(r => Int32.Parse((string)r[5]));
            int autolow = data.Where(r => (r[3].Equals("auto") && r[4].Equals("low"))).Sum(r => Int32.Parse((string)r[5]));
            int teleophigh = data.Where(r => (r[3].Equals("teleop") && r[4].Equals("high"))).Sum(r => Int32.Parse((string)r[5]));
            int teleoplow = data.Where(r => (r[3].Equals("teleop") && r[4].Equals("low"))).Sum(r => Int32.Parse((string)r[5]));

            int matches = data.Select(r => r[2]).Distinct().Count();

            return new int[] { autohigh, autolow, teleophigh, teleoplow, matches };
        }

        private Dictionary<int, int[]> GetCargoCounts()
        {
            if (System.IO.File.Exists(Path.Combine(_appDataFolder, "data.csv")))
            {
                DataTable table;
                using (GenericParserAdapter parser = new GenericParserAdapter(Path.Combine(_appDataFolder, "data.csv")))
                {
                    parser.ColumnDelimiter = ',';
                    parser.FirstRowSetsExpectedColumnCount = true;
                    parser.FirstRowHasHeader = false;

                    table = parser.GetDataTable();
                }

                Dictionary<string, int[]> serverCounts = new Dictionary<string, int[]>();
                Dictionary<string, int[]> clientCounts = new Dictionary<string, int[]>();

                List<string> teamNumbers = table.AsEnumerable().Select(r => (string)r[1]).Distinct().ToList();
                List<string> teamsAndMatches = table.AsEnumerable().Select(r => (string)r[1] + "~" + (string)r[2]).Distinct().ToList();
                foreach (string teamAndMatch in teamsAndMatches)
                {
                    string[] teamAndMatchArr = teamAndMatch.Split('~');
                    var data = table.AsEnumerable().Where(r => ((string)r[1] == teamAndMatchArr[0] && (string)r[2] == teamAndMatchArr[1]));

                    int autohigh = data.Where(r => (r[3].Equals("auto") && r[4].Equals("high"))).Sum(r => Int32.Parse((string)r[5]));
                    int autolow = data.Where(r => (r[3].Equals("auto") && r[4].Equals("low"))).Sum(r => Int32.Parse((string)r[5]));
                    int teleophigh = data.Where(r => (r[3].Equals("teleop") && r[4].Equals("high"))).Sum(r => Int32.Parse((string)r[5]));
                    int teleoplow = data.Where(r => (r[3].Equals("teleop") && r[4].Equals("low"))).Sum(r => Int32.Parse((string)r[5]));

                    serverCounts[teamAndMatch] = new int[] { autohigh, autolow, teleophigh, teleoplow };

                    autohigh = data.Where(r => (r[3].Equals("auto") && r[4].Equals("high"))).Max(r => Int32.Parse((string)r[6]) as int?) ?? 0;
                    autolow = data.Where(r => (r[3].Equals("auto") && r[4].Equals("low"))).Max(r => Int32.Parse((string)r[6]) as int?) ?? 0;
                    teleophigh = data.Where(r => (r[3].Equals("teleop") && r[4].Equals("high"))).Max(r => Int32.Parse((string)r[6]) as int?) ?? 0;
                    teleoplow = data.Where(r => (r[3].Equals("teleop") && r[4].Equals("low"))).Max(r => Int32.Parse((string)r[6]) as int?) ?? 0;

                    clientCounts[teamAndMatch] = new int[] { autohigh, autolow, teleophigh, teleoplow };
                }

                //if client count is higher, override with that
                Dictionary<int, int[]> totalCounts = new Dictionary<int, int[]>();
                foreach (string teamAndMatch in serverCounts.Keys)
                {
                    int matches = 1;
                    int autohigh = Math.Max(serverCounts[teamAndMatch][0], clientCounts[teamAndMatch][0]);
                    int autolow = Math.Max(serverCounts[teamAndMatch][1], clientCounts[teamAndMatch][1]);
                    int teleophigh = Math.Max(serverCounts[teamAndMatch][2], clientCounts[teamAndMatch][2]);
                    int teleoplow = Math.Max(serverCounts[teamAndMatch][3], clientCounts[teamAndMatch][3]);

                    int team = Int32.Parse(teamAndMatch.Split('~')[0]);
                    if (totalCounts.ContainsKey(team))
                    {
                        int[] counts = totalCounts[team];
                        autohigh += counts[0];
                        autolow += counts[1];
                        teleophigh += counts[2];
                        teleoplow += counts[3];
                        matches += counts[4];
                    }
                    totalCounts[team] = new int[] { autohigh, autolow, teleophigh, teleoplow, matches };
                }

                return totalCounts;
            }

            return null;
        }

        public int Rank(string eventCode, int teamNumber)
        {
            List<EventRanking> eventRankings = FRCEventsAPI.GetEventRankings(eventCode, teamNumber);
            return eventRankings[0].rank;
        }

        private void TEMP_GenerateSampleDataFile()
        {
            int[,] t = new int[,] {
                {5414,1},{418,1},{7418,1},{8177,1},{5261,1},{5923,1},{8370,2},{118,2},{7115,2},{5829,2},{4597,2},{4332,2},{1255,3},{231,3},{5427,3},{7616,3},{2882,3},{5892,3},{324,4},{6645,4},{5894,4},{8515,4},{4639,4},{4587,4},{7492,5},{4295,5},{8576,5},{7312,5},{5682,5},{3035,5},{118,6},{8150,6},{7418,6},{8210,6},{5829,6},{5427,6},{5261,7},{2882,7},{324,7},{8515,7},{5894,7},{7616,7},{1255,8},{5892,8},{8370,8},{5682,8},{4295,8},{6645,8},{3035,9},{4639,9},{4597,9},{4332,9},{8576,9},{4587,9},{7115,10},{8177,10},{231,10},{418,10},{7312,10},{7492,10},{8150,11},{8210,11},{8515,11},{5414,11},{5923,11},{4295,11},{118,12},{7616,12},{324,12},{8576,12},{7418,12},{3035,12},{8177,13},{5829,13},{418,13},{4597,13},{5427,13},{7492,13},{6645,14},{4587,14},{7115,14},{2882,14},{231,14},{5414,14},{5261,15},{4332,15},{7312,15},{5682,15},{8150,15},{5892,15},{5923,16},{8370,16},{8210,16},{5894,16},{1255,16},{4639,16},{5427,17},{7115,17},{7418,17},{3035,17},{4587,17},{418,17},{4332,18},{4295,18},{118,18},{7616,18},{5682,18},{4597,18},{5894,19},{7492,19},{5261,19},{7312,19},{8370,19},{8150,19},{4639,20},{8210,20},{5414,20},{6645,20},{8515,20},{8177,20},{8576,21},{231,21},{5892,21},{324,21},{5923,21},{5829,21},{2882,22},{7492,22},{7418,22},{1255,22},{4597,22},{118,22},{8177,23},{5894,23},{5682,23},{8370,23},{6645,23},{7616,23},{8150,24},{5427,24},{5923,24},{3035,24},{324,24},{7115,24},{5414,25},{1255,25},{4332,25},{231,25},{7312,25},{8210,25},{418,26},{2882,26},{8576,26},{8515,26},{5892,26},{4295,26},{4587,27},{5829,27},{7492,27},{4639,27},{7616,27},{5261,27},{231,28},{4332,28},{5923,28},{7418,28},{8177,28},{8210,28},{5892,29},{418,29},{6645,29},{4295,29},{8150,29},{1255,29},{5427,30},{3035,30},{8370,30},{8576,30},{8515,30},{5261,30},{4639,31},{324,31},{5682,31},{5829,31},{7115,31},{5414,31},{4587,32},{4597,32},{5894,32},{7312,32},{118,32},{2882,32},
                {3035,33},{5892,33},{4332,33},{8370,33},{7492,33},{231,33},{8210,34},{5261,34},{418,34},{7616,34},{8576,34},{5829,34},{4295,35},{5427,35},{5894,35},{5923,35},{6645,35},{118,35},{4597,36},{5414,36},{8150,36},{7115,36},{2882,36},{8515,36},{4639,37},{7312,37},{8177,37},{7418,37},{324,37},{1255,37},{4587,38},{118,38},{231,38},{418,38},{5923,38},{5682,38},{4597,39},{8370,39},{8515,39},{7492,39},{8150,39},{7616,39},{7418,40},{4639,40},{5829,40},{5892,40},{5414,40},{5894,40},{7312,41},{7115,41},{4295,41},{5261,41},{3035,41},{1255,41},{5427,42},{5682,42},{4587,42},{324,42},{8210,42},{8576,42},{2882,43},{6645,43},{8150,43},{8177,43},{4332,43},{7492,43},{7115,44},{418,44},{4597,44},{5892,44},{5923,44},{4639,44},{4587,45},{7312,45},{324,45},{8370,45},{7418,45},{4295,45},{5414,46},{8177,46},{7616,46},{5894,46},{118,46},{8576,46},{5682,47},{8210,47},{1255,47},{231,47},{3035,47},{8515,47},{6645,48},{5261,48},{5829,48},{5427,48},{4332,48},{2882,48},{5923,49},{7616,49},{7312,49},{7115,49},{4639,49},{8150,49},{5682,50},{8515,50},{5414,50},{231,50},{7418,50},{4597,50},{118,51},{5892,51},{8177,51},{5261,51},{4587,51},{8370,51},{5829,52},{5894,52},{2882,52},{8210,52},{4295,52},{3035,52},{7492,53},{1255,53},{6645,53},{324,53},{418,53},{5427,53},{8576,54},{8177,54},{8370,54},{4332,54},{7418,54},{5682,54},{5923,55},{3035,55},{2882,55},{5892,55},{5829,55},{7312,55},{4295,56},{231,56},{4639,56},{118,56},{5427,56},{5261,56},{7616,57},{1255,57},{4587,57},{8150,57},{5894,57},{418,57},{8515,58},{7492,58},{324,58},{4332,58},{7115,58},{8210,58},{4597,59},{8576,59},{7312,59},{6645,59},{5414,59},{5427,59},{418,60},{7616,60},{231,60},{2882,60},{8370,60},{4639,60},{1255,61},{8515,61},{5923,61},{5829,61},{5682,61},{118,61},{7492,62},{5414,62},{3035,62},{4295,62},{4587,62},{8177,62},{7418,63},{5261,63},{5892,63},{8210,63},{4597,63},{6645,63},{8150,64},{324,64},{4332,64},{5894,64},{8576,64},{7115,64}
            };
            string path = Path.Combine(_appDataFolder, "data2.csv");
            for (int i = 0; i < t.Length / 2; i++)
            {
                int team = t[i, 0];
                int match = t[i, 1];

                Random rnd = new Random();
                int ah = rnd.Next(0, 2);
                int al = rnd.Next(0, 2);
                int th = rnd.Next(0, 10);
                int tl = rnd.Next(0, 10);

                if (ah > 0) tl = 0;
                if (th > 0) tl = tl / 4;

                for (int j = 0; j < ah; j++)
                {
                    string data = $"{DateTime.Now:G},{team},{match},auto,high,1,{ah},,,\r\n";
                    System.IO.File.AppendAllText(path, data);
                }

                for (int j = 0; j < al; j++)
                {
                    string data = $"{DateTime.Now:G},{team},{match},auto,low,1,{al},,,\r\n";
                    System.IO.File.AppendAllText(path, data);
                }

                for (int j = 0; j < th; j++)
                {
                    string data = $"{DateTime.Now:G},{team},{match},teleop,high,1,{th},,,\r\n";
                    System.IO.File.AppendAllText(path, data);
                }

                for (int j = 0; j < tl; j++)
                {
                    string data = $"{DateTime.Now:G},{team},{match},teleop,low,1,{tl},,,\r\n";
                    System.IO.File.AppendAllText(path, data);
                }
            }
        }
    }
}

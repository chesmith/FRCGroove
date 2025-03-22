using FRCGroove.Web.Models;
using FRCGroove.Lib;
using FRCGroove.Lib.Models.TBAv3;
using FRCGroove.Lib.Models.Groove;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FRCGroove.Web.Controllers
{
    public class FRCEventController : Controller
    {
        private static Random rnd = new Random();

        public ActionResult Index(string eventCode = "", string teamList = "")
        {
            //TODO: I thought there was no need for this anymore, but removing this and the teamList param broke retention of watched teams
            List<string> teams = BuildTeamsOfInterest(teamList);

            Dashboard dashboard = BuildEventDashboard(eventCode, teams);

            this.ControllerContext.HttpContext.Response.Cookies.Add(new HttpCookie("teamList") { Value = string.Join(",", teams), Expires = new DateTime(DateTime.Now.Year + 1, DateTime.Now.Month, DateTime.Now.Day) });
            this.ControllerContext.HttpContext.Response.Cookies.Add(new HttpCookie("eventCode") { Value = eventCode, Expires = DateTime.Now.AddYears(1) });

            return View(dashboard);
        }

        /// <summary>
        /// Builds a List of team numbers (as strings) as pulled from the querystring and "teamList" cookie, updated with any removals ("x" in team number)
        /// </summary>
        /// <param name="teamList">Comma-separated list of team numbers pulled from the querystring</param>
        /// <returns>List of team numbers as strings</returns>
        private List<string> BuildTeamsOfInterest(string teamList = "")
        {
            string[] teamsFromQuerystring = teamList.Split(',');
            List<string> teams = new List<string>(teamsFromQuerystring);
            if (this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("teamList"))
            {
                string teamsFromCookie = this.ControllerContext.HttpContext.Request.Cookies["teamList"].Value;
                if (teamsFromCookie.Length > 0)
                {
                    string[] additionalTeams = teamsFromCookie.Split(',');
                    teams.AddRange(additionalTeams);
                }
            }

            List<string> teamsToRemove = teams.Where(t => t.IndexOf("x") == 0).Select(t => t.Substring(1)).ToList();
            List<string> teamsToKeep = teams.Where(t => t.Length > 0 && t.IndexOf("x") < 0 && !teamsToRemove.Contains(t)).ToList();

            return teamsToKeep.Distinct().OrderBy(t => Int32.Parse(t)).ToList();
        }

        private Dashboard BuildEventDashboard(string eventKey, List<string> teamsOfInterest)
        {
            Dashboard dashboard = new Dashboard();

            // TODO: This was due to FRC referring to 2023 TRI as "TXHO2" while TBA used "txtri" - do this sort of translation elsewhere (low priority as we're now mainly depending on TBA)
            //if (eventKey == "2023txho2") eventKey = "2023txri";

            if (eventKey.Length > 0)
            {
                dashboard.Event = Groove.GetEvent(eventKey);
                if (dashboard.Event != null)
                {
                    dashboard.Matches = Groove.GetMatches(eventKey);
                    dashboard.Matches = dashboard.Matches?.OrderBy(m => m.sortTitle).ToList();

                    dashboard.EventState = DetermineEventState(dashboard.Matches);

                    //TODO: need to rework brackets after move to double elim
                    if (dashboard.Matches != null && (dashboard.EventState == EventState.Past || dashboard.EventState == EventState.Qualifications || dashboard.EventState == EventState.Playoffs || dashboard.EventState == EventState.Finals))
                    {
                        if (dashboard.EventState != EventState.Qualifications)
                        {
                            //dashboard.TBAPlayoffAlliances = TBAAPIv3.GetPlayoffAlliances(eventKey); //API CALL (5 min cache)
                            //dashboard.TBABracket = new PlayoffBracket(dashboard.Alliances, dashboard.Matches);
                        }

                        if (dashboard.EventState != EventState.Past)
                        {
                            List<GrooveMatch> matchesForOffsetCalc = dashboard.Matches;
                            if (dashboard.EventState != EventState.Qualifications)
                                matchesForOffsetCalc = dashboard.Matches.Where(m => m.competitionLevel != "Qualification").ToList();
                            dashboard.ScheduleOffset = CalculateScheduleOffset(matchesForOffsetCalc);
                        }
                    }

                    if (dashboard.EventState != EventState.Invalid)
                    {
                        List<GrooveEventRanking> rankings = Groove.GetEventRankings(eventKey);
                        if(rankings != null)
                            dashboard.EventRankings = rankings.ToDictionary(r => r.teamNumber, r => r);
                    }
                }
            }

            //TODO: I don't think we need to send this in the model, maybe just a count - otherwise we're doing a 2nd call to GatherTeamsOfInterest from javascript later
            dashboard.TeamsOfInterest.AddRange(GatherTeamsOfInterest(eventKey, teamsOfInterest, dashboard.EventRankings));

            return dashboard;
        }

        private EventState DetermineEventState(List<GrooveMatch> matches)
        {
            EventState eventState = EventState.Invalid;
            if (matches != null)
            {
                if (matches.Count() <= 1)
                    eventState = EventState.Future;
                else
                {
                    List<GrooveMatch> finals = matches.Where(m =>
                        m.competitionLevel == "Final"
                        && m.alliances["red"].teamKeys.Count() > 0
                        && m.alliances["blue"].teamKeys.Count() > 0).ToList();
                    if (finals.Exists(t => t.alliances["red"].score > 0 || t.alliances["blue"].score > 0))
                    {
                        bool redWin = (finals.Count(t => t.alliances["red"].score > t.alliances["blue"].score) == 2);
                        bool blueWin = (finals.Count(t => t.alliances["red"].score < t.alliances["blue"].score) == 2);
                        if (redWin || blueWin)
                            eventState = EventState.Past;
                        else
                            eventState = EventState.Finals;
                    }
                    else
                    {
                        List<GrooveMatch> playoffs = matches.Where(m =>
                            m.competitionLevel == "Playoff"
                            && m.alliances["red"].teamKeys.Count() > 0
                            && m.alliances["blue"].teamKeys.Count() > 0).ToList();
                        if (playoffs.Exists(t => t.alliances["red"].score > 0 || t.alliances["blue"].score > 0))
                        {
                            eventState = EventState.Playoffs;
                        }
                        else
                        {
                            eventState = EventState.Qualifications;
                        }
                    }
                }
            }

            return eventState;
        }

        private List<GrooveTeam> GatherTeamsOfInterest(string eventCode, List<string> teamList, Dictionary<int, GrooveEventRanking> eventRankings = null)
        {
            List<GrooveTeam> teamsOfInterest = new List<GrooveTeam>();
            if (teamList != null && teamList.Count > 0)
            {
                if (eventRankings == null)
                {
                    List<GrooveEventRanking> rankings = Groove.GetEventRankings(eventCode);
                    if (rankings != null)
                        eventRankings = rankings.ToDictionary(r => r.teamNumber, r => r);
                }

                TBAStatsCollection stats = TBAAPIv3.GetStats(eventCode); //API CALL (5 min cache)
                foreach (string teamNumber in teamList)
                {
                    GrooveTeam team = Groove.GetTeam(Int32.Parse(teamNumber));
                    if (team != null)
                    {
                        if (stats != null && stats.oprs != null && stats.oprs.ContainsKey("frc" + team.number))
                            team.Stats = new TBAStats(stats, team.number);
                        else
                            team.Stats = null;

                        if (eventRankings != null && eventRankings.Count > 0 && eventRankings.ContainsKey(team.number))
                            team.eventRank = eventRankings[team.number].rank;
                        else
                            team.eventRank = -1;

                        teamsOfInterest.Add(team);
                    }
                }
            }
            return teamsOfInterest;
        }

        private double CalculateScheduleOffset(List<GrooveMatch> matches)
        {
            DateTime today = DateTime.Now.Date;
            var allOfTodaysMatches = matches.Where(m => m.timeScheduled.Date == today);
            var todaysFinishedMatches = allOfTodaysMatches.Where(m => m.hasStarted);
            double average = 0.0;
            if (todaysFinishedMatches.Count() < allOfTodaysMatches.Count())
            {
                List<GrooveMatch> last3Matches = todaysFinishedMatches.Reverse().Take(3).ToList();
                double sum = last3Matches.Select(m => (m.timeActual - m.timeScheduled).TotalMinutes).Sum();
                if (Math.Abs(sum) > 0)
                    average = sum / last3Matches.Count();
            }
            return average;
        }

        [HttpPost]
        public JsonResult TeamsOfInterestAjax(string eventCode, string teamList, string sortName, string sortDirection)
        {
            if (eventCode.Length > 0)
            {
                List<string> teams = BuildTeamsOfInterest(teamList);

                this.ControllerContext.HttpContext.Response.Cookies.Add(new HttpCookie("teamList") { Value = string.Join(",", teams), Expires = new DateTime(DateTime.Now.Year + 1, DateTime.Now.Month, DateTime.Now.Day) });

                List<GrooveTeam> teamsOfInterest = GatherTeamsOfInterest(eventCode, teams);
                teamsOfInterest = teamsOfInterest.Where(t => t.Stats != null).ToList();

                if (sortName.Length == 0) sortName = "Rank";

                if (sortName == "Number")
                    return (sortDirection == "ASC" ? Json(teamsOfInterest.OrderBy(t => t.number).ToList()) : Json(teamsOfInterest.OrderByDescending(t => t.number).ToList()));
                else if (sortName == "Name")
                    return (sortDirection == "ASC" ? Json(teamsOfInterest.OrderBy(t => t.name).ToList()) : Json(teamsOfInterest.OrderByDescending(t => t.name).ToList()));
                else if (sortName == "Rank")
                    return (sortDirection == "ASC" ? Json(teamsOfInterest.OrderBy(t => t.eventRank).ToList()) : Json(teamsOfInterest.OrderByDescending(t => t.eventRank).ToList()));
                else if (sortName == "OPR")
                    return (sortDirection == "ASC" ? Json(teamsOfInterest.OrderBy(t => t.Stats.OPR).ToList()) : Json(teamsOfInterest.OrderByDescending(t => t.Stats.OPR).ToList()));
                else if (sortName == "DPR")
                    return (sortDirection == "ASC" ? Json(teamsOfInterest.OrderBy(t => t.Stats.DPR).ToList()) : Json(teamsOfInterest.OrderByDescending(t => t.Stats.DPR).ToList()));
                else if (sortName == "CCWM")
                    return (sortDirection == "ASC" ? Json(teamsOfInterest.OrderBy(t => t.Stats.CCWM).ToList()) : Json(teamsOfInterest.OrderByDescending(t => t.Stats.CCWM).ToList()));
                else
                    return null;
            }
            else
                return null;
        }

        public JsonResult GetDashboardData(string eventCode)
        {
            //TODO: getting the full dashboard may be overkill
            Dashboard dashboard = BuildEventDashboard(eventCode, new List<string>());

            if (dashboard.EventState != EventState.Invalid)
            {
                List<GrooveEventRanking> rankings = Groove.GetEventRankings(eventCode);
                if (rankings != null)
                    dashboard.EventRankings = rankings.ToDictionary(r => r.teamNumber, r => r);
            }

            return Json(new { EventState = dashboard.EventState.ToString(), Matches = dashboard.Matches, ScheduleOffset = dashboard.ScheduleOffset, EventRankings = dashboard.EventRankings.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value) }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Pears(string eventCode, string teamNumber = "5414")
        {
            List<GrooveMatch> matches = Groove.GetMatches(eventCode);
            List<string> teams = matches
                .SelectMany(m => m.alliances.Values.SelectMany(a => a.teamKeys))
                .Distinct().OrderBy(v => v)
                .ToList();

            Dictionary<string, string> pears = new Dictionary<string, string>();
            foreach (string team in teams)
            {
                List<GrooveMatch> matchingMatches = matches
                    .Where(match => match.alliances.Any(alliance =>
                    alliance.Value.teamKeys.Contains(team) && alliance.Value.teamKeys.Contains($"frc{teamNumber}")))
                .ToList();

                pears.Add(team.Substring(3), String.Join(",", matchingMatches.Where(m => m.competitionLevel == "Qualification").Select(m => m.matchNumber)));
            }

            var firstEvents = new Dictionary<int, dynamic>();
            List<GrooveTeam> eventTeams = Groove.GetEventTeams(eventCode);
            int year = Int32.Parse(eventCode.Substring(0, 4));
            foreach (GrooveTeam team in eventTeams)
            {
                // for every team in this event, get the list of their events this season
                List<GrooveEvent> events = Groove.GetTeamEvents(team.number, year);
                // sort that list by date and get their first event
                var firstEvent = events.OrderBy(e => e.dateStart).FirstOrDefault();
                // determine if this is their first event
                bool isFirstEvent = firstEvent.key == eventCode;
                // if this isn't their first event, for that first event, get (a) their rank and total number of teams, (b) their alliance and pick number (if chosen), and (c) how far they made it in playoffs
                if (!isFirstEvent)
                {
                    var status = Groove.GetTeamEventStatus(team.number, firstEvent.key);
                    if (status != null)
                    {
                        if (status.qual != null)
                        {
                            var rank = status.qual.ranking.rank;
                            var totalTeams = status.qual.num_teams;
                            var alliance = status.alliance?.number;
                            var pick = status.alliance?.pick;
                            var playoffLevel = status.playoff?.double_elim_round.Replace("Round", "Rnd");
                            var won = (status.playoff?.status == "won");
                            firstEvents.Add(team.number, new TeamFirstEvent { Name = Groove.TeamListingCache[team.number].name, FirstEvent = firstEvent.key, Rank = rank, TotalTeams = totalTeams, Alliance = alliance, Pick = pick, PlayoffLevel = playoffLevel, Won = won });
                        }
                        else
                            firstEvents.Add(team.number, new TeamFirstEvent { Name = Groove.TeamListingCache[team.number].name, FirstEvent = firstEvent.key });
                    }
                }
                else
                {
                    firstEvents.Add(team.number, new TeamFirstEvent { Name = Groove.TeamListingCache[team.number].name, FirstEvent = firstEvent.key });
                }
            }

            ViewBag.FirstEvents = firstEvents;
            ViewBag.TeamNumber = teamNumber;
            return View(pears);
        }
    }

    public class TeamFirstEvent
    {
        public string Name { get; set; }
        public string FirstEvent { get; set; }
        public int Rank { get; set; }
        public int TotalTeams { get; set; }
        public int? Alliance { get; set; }
        public int? Pick { get; set; }
        public string PlayoffLevel { get; set; }
        public bool Won { get; set; }
    }
}
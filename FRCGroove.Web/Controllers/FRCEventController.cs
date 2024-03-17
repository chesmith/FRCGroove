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

        public ActionResult Index(string districtCode = "", string eventCode = "", string teamList = "")
        {
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
        private List<string> BuildTeamsOfInterest(string teamList)
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

        //private Dashboard BuildEventDashboard(string districtCode, string eventCode, List<string> teamsOfInterest)
        //{
        //    Dashboard dashboard = new Dashboard();

        //    dashboard.districtCode = districtCode;

        //    if (eventCode.Length > 0)
        //    {
        //        dashboard.FrcEvent = FRCEventsAPI.GetEvent(eventCode);
        //        if (dashboard.FrcEvent != null)
        //        {
        //            dashboard.Matches = FRCEventsAPI.GetFullHybridSchedule(eventCode);

        //            if (dashboard.Matches != null && (dashboard.EventState == FRCEventState.Past || dashboard.EventState == FRCEventState.Qualifications || dashboard.EventState == FRCEventState.Quarterfinals || dashboard.EventState == FRCEventState.Semifinals || dashboard.EventState == FRCEventState.Finals))
        //            {
        //                if (dashboard.EventState != FRCEventState.Qualifications)
        //                {
        //                    dashboard.Alliances = FRCEventsAPI.GetPlayoffAlliances(eventCode);
        //                    dashboard.Bracket = new PlayoffBracket(dashboard.Alliances, dashboard.Matches);
        //                }

        //                if (dashboard.EventState != FRCEventState.Past)
        //                {
        //                    List<Match> matchesForOffsetCalc = dashboard.Matches;
        //                    if (dashboard.EventState != FRCEventState.Qualifications)
        //                        matchesForOffsetCalc = dashboard.Matches.Where(m => m.tournamentLevel == "Playoff").ToList();
        //                    dashboard.ScheduleOffset = CalculateScheduleOffset(matchesForOffsetCalc);
        //                }
        //            }
        //        }

        //        dashboard.RegisteredTeams = FRCEventsAPI.TeamListingCache;

        //        //LEGACY TODO: Should this be up in the != null section above?
        //        if (dashboard.EventState != FRCEventState.Invalid)
        //        {
        //            List<EventRanking> eventRankings = FRCEventsAPI.GetEventRankings(eventCode);
        //            if (eventRankings != null)
        //                dashboard.EventRankings = eventRankings.ToDictionary(e => e.teamNumber, e => e);
        //        }
        //    }

        //    dashboard.TeamsOfInterest.AddRange(GatherTeamsOfInterest(eventCode, teamsOfInterest, dashboard.EventRankings));

        //    return dashboard;
        //}

        //private double CalculateScheduleOffset(List<Match> matches)
        //{
        //    DateTime today = DateTime.Now.Date;
        //    List<Match> todaysMatches = matches.Where(m => m.startTime.Date == today && m.actualStartTime != null).Reverse().Take(3).ToList();
        //    double sum = todaysMatches.Select(m => (m.actualStartTime.Value - m.startTime).TotalMinutes).Sum();
        //    double average = 0.0;
        //    if (Math.Abs(sum) > 0)
        //        average = sum / todaysMatches.Count();

        //    //if(average == 0.0)
        //    //{
        //    //    //LEGACY TODO: if the next match scores have not posted and the time for the match after that has passed, consider matches running late
        //    //    //      and report as schedule offset by that delta
        //    //    List<Match> tmatches = matches.Where(m => m.actualStartTime == null).Take(3).ToList();
        //    //    if(tmatches[1].startTime < DateTime.Now)
        //    //    {
        //    //        average = (DateTime.Now - tmatches[1].startTime).TotalMinutes;
        //    //    }
        //    //}

        //    return average;
        //}
    }
}
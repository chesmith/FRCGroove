using FRCGroove.Web.Models;
using FRCGroove.Lib;
using FRCGroove.Lib.Models;
using System.Text.RegularExpressions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;

namespace FRCGroove.Web.Controllers
{
    public class FRCEventController : Controller
    {
        private static Random rnd = new Random();

        public ActionResult Index(string districtCode = "", string eventCode = "", string teamList = "")
        {
            var stopwatch = Stopwatch.StartNew();
            List<string> teams = BuildTeamsOfInterest(teamList);

            //string tbaEventCode = TBAAPI.TranslateFRCEventCode(eventCode);
            //Dashboard dashboard = BuildEventDashboardTBA(tbaEventCode, teams);
            Dashboard dashboard = BuildEventDashboardTBA(eventCode, teams);

            this.ControllerContext.HttpContext.Response.Cookies.Add(new HttpCookie("teamList") { Value = string.Join(",", teams), Expires = DateTime.Now.AddYears(100) });
            this.ControllerContext.HttpContext.Response.Cookies.Add(new HttpCookie("eventCode") { Value = eventCode, Expires = DateTime.Now.AddYears(1) });

            //TODO: if we added teams from the cookie that weren't there in the input list, do a redirect with the full URL instead of just showing the dashboard (update the cookie first?)

            stopwatch.Stop();
            //Debug.WriteLine($"Total, {stopwatch.Elapsed.TotalMilliseconds}");

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

        private Dashboard BuildEventDashboardTBA(string tbaEventCode, List<string> teamsOfInterest)
        {
            Dashboard dashboard = new Dashboard();

            Stopwatch stopwatch;

            if (tbaEventCode == "2023txho2") tbaEventCode = "2023txri";

            if (tbaEventCode.Length > 0)
            {
                stopwatch = Stopwatch.StartNew();
                dashboard.TBAEvent = TBAAPI.GetEvent(tbaEventCode); //API CALL (6 hour cache)
                stopwatch.Stop();
                //Debug.WriteLine($"TBAAPI.GetEvent, {stopwatch.Elapsed.TotalMilliseconds}");
                if (dashboard.TBAEvent != null)
                {
                    stopwatch = Stopwatch.StartNew();
                    dashboard.TBAMatches = TBAAPI.GetMatches(tbaEventCode); //API CALL (TBA-compliant cache)
                    stopwatch.Stop();
                    //Debug.WriteLine($"TBAAPI.GetMatches, {stopwatch.Elapsed.TotalMilliseconds}");
                    dashboard.TBAMatches = dashboard.TBAMatches?.OrderBy(m => m.sortTitle).ToList();

                    dashboard.EventState = DetermineEventState(dashboard.TBAMatches);

                    //TODO: replace with TBA (I think mostly done but for brackets)
                    if (dashboard.TBAMatches != null && (dashboard.EventState == FRCEventState.Past || dashboard.EventState == FRCEventState.Qualifications || dashboard.EventState == FRCEventState.Quarterfinals || dashboard.EventState == FRCEventState.Semifinals || dashboard.EventState == FRCEventState.Finals))
                    {
                        if (dashboard.EventState != FRCEventState.Qualifications)
                        {
                            stopwatch = Stopwatch.StartNew();
                            dashboard.TBAPlayoffAlliances = TBAAPI.GetPlayoffAlliances(tbaEventCode); //API CALL (5 min cache)
                            stopwatch.Stop();
                            //Debug.WriteLine($"TBAAPI.GetPlayoffAlliances, {stopwatch.Elapsed.TotalMilliseconds}");
                            //dashboard.TBABracket = new PlayoffBracket(dashboard.Alliances, dashboard.Matches);
                        }

                        if (dashboard.EventState != FRCEventState.Past)
                        {
                            List<TBAMatchData> matchesForOffsetCalc = dashboard.TBAMatches;
                            if (dashboard.EventState != FRCEventState.Qualifications)
                                matchesForOffsetCalc = dashboard.TBAMatches.Where(m => m.comp_level != "qm").ToList();
                            dashboard.ScheduleOffset = CalculateScheduleOffsetTBA(matchesForOffsetCalc);
                        }
                    }

                    if (dashboard.EventState != FRCEventState.Invalid)
                    {
                        stopwatch = Stopwatch.StartNew();
                        TBAEventRankings rankings = TBAAPI.GetEventRankings(tbaEventCode); //API CALL (TBA-compliant cache)
                        stopwatch.Stop();
                        //Debug.WriteLine($"TBAAPI.GetEventRankings, {stopwatch.Elapsed.TotalMilliseconds}");
                        if (rankings != null && rankings.rankings != null)
                        {
                            //TODO: Int32.Parse (I think this was an issue with offseason events where teams might have a letter in their name)
                            dashboard.EventRankings = rankings.rankings.ToDictionary(e => Int32.Parse(Regex.Replace(e.team_key, "[^0-9,-]+", "")), e => e);
                        }
                    }
                }

                dashboard.RegisteredTeams = TBAAPI.TeamListingCache;
                dashboard.EPACache = TBAAPI.EPACache;
            }

            //TODO: I don't think we need to send this in the model, maybe just a count - otherwise we're doing a 2nd call to GatherTeamsOfInterest from javascript later
            stopwatch = Stopwatch.StartNew();
            dashboard.TeamsOfInterest.AddRange(GatherTeamsOfInterest(tbaEventCode, teamsOfInterest, dashboard.EventRankings));
            stopwatch.Stop();
            //Debug.WriteLine($"GatherTeamsOfInterest, {stopwatch.Elapsed.TotalMilliseconds}");

            if(tbaEventCode == "2023txbel") dashboard.ScheduleOffset = rnd.Next(-10, 10);

            return dashboard;
        }

        private FRCEventState DetermineEventState(List<TBAMatchData> matches)
        {
            FRCEventState eventState = FRCEventState.Invalid;
            if (matches != null)
            {
                if (matches.Count() <= 1)
                    eventState = FRCEventState.Future;
                else
                {
                    List<TBAMatchData> finals = matches.Where(m =>
                        m.comp_level == "f"
                        && m.alliances.red.team_keys.Count() > 0
                        && m.alliances.blue.team_keys.Count() > 0).ToList();
                    if (finals.Exists(t => t.alliances.red.score > 0 || t.alliances.blue.score > 0))
                    {
                        bool redWin = (finals.Count(t => t.alliances.red.score > t.alliances.blue.score) == 2);
                        bool blueWin = (finals.Count(t => t.alliances.red.score < t.alliances.blue.score) == 2);
                        if (redWin || blueWin)
                            eventState = FRCEventState.Past;
                        else
                            eventState = FRCEventState.Finals;
                    }
                    else
                    {
                        List<TBAMatchData> semifinals = matches.Where(m =>
                            m.comp_level == "sf"
                            && m.alliances.red.team_keys.Count() > 0
                            && m.alliances.blue.team_keys.Count() > 0).ToList();
                        if (semifinals.Exists(t => t.alliances.red.score > 0 || t.alliances.blue.score > 0))
                        {
                            eventState = FRCEventState.Semifinals;
                        }
                        else
                        {
                            eventState = FRCEventState.Qualifications;
                        }
                    }
                }
            }

            return eventState;
        }

        private List<TBATeam> GatherTeamsOfInterest(string eventCode, List<string> teamList, Dictionary<int, TBARanking> eventRankings = null)
        {
            List<TBATeam> teamsOfInterest = new List<TBATeam>();
            if (teamList != null && teamList.Count > 0)
            {
                TBAStatsCollection stats = TBAAPI.GetStats(eventCode); //API CALL (5 min cache)
                if (eventRankings == null)
                {
                    TBAEventRankings rankings = TBAAPI.GetEventRankings(eventCode); //API CALL (TBA-compliant cache)
                    if (rankings != null && rankings.rankings != null)
                    {
                        //TODO: Int32.Parse (I think this was an issue with offseason events where teams might have a letter in their team number)
                        eventRankings = rankings.rankings.ToDictionary(e => Int32.Parse(Regex.Replace(e.team_key, "[^0-9,-]+", "")), e => e);
                    }
                }

                foreach (string teamNumber in teamList)
                {
                    TBATeam team = TBAAPI.GetTeam(Int32.Parse(teamNumber)); //CACHED AT STARTUP
                    if (stats != null && stats.oprs != null && stats.oprs.ContainsKey("frc" + team.team_number))
                        team.Stats = new TBAStats(stats, team.team_number);
                    else
                        team.Stats = null;

                    if (eventRankings != null && eventRankings.Count > 0 && eventRankings.ContainsKey(team.team_number))
                        team.eventRank = eventRankings[team.team_number].rank;
                    else
                        team.eventRank = -1;

                    teamsOfInterest.Add(team);
                }
            }
            return teamsOfInterest;
        }

        private double CalculateScheduleOffsetTBA(List<TBAMatchData> matches)
        {
            DateTime today = DateTime.Now.Date;
            var allOfTodaysMatches = matches.Where(m => m.timeDT.Date == today);
            var todaysFinishedMatches = allOfTodaysMatches.Where(m => m.actual_time != 0);
            double average = 0.0;
            if (todaysFinishedMatches.Count() < allOfTodaysMatches.Count())
            {
                List<TBAMatchData> last3Matches = todaysFinishedMatches.Reverse().Take(3).ToList();
                double sum = last3Matches.Select(m => (m.actual_timeDT - m.timeDT).TotalMinutes).Sum();
                if (Math.Abs(sum) > 0)
                    average = sum / last3Matches.Count();

                //if(average == 0.0)
                //{
                //    //TODO: if the next match scores have not posted and the time for the match after that has passed, consider matches running late
                //    //      and report as schedule offset by that delta
                //    List<Match> tmatches = matches.Where(m => m.actualStartTime == null).Take(3).ToList();
                //    if(tmatches[1].startTime < DateTime.Now)
                //    {
                //        average = (DateTime.Now - tmatches[1].startTime).TotalMinutes;
                //    }
                //}
            }
            return average;
        }

        [HttpPost]
        public JsonResult TeamsOfInterestAjax(string eventCode, string teamList, string sortName, string sortDirection)
        {
            if (eventCode.Length > 0)
            {
                List<string> teams = BuildTeamsOfInterest(teamList);

                this.ControllerContext.HttpContext.Response.Cookies.Add(new HttpCookie("teamList") { Value = string.Join(",", teams), Expires = DateTime.Now.AddYears(1) });

                List<TBATeam> teamsOfInterest = GatherTeamsOfInterest(eventCode, teams);
                teamsOfInterest = teamsOfInterest.Where(t => t.Stats != null).ToList();

                if (sortName.Length == 0) sortName = "Rank";

                if (sortName == "Number")
                    return (sortDirection == "ASC" ? Json(teamsOfInterest.OrderBy(t => t.team_number).ToList()) : Json(teamsOfInterest.OrderByDescending(t => t.team_number).ToList()));
                else if (sortName == "Name")
                    return (sortDirection == "ASC" ? Json(teamsOfInterest.OrderBy(t => t.nickname).ToList()) : Json(teamsOfInterest.OrderByDescending(t => t.nickname).ToList()));
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
            //var matches = TBAAPI.GetMatches(eventCode);
            //TODO: getting the full dashboard may be overkill
            Dashboard dashboard = BuildEventDashboardTBA(eventCode, new List<string>());

            if (dashboard.EventState != FRCEventState.Invalid)
            {
                TBAEventRankings rankings = TBAAPI.GetEventRankings(eventCode); //API CALL (TBA-compliant cache)
                if (rankings != null && rankings.rankings != null)
                {
                    //TODO: Int32.Parse (I think this was an issue with offseason events where teams might have a letter in their name)
                    dashboard.EventRankings = rankings.rankings.ToDictionary(e => Int32.Parse(Regex.Replace(e.team_key, "[^0-9,-]+", "")), e => e);
                }
            }

            //TODO: This is for testing live updates
            var matches = dashboard.TBAMatches;
            //matches = matches.OrderBy(m => m.sortTitle).ToList();
            if (eventCode == "2023txbel")
            {
                int lastMatch = rnd.Next(5, matches.Count - 20);
                for (int i = 0; i < matches.Count(); i++)
                {
                    TBAMatchData match = matches[i];
                    if (i < lastMatch)
                    {
                        match.score_breakdown.red.totalPoints = rnd.Next(10, 125);
                        match.alliances.red.predictedPoints = rnd.Next(10, 125);
                        match.score_breakdown.red.rp = rnd.Next(0, 4);
                        match.score_breakdown.blue.totalPoints = rnd.Next(10, 125);
                        match.alliances.blue.predictedPoints = rnd.Next(10, 125);
                        match.score_breakdown.blue.rp = rnd.Next(0, 4);

                        //if (match.comp_level == "f")
                        //    match.actual_time = 0;
                    }
                    else
                        match.actual_time = 0;
                }
            }
            //return Json(dashboard, JsonRequestBehavior.AllowGet);
            return Json(new { EventState = dashboard.EventState.ToString(), TBAMatches = dashboard.TBAMatches, ScheduleOffset = dashboard.ScheduleOffset, EventRankings = dashboard.EventRankings.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value) }, JsonRequestBehavior.AllowGet);
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

        //        //TODO: Should this be up in the != null section above?
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
        //    //    //TODO: if the next match scores have not posted and the time for the match after that has passed, consider matches running late
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
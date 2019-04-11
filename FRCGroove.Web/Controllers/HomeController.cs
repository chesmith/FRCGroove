using FRCGroove.Lib;
using FRCGroove.Lib.models;

using FRCGroove.Web.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FRCGroove.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string districtCode = "")
        {
            FRCEventListing frcEventListing = new FRCEventListing();

            frcEventListing.Districts = FRCEventsAPI.GetDistrictListing();
            frcEventListing.Districts.Insert(0, new District() { code = "All", name = "All Districts" });
            frcEventListing.Districts.Insert(1, new District() { code = "World", name = "World Championship" });

            if (districtCode.Length == 0 && this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("districtCode"))
            {
                string districtCodeFromCookie = this.ControllerContext.HttpContext.Request.Cookies["districtCode"].Value;
                if (districtCodeFromCookie.Length > 0)
                {
                    districtCode = districtCodeFromCookie;
                }
            }

            frcEventListing.districtCode = districtCode;

            List<Event> eventListing;
            if (districtCode.Length > 0 && districtCode != "World" && districtCode != "All")
                eventListing = FRCEventsAPI.GetDistrictEventListing(districtCode);
            else
                eventListing = FRCEventsAPI.GetEventListing();

            if (districtCode == "World")
            {
                eventListing = eventListing.Where(e => e.name.StartsWith("FIRST Championship")).ToList();
            }

            frcEventListing.PastEvents = eventListing.Where(e => e.dateEnd < DateTime.Now.Date).ToList();
            frcEventListing.CurrentEvents = eventListing.Where(e => e.dateStart <= DateTime.Now.Date && e.dateEnd >= DateTime.Now.Date).ToList();
            frcEventListing.FutureEvents = eventListing.Where(e => e.dateStart > DateTime.Now.Date).ToList();

            HttpCookie cookie = new HttpCookie("districtCode");
            cookie.Value = string.Join(",", districtCode);
            this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);

            return View(frcEventListing);
        }

        private Dashboard BuildEventDashboard(string districtCode, string eventCode, List<string> teamList)
        {
            Dashboard dashboard = new Dashboard();

            dashboard.districtCode = districtCode;

            if (eventCode.Length > 0)
            {
                dashboard.FrcEvent = FRCEventsAPI.GetEvent(eventCode);
                if (dashboard.FrcEvent != null)
                {
                    dashboard.Matches = FRCEventsAPI.GetFullHybridSchedule(eventCode);

                    if (dashboard.Matches != null && (dashboard.EventState == FRCEventState.Past || dashboard.EventState == FRCEventState.Qualifications || dashboard.EventState == FRCEventState.Quarterfinals || dashboard.EventState == FRCEventState.Semifinals || dashboard.EventState == FRCEventState.Finals))
                    {
                        if (dashboard.EventState != FRCEventState.Qualifications)
                        {
                            dashboard.Alliances = FRCEventsAPI.GetPlayoffAlliances(eventCode);
                            dashboard.Bracket = new PlayoffBracket(dashboard.Alliances, dashboard.Matches);
                        }

                        if (dashboard.EventState != FRCEventState.Past)
                        {
                            List<Match> matchesForOffsetCalc = dashboard.Matches;
                            if (dashboard.EventState != FRCEventState.Qualifications)
                                matchesForOffsetCalc = dashboard.Matches.Where(m => m.tournamentLevel == "Playoff").ToList();
                            dashboard.ScheduleOffset = CalculateScheduleOffset(matchesForOffsetCalc);
                        }
                    }
                }
            }

            if (teamList != null && teamList.Count > 0)
            {
                foreach (string teamNumber in teamList)
                {
                    RegisteredTeam team = FRCEventsAPI.GetTeam(Int32.Parse(teamNumber));
                    if (eventCode.Length > 0)
                    {
                        EnrichTeamData(team, eventCode, dashboard.Matches);
                    }
                    dashboard.TeamsOfInterest.Add(team);
                }
            }

            return dashboard;
        }

        private void EnrichTeamData(RegisteredTeam team, string eventCode, List<Match> matches)
        {
            if (team != null)
            {
                List<DistrictRank> districtRankings = FRCEventsAPI.GetDistrictRankings(team.districtCode, team.number);
                if (districtRankings.Count > 0)
                    team.districtRank = districtRankings[0].rank;

                List<EventRanking> eventRankings = FRCEventsAPI.GetEventRankings(eventCode, team.number);
                if (eventRankings.Count > 0)
                    team.eventRank = eventRankings[0].rank;

                team.Stats = TBAAPI.GetStats("2019" + eventCode.ToLower());

                if (matches != null)
                {
                    Match nextMatch = (Match)matches.Where(m => m.teams.Count(t => t.number == team.number) > 0 && m.actualStartTime == null).FirstOrDefault();
                    if (nextMatch != null)
                    {
                        team.NextMatch = nextMatch;
                    }
                }
            }
        }

        private double CalculateScheduleOffset(List<Match> matches)
        {
            DateTime today = DateTime.Now.Date;
            List<Match> todaysMatches = matches.Where(m => m.startTime.Date == today && m.actualStartTime != null).Reverse().Take(3).ToList();
            double sum = todaysMatches.Select(m => (m.actualStartTime.Value - m.startTime).TotalMinutes).Sum();
            double average = 0.0;
            if (Math.Abs(sum) > 0)
                average = sum / todaysMatches.Count();

            return average;
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult FRCEvent(string districtCode = "", string eventCode = "", string teamList = "")
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

            teams = teamsToKeep.Distinct().ToList();

            Dashboard dashboard = BuildEventDashboard(districtCode, eventCode, teams);

            HttpCookie cookie = new HttpCookie("teamList");
            cookie.Value = string.Join(",", teams);
            this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);

            //TODO: if we added teams from the cookie that weren't there in the input list, do a redirect with the full URL instead of just showing the dashboard (update the cookie first?)

            return View(dashboard);
        }
    }
}
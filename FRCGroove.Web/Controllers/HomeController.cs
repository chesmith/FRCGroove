using FRCGroove.Lib;
using FRCGroove.Lib.models;

using FRCGroove.Web.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FRCGroove.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string eventCode = "", string teamList = "")
        {
            FRCEventListing frcEventListing = new FRCEventListing();
            List<Event> eventListing = FRCEventsAPI.GetDistrictEventListing("TX");

            frcEventListing.PastEvents = eventListing.Where(e => e.dateEnd < DateTime.Now.Date).ToList();
            frcEventListing.CurrentEvents = eventListing.Where(e => e.dateStart <= DateTime.Now.Date && e.dateEnd >= DateTime.Now.Date).ToList();
            frcEventListing.FutureEvents = eventListing.Where(e => e.dateStart > DateTime.Now.Date).ToList();

            return View(frcEventListing);
        }

        private Dashboard BuildEventDashboard(string eventCode, string teamList)
        {
            Dashboard dashboard = new Dashboard();

            if (eventCode.Length > 0)
            {
                dashboard.FrcEvent = FRCEventsAPI.GetEvent(eventCode);
                if (dashboard.FrcEvent != null)
                {
                    dashboard.Matches = FRCEventsAPI.GetFullHybridSchedule(eventCode);
                    //dashboard.Matches = FRCEventsAPI.GetHybridSchedule(eventCode, "Qualification");

                    TimeSpan[] rollingDelta = new TimeSpan[3];
                    foreach (Match match in dashboard.Matches)
                    {
                        if (match.actualStartTime == null) break;

                        rollingDelta[0] = rollingDelta[1];
                        rollingDelta[1] = rollingDelta[2];
                        rollingDelta[2] = (match.actualStartTime.Value - match.startTime);
                    }
                    dashboard.ScheduleOffset = (rollingDelta[0].TotalMinutes + rollingDelta[1].TotalMinutes + rollingDelta[2].TotalMinutes) / 3;
                }
            }

            if (teamList.Length > 0)
            {
                string[] teamNumbers = teamList.Split(',');
                foreach (string teamNumber in teamNumbers)
                {
                    RegisteredTeam team = FRCEventsAPI.GetTeam(Int32.Parse(teamNumber));
                    if (eventCode.Length > 0)
                    {
                        EnrichTeamData(team, eventCode, dashboard.Matches);
                    }
                    dashboard.TeamsOfInterest.Add(team);
                }
            }

            dashboard.Bracket = new PlayoffBracket(dashboard.Matches);

            return dashboard;
        }

        //public ActionResult Index()
        //{
        //    Dashboard dashboard = new Dashboard();

        //    //TODO: input list of teams
        //    dashboard.Teams.Add(FRCEventsAPI.GetTeam(5414));

        //    foreach (RegisteredTeam team in dashboard.Teams)
        //    {
        //        EnrichTeamData(team);
        //    }

        //    //TODO: input event code (required)
        //    dashboard.Matches = FRCEventsAPI.GetFullHybridSchedule("TXGRE");

        //    TimeSpan[] rollingDelta = new TimeSpan[3];
        //    foreach (Match match in dashboard.Matches)
        //    {
        //        if (match.actualStartTime.Year == 1 || match.actualStartTime == null) break;

        //        rollingDelta[0] = rollingDelta[1];
        //        rollingDelta[1] = rollingDelta[2];
        //        rollingDelta[2] = (match.actualStartTime - match.startTime);
        //    }
        //    dashboard.ScheduleOffset = (rollingDelta[0].TotalMinutes + rollingDelta[1].TotalMinutes + rollingDelta[2].TotalMinutes) / 3;

        //    return View(dashboard);
        //}

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

                //TODO: get team's next match time
                Match nextMatch = (Match)matches.Where(m => m.teams.Where(t => t.number == team.number).Count() > 0 && m.actualStartTime == null).FirstOrDefault();
                if(nextMatch != null)
                {
                    team.NextMatch = nextMatch;
                }
            }
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

        public ActionResult FRCEvent(string eventCode = "", string teamList = "")
        {
            Dashboard dashboard = BuildEventDashboard(eventCode, teamList);

            return View(dashboard);
        }
    }
}
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
        public ActionResult Index()
        {
            Dashboard dashboard = new Dashboard();

            dashboard.Teams = FRCEventsAPI.GetTeamListing("TX", 5414);
            dashboard.DistrictRank = FRCEventsAPI.GetDistrictRankings("TX", dashboard.Teams[0].teamNumber)[0].rank;
            List<EventRanking> eventRankings = FRCEventsAPI.GetEventRankings("TXGRE", dashboard.Teams[0].teamNumber);
            if(eventRankings.Count > 0)
                dashboard.EventRank = eventRankings[0].rank;
            dashboard.Matches = FRCEventsAPI.GetFullHybridSchedule("TXGRE");

            TimeSpan[] rollingDelta = new TimeSpan[3];
            foreach (Match match in dashboard.Matches)
            {
                if (match.actualStartTime.Year == 1 || match.actualStartTime == null) break;

                rollingDelta[0] = rollingDelta[1];
                rollingDelta[1] = rollingDelta[2];
                rollingDelta[2] = (match.actualStartTime - match.startTime);
            }
            dashboard.ScheduleOffset = (rollingDelta[0].TotalMinutes + rollingDelta[1].TotalMinutes + rollingDelta[2].TotalMinutes) / 3;

            dashboard.Stats = TBAAPI.GetStats("2019txgre");

            return View(dashboard);
        }

        //private void EnrichTeamData(RegisteredTeam team)
        //{
        //    dashboard.DistrictRank = FRCEventsAPI.GetDistrictRankings("TX", dashboard.Teams[0].teamNumber)[0].rank;
        //    List<EventRanking> eventRankings = FRCEventsAPI.GetEventRankings("TXGRE", dashboard.Teams[0].teamNumber);
        //    if (eventRankings.Count > 0)
        //        dashboard.EventRank = eventRankings[0].rank;

        //    dashboard.Stats = TBAAPI.GetStats("2019txgre");
        //}

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
    }
}
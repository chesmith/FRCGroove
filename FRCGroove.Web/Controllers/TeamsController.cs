using FRCGroove.Lib;
using FRCGroove.Lib.Models.Groove;
using FRCGroove.Lib.Models.Statboticsv2;
using FRCGroove.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FRCGroove.Web.Controllers
{
    public class TeamsController : Controller
    {
        private Dictionary<string, string> _champsDivisions = new Dictionary<string, string>() {
                { "ARPKY", "ARCHIMEDES" },
                { "CPRA", "CURIE" },
                { "DCMP", "DALY" },
                { "GCMP", "GALILEO" },
                { "HCMP", "HOPPER" },
                { "JCMP", "JOHNSON" },
                { "MPCIA", "MILSTEIN" },
                { "NPFCMP", "NEWTON" }
            };

        // GET: Teams
        public ActionResult Index(string sort, string search)
        {
            TeamListing teamListingModel = BuildChampsTeamListing(sort, search);

            return View(teamListingModel);
        }

        private TeamListing BuildChampsTeamListing(string sort, string search)
        {
            TeamListing teamListing = new TeamListing();

            List<GrooveTeam> champsTeams = Groove.GetChampsTeams();

            Dictionary<int, string> pitLocations = Groove.GetChampsPitLocations();
            if (pitLocations != null)
            {
                foreach (GrooveTeam team in champsTeams)
                {
                    if (pitLocations.ContainsKey(team.number))
                        team.pitLocation = pitLocations[team.number];
                }
            }

            foreach(GrooveTeam team in champsTeams)
            {
                if (StatboticsAPIv2.EPACache.ContainsKey(team.number))
                    team.epa = StatboticsAPIv2.EPACache[team.number];
                else
                    team.epa = new EPA() { epa_end = -1 };
            }

            if (sort == null || sort.Length == 0) sort = "#";

            if (search != null && search.Length > 0)
            {
                search = search.ToLower();
                champsTeams = champsTeams.Where(t => t.number.ToString().StartsWith(search) || t.name.ToLower().Contains(search) || t.champsDivision.ToLower().Contains(search)).ToList();
            }

            if (sort == "#" || sort.ToLower() == "number")
                teamListing.Teams = champsTeams.OrderBy(t => t.number).ToList();
            else if (sort.ToLower() == "name")
                teamListing.Teams = champsTeams.OrderBy(t => t.name).ToList();
            else if (sort.ToLower() == "epa")
                teamListing.Teams = champsTeams.OrderByDescending(t => t.epa.epa_end).ToList();
            else if (sort.ToLower() == "division")
                teamListing.Teams = champsTeams.OrderBy(t => t.champsDivision).ToList();
            else if (sort.ToLower() == "pit")
                teamListing.Teams = champsTeams.OrderBy(t => t.pitLocation).ToList();

            teamListing.Watchlist = BuildTeamsOfInterest(string.Empty).Select(t => Int32.Parse(t)).ToList();

            return teamListing;
        }

        private TeamListing BuildAllTeamListing(string sort, string search)
        {
            TeamListing teamListing = new TeamListing();

            if (search.Length > 0)
            {
                List<GrooveTeam> allTeams = Groove.TeamListingCache.Select(t => t.Value).ToList();

                foreach (GrooveTeam team in allTeams)
                {
                    if (StatboticsAPIv2.EPACache.ContainsKey(team.number))
                        team.epa = StatboticsAPIv2.EPACache[team.number];
                    else
                        team.epa = new EPA() { epa_end = -1 };
                }

                if (sort == null || sort.Length == 0) sort = "#";

                if (search != null && search.Length > 0)
                {
                    search = search.ToLower();
                    allTeams = allTeams.Where(t => t.number.ToString().StartsWith(search) || t.name.ToLower().Contains(search)).ToList();
                }

                teamListing.Teams = allTeams;

                if (sort == "#" || sort.ToLower() == "number")
                    teamListing.Teams = allTeams.OrderBy(t => t.number).ToList();
                else if (sort.ToLower() == "name")
                    teamListing.Teams = allTeams.OrderBy(t => t.name).ToList();
                else if (sort.ToLower() == "epa")
                    teamListing.Teams = allTeams.OrderByDescending(t => t.epa.epa_end).ToList();
            }

            teamListing.Watchlist = BuildTeamsOfInterest(string.Empty).Select(t => Int32.Parse(t)).ToList();

            return teamListing;
        }

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

        public ActionResult WatchList()
        {
            return View();
        }

        [HttpPost]
        public JsonResult UpdateWatchList(string teamList)
        {
            List<string> teams = BuildTeamsOfInterest(teamList);

            this.ControllerContext.HttpContext.Response.Cookies.Add(new HttpCookie("teamList") { Value = string.Join(",", teams), Expires = new DateTime(DateTime.Now.Year + 1, DateTime.Now.Month, DateTime.Now.Day) });

            return null;
        }

        public JsonResult GetChampsTeams(string sort, string search)
        {
            return Json(BuildChampsTeamListing(sort, search), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAllTeams(string sort, string search)
        {
            return Json(BuildAllTeamListing(sort, search), JsonRequestBehavior.AllowGet);
        }
    }
}
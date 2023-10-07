using FRCGroove.Lib;
using FRCGroove.Lib.Models;

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
            TBAEventListing eventListing = new TBAEventListing();

            eventListing.Districts = TBAAPI.GetDistrictListing();
            eventListing.Districts.Insert(0, new TBADistrict() { abbreviation = "All", display_name = "All Districts", key = "All", year = 2023 });
            eventListing.Districts.Insert(0, new TBADistrict() { abbreviation = "World", display_name = "World Championship", key = "World", year = 2023 });

            if (districtCode.Length == 0 && this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("districtCode"))
            {
                string districtCodeFromCookie = this.ControllerContext.HttpContext.Request.Cookies["districtCode"].Value;
                if (districtCodeFromCookie.Length > 0)
                {
                    districtCode = districtCodeFromCookie;
                }
            }

            eventListing.districtCode = districtCode;

            List<TBAEvent> events;
            if (districtCode.Length > 0 && districtCode != "World" && districtCode != "All")
            {
                events = TBAAPI.GetDistrictEventListing(districtCode);
            }
            else
            {
                events = TBAAPI.GetEventListing(DateTime.Now.Year);
            }

            if (events != null)
            {
                if (districtCode == "World")
                {
                    events = events.Where(e => e.event_type == 3).ToList();
                }

                //TODO: this assumes dates and times are in my timezone - adjust everything to UTC or something 
                eventListing.PastEvents = events.Where(e => e.dateEnd < DateTime.Now.Date).OrderBy(e => e.dateStart).ThenBy(e => e.name).ToList();
                eventListing.CurrentEvents = events.Where(e => e.dateStart <= DateTime.Now.Date && e.dateEnd >= DateTime.Now.Date).OrderBy(e => e.dateStart).ThenBy(e => e.name).ToList();
                eventListing.FutureEvents = events.Where(e => e.dateStart > DateTime.Now.Date).OrderBy(e => e.dateStart).ThenBy(e => e.name).ToList();
            }

            HttpCookie cookie = new HttpCookie("districtCode");
            cookie.Value = string.Join(",", districtCode);
            cookie.Expires = DateTime.Now.AddYears(1);
            this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);

            return View(eventListing);
        }

        private void AdjustChampsEventCodes(List<Event> events)
        {
            var champs = events.Where(e => FRCEventsAPI.ChampsDivisions.ContainsKey(e.code));
            foreach (Event e in champs)
            {
                e.code = FRCEventsAPI.ChampsDivisions[e.code];
            }
        }

        public ActionResult About()
        {
            //ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult ResetEPACache()
        {
            TBAAPI.ResetEPACache();

            return View();
        }
    }
}
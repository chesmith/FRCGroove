using FRCGroove.Lib;
using FRCGroove.Lib.Models.Groove;

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
        public ActionResult Index(string districtKey = "")
        {
            EventListing eventListing = new EventListing();

            eventListing.Districts = Groove.GetDistricts();
            eventListing.Districts.Insert(0, new GrooveDistrict() { key = "All", name = "All Districts", year = DateTime.Now.Year });
            eventListing.Districts.Add(new GrooveDistrict() { key = "World", name = "World Championship", year = DateTime.Now.Year });

            if (districtKey.Length == 0 && this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("districtKey"))
            {
                string districtKeyFromCookie = this.ControllerContext.HttpContext.Request.Cookies["districtKey"].Value;
                if (districtKeyFromCookie.Length > 0)
                {
                    districtKey = districtKeyFromCookie;
                }
            }

            eventListing.districtKey = districtKey;

            List<GrooveEvent> events = GetEventListing(eventListing.districtKey);
            if (events != null)
            {
                //TODO: this assumes dates and times are in my timezone (US Central) - is it possible to account for the user's local timezone?
                eventListing.PastEvents = events.Where(e => e.dateEnd < DateTime.Now.Date).OrderBy(e => e.dateStart).ThenBy(e => e.name).ToList();
                eventListing.CurrentEvents = events.Where(e => e.dateStart <= DateTime.Now.Date && e.dateEnd >= DateTime.Now.Date).OrderBy(e => e.dateStart).ThenBy(e => e.name).ToList();
                eventListing.FutureEvents = events.Where(e => e.dateStart > DateTime.Now.Date).OrderBy(e => e.dateStart).ThenBy(e => e.name).ToList();
            }

            HttpCookie cookie = new HttpCookie("districtKey");
            cookie.Value = string.Join(",", districtKey);
            cookie.Expires = DateTime.Now.AddYears(1);
            this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);

            return View(eventListing);
        }

        private static List<GrooveEvent> GetEventListing(string districtKey)
        {
            if (districtKey.Length > 0 && districtKey != "World" && districtKey != "All")
            {
                return Groove.GetDistrictEvents(districtKey);
            }
            else
            {
                List<GrooveEvent> events = Groove.GetEvents(DateTime.Now.Year);
                if (districtKey == "World")
                {
                    return events.Where(e => e.type == "Championship Division" || e.type == "Championship Finals").ToList();
                }
                return events;
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
            StatboticsAPIv2.ResetEPACache();
            return View();
        }

        public ActionResult ResetTeamListingCache()
        {
            Groove.CreateTeamListingCache();
            return View();
        }
    }
}
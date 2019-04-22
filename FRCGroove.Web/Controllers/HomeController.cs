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


            if (eventListing != null)
            {
                if (districtCode == "World")
                {
                    eventListing = eventListing.Where(e => e.name.StartsWith("FIRST Championship")).ToList();
                }

                frcEventListing.PastEvents = eventListing.Where(e => e.dateEnd < DateTime.Now.Date).ToList();
                frcEventListing.CurrentEvents = eventListing.Where(e => e.dateStart <= DateTime.Now.Date && e.dateEnd >= DateTime.Now.Date).ToList();
                frcEventListing.FutureEvents = eventListing.Where(e => e.dateStart > DateTime.Now.Date).ToList();
            }

            HttpCookie cookie = new HttpCookie("districtCode");
            cookie.Value = string.Join(",", districtCode);
            cookie.Expires = DateTime.Now.AddYears(1);
            this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);

            return View(frcEventListing);
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
    }
}
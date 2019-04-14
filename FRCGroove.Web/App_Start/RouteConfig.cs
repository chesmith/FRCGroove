using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace FRCGroove.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            //);

            routes.MapRoute(
                name: "Default",
                url: "",
                defaults: new { controller = "Home", action = "Index" }
            );

            routes.MapRoute(
                name: "About",
                url: "About",
                defaults: new { controller = "Home", action = "About" }
            );

            routes.MapRoute(
                name: "TeamsOfInterestAjax",
                url: "FRCEvent/TeamsOfInterestAjax",
                defaults: new { controller = "FRCEvent", action = "TeamsOfInterestAjax" }
            );

            routes.MapRoute(
                name: "FRCEvent",
                url: "FRCEvent/{eventCode}/{teamList}",
                defaults: new { controller = "FRCEvent", action = "Index", teamList = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Teams",
                url: "Teams",
                defaults: new { controller = "Teams", action = "Index" }
            );
        }
    }
}

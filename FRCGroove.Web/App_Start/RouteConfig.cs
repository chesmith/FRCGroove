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
                name: "Contact",
                url: "Contact",
                defaults: new { controller = "Home", action = "Contact" }
            );

            routes.MapRoute(
                name: "TeamsOfInterestAjax",
                url: "FRCEvent/TeamsOfInterestAjax",
                defaults: new { controller = "FRCEvent", action = "TeamsOfInterestAjax" }
            );

            routes.MapRoute(
                name: "GetDashboardData",
                url: "FRCEvent/GetDashboardData",
                defaults: new { controller = "FRCEvent", action = "GetDashboardData" }
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

            routes.MapRoute(
                name: "WatchList",
                url: "Teams/List",
                defaults: new { controller = "Teams", action = "WatchList" }
            );

            routes.MapRoute(
                name: "UpdateWatchList",
                url: "Teams/UpdateWatchList",
                defaults: new { controller = "Teams", action = "UpdateWatchList" }
            );

            routes.MapRoute(
                name: "GetChampsTeams",
                url: "Teams/GetChampsTeams",
                defaults: new { controller = "Teams", action = "GetChampsTeams" }
            );

            routes.MapRoute(
                name: "GetAllTeams",
                url: "Teams/GetAllTeams",
                defaults: new { controller = "Teams", action = "GetAllTeams" }
            );

            routes.MapRoute(
                name: "ResetEPACache",
                url: "ResetEPACache",
                defaults: new { controller = "Home", action = "ResetEPACache" }
            );

            routes.MapRoute(
                name: "ResetTeamCache",
                url: "ResetTeamCache",
                defaults: new { controller = "Home", action = "ResetTeamCache" }
            );
        }
    }
}

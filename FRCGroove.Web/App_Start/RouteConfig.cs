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
                name: "Scout",
                url: "Scout",
                defaults: new { controller = "Scout", action = "Index" }
            );

            routes.MapRoute(
                name: "LogCargo",
                url: "Scout/LogCargo",
                defaults: new { controller = "Scout", action = "LogCargo" }
            );

            routes.MapRoute(
                name: "Data",
                url: "Scout/Data/{team}/{match}",
                defaults: new { controller = "Scout", action = "Data", team = UrlParameter.Optional, match = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Selection",
                url: "Scout/Selection/{tbaEventCode}",
                defaults: new { controller = "Scout", action = "Selection" }
            );

            routes.MapRoute(
                name: "Rank",
                url: "Scout/Rank/{tbaEventCode}/{teamNumber}",
                defaults: new { controller = "Scout", action = "Rank" }
            );

            routes.MapRoute(
                name: "ScoutList",
                url: "Scout/List/{tbaEventCode}",
                defaults: new { controller = "Scout", action = "List" }
            );

            routes.MapRoute(
                name: "PreScout",
                url: "Scout/PreScout/{tbaEventCode}",
                defaults: new { controller = "Scout", action = "PreScout" }
            );
        }
    }
}

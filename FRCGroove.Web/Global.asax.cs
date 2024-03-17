using FRCGroove.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace FRCGroove.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            FRCEventsAPIv2.CacheFolder = HostingEnvironment.MapPath("~/App_Data/cache/");

            Groove.CacheFolder = HostingEnvironment.MapPath("~/App_Data/cache/");
            if(Groove.DoesTeamListingCacheExist())
                Groove.LoadTeamListingCache();
            else
                Groove.CreateTeamListingCache();

            StatboticsAPIv2.CacheFolder = HostingEnvironment.MapPath("~/App_Data/cache/");
            StatboticsAPIv2.InitializeEPACache();
        }
    }
}

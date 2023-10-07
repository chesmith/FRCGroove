using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using FRCGroove.Lib.Models;

namespace FRCGroove.Web.Models
{
    public class TBAEventListing
    {
        public List<TBADistrict> Districts { get; set; }

        public string districtCode { get; set; }

        public List<TBAEvent> PastEvents { get; set; }
        public List<TBAEvent> CurrentEvents { get; set; }
        public List<TBAEvent> FutureEvents { get; set; }

    }
}
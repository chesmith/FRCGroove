using System.Collections.Generic;
using FRCGroove.Lib.Models.Groove;
using FRCGroove.Lib.Models.TBAv3;

namespace FRCGroove.Web.Models
{
    public class EventListing
    {
        public List<GrooveDistrict> Districts { get; set; }

        public string districtKey { get; set; }

        public List<GrooveEvent> PastEvents { get; set; }
        public List<GrooveEvent> CurrentEvents { get; set; }
        public List<GrooveEvent> FutureEvents { get; set; }

    }
}
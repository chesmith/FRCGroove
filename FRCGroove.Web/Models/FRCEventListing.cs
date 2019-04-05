using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using FRCGroove.Lib.models;

namespace FRCGroove.Web.Models
{
    public class FRCEventListing
    {
        public List<District> Districts { get; set; }

        public List<Event> PastEvents { get; set; }
        public List<Event> CurrentEvents { get; set; }
        public List<Event> FutureEvents { get; set; }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using FRCGroove.Lib.Models.FRCv2;

namespace FRCGroove.Web.Models
{
    public class FRCEventListing
    {
        public List<FRCDistrict> Districts { get; set; }

        public string districtCode { get; set; }

        public List<Event> PastEvents { get; set; }
        public List<Event> CurrentEvents { get; set; }
        public List<Event> FutureEvents { get; set; }
    }
}
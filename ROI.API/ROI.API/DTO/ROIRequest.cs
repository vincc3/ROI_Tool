using System;
using System.Collections.Generic;

namespace ROI.API.DTO
{
    public class RoiRequest
    {
        public double totalInvest { get; set; }

        public List<RoiRequestItem> items { get; set; }
    }

    public class RoiRequestItem
    {
        public int option { get; set; }

        public double percent { get; set; }
    }

}

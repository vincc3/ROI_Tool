using System;

namespace ROI.API.DTO
{
    public class RoiResult
    {
        public double InvestReturnInAud { get; set; }

        public double InvestReturnInUsd { get; set; }

        public double FeeInAud { get; set; }

        public double FeeInUsd { get; set; }
    }
}

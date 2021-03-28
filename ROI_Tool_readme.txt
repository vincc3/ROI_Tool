ROI.API: writen in .NET Core 3.1
The following APIs are provided:
1. Get total ROI results according to all options settings.
POST http://host/ori
http body is JSON format data:
{"totalInvest":1000000,
"items":[{"option":1,"percent":20},
         {"option":3,"percent":30},
         {"option":9,"percent":25}]}


2. GET http://host/ori?option={ORI_option}&percent={percent}&invest={totalInvestment}
It gets the result of one ROI option setting. The API is just for internal testing.


Both APIs return the object:
    public class RoiResult
    {
        public double InvestReturnInAud { get; set; }

        public double InvestReturnInUsd { get; set; }

        public double FeeInAud { get; set; }

        public double FeeInUsd { get; set; }
    }

====================================================================
ROI.React:
The source code is under "roi-app".
The ROI.API URL is set "baseURL" in "roi-app/src/Config.js":
module.exports = {
    baseURL: "http://localhost:31377"
};
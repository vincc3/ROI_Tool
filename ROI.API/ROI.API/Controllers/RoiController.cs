using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ROI.API.DTO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ROI.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RoiController : ControllerBase
    {
        private readonly ILogger<RoiController> _logger;

        public RoiController(ILogger<RoiController> logger)
        {
            _logger = logger;
        }

        // query ROI: GET ori?option={option}&percent={percent}&invest={invest}
        [HttpGet]
        public async Task<ActionResult<RoiResult>> Get([FromQuery(Name = "option")] int investOption,
                                                        [FromQuery(Name = "percent")] double percentOfInvest, 
                                                        [FromQuery(Name = "invest")] double totalInvestment)
        {
            InvestOptions enumInvestOption = (InvestOptions)investOption;
            var rate = await GetAusRateOfUsd();
            var result =  CalculateRoiResult(rate, totalInvestment, percentOfInvest, enumInvestOption);
            return result;
        }

        // query ROI: POST
        [HttpPost]
        public async Task<ActionResult<RoiResult>> Post([FromBody]RoiRequest request)
        {
            var result = new RoiResult()
            {
                InvestReturnInAud = 0,
                InvestReturnInUsd = 0,
                FeeInAud = 0,
                FeeInUsd = 0
            };
            var totalInvestment = request.totalInvest;
            var rate = await GetAusRateOfUsd();
            foreach (var item in request.items)
            {
                if (item.option > 0 && item.percent > 0)
                {
                    InvestOptions enumInvestOption = (InvestOptions)item.option;
                    var res = CalculateRoiResult(rate, totalInvestment, item.percent, enumInvestOption);
                    result.InvestReturnInAud += res.InvestReturnInAud;
                    result.InvestReturnInUsd += res.InvestReturnInUsd;
                    result.FeeInAud += res.FeeInAud;
                    result.FeeInUsd += res.FeeInUsd;
                }
            }
            result.FeeInUsd = Math.Round(result.FeeInUsd, 4);
            result.FeeInAud = Math.Round(result.FeeInAud, 4);
            return result;
        }

        private async Task<double> GetAusRateOfUsd()
        {
            double rate = 1;
            string url = "https://api.exchangeratesapi.io/latest?base=USD";
            try
            {
                using (var request = new HttpClient())
                {
                    request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var jsonResult = await request.GetStringAsync(url).ConfigureAwait(false);
                    using JsonDocument doc = JsonDocument.Parse(jsonResult);
                    JsonElement root = doc.RootElement;
                    JsonElement rateObj;
                    if (root.TryGetProperty("rates", out rateObj))
                    {
                        JsonElement audObj;
                        if (rateObj.TryGetProperty("AUD", out audObj))
                        {
                            double audRate = 1;
                            if (audObj.TryGetDouble(out audRate))
                                rate = audRate;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return rate;
        }

        private RoiResult CalculateRoiResult(double rate, double totalInvestment, double percentOfInvest, InvestOptions enumInvestOption)
        {
            double investment = totalInvestment / 100 * percentOfInvest;
            var result = new RoiResult();
            switch (enumInvestOption)
            {
                case InvestOptions.CashInvestments:
                    {
                        if (percentOfInvest <= 50)
                        {
                            result.InvestReturnInAud = CalculateResult(investment, 8.5);
                            result.FeeInAud = CalculateResult(result.InvestReturnInAud, 0.5);
                        }
                        else
                        {
                            result.InvestReturnInAud = CalculateResult(investment, 10);
                            result.FeeInAud = 0;
                        }
                        break;
                    }

                case InvestOptions.FixedInterest:
                    {
                        result.InvestReturnInAud = CalculateResult(investment, 10);
                        result.FeeInAud = CalculateResult(result.InvestReturnInAud, 1);
                        break;
                    }

                case InvestOptions.Shares:
                    {
                        if (percentOfInvest <= 70)
                        {
                            result.InvestReturnInAud = CalculateResult(investment, 4.3);
                        }
                        else
                        {
                            result.InvestReturnInAud = CalculateResult(investment, 6);
                        }

                        result.FeeInAud = CalculateResult(result.InvestReturnInAud, 2.5);
                        break;
                    }

                case InvestOptions.ManagedFunds:
                    {
                        result.InvestReturnInAud = CalculateResult(investment, 12);
                        result.FeeInAud = CalculateResult(result.InvestReturnInAud, 0.3);
                        break;
                    }

                case InvestOptions.ExchangeTradedFunds_ETFs:
                    {
                        if (percentOfInvest <= 40)
                        {
                            result.InvestReturnInAud = CalculateResult(investment, 12.8);
                        }
                        else
                        {
                            result.InvestReturnInAud = CalculateResult(investment, 12.8); // the return rate is not given in the document
                        }

                        result.FeeInAud = CalculateResult(result.InvestReturnInAud, 2);
                        break;
                    }

                case InvestOptions.InvestmentBonds:
                    {
                        result.InvestReturnInAud = CalculateResult(investment, 8);
                        result.FeeInAud = CalculateResult(result.InvestReturnInAud, 0.9);
                        break;
                    }

                case InvestOptions.Annuities:
                    {
                        result.InvestReturnInAud = CalculateResult(investment, 4);
                        result.FeeInAud = CalculateResult(result.InvestReturnInAud, 1.4);

                        result.InvestReturnInUsd = result.InvestReturnInAud / rate;
                        result.FeeInUsd = result.FeeInAud / rate;
                        break;
                    }

                case InvestOptions.ListedInvestmentCompanies_LICs:
                    {
                        result.InvestReturnInAud = CalculateResult(investment, 6);
                        result.FeeInAud = CalculateResult(result.InvestReturnInAud, 1.3);
                        break;
                    }

                case InvestOptions.RealEstateInvestmentTrusts_REITs:
                    {
                        result.InvestReturnInAud = CalculateResult(investment, 4);
                        result.FeeInAud = CalculateResult(result.InvestReturnInAud, 2);
                        break;
                    }

            }

            result.InvestReturnInUsd = result.InvestReturnInAud / rate;
            result.FeeInUsd = result.FeeInAud / rate;
            return result;
        }

        private double CalculateResult(double totalInvestment, double resultPercent)
        {
            return totalInvestment / 100 * resultPercent;
        }
    }

    /// <summary>
    /// ////////////////////////////////////////////////////
    /*  only 9 options defined in the document
    {
     "investmentOptions": [
       "Cash investments",
       "Fixed interest" ,
       "Shares",
       "Managed funds",
       "Exchange traded funds (ETFs)",
       "Investment bonds",
       "Annuities",
       "Listed investment companies (LICs)",
       "Real estate investment trusts (REITs)"
     ]
    }
    */
    /// </summary>

    public enum InvestOptions : int
    {
        CashInvestments = 1,
        FixedInterest = 2,
        Shares = 3,
        ManagedFunds = 4,
        ExchangeTradedFunds_ETFs = 5,
        InvestmentBonds = 6,
        Annuities = 7,
        ListedInvestmentCompanies_LICs = 8,
        RealEstateInvestmentTrusts_REITs = 9
    }
}

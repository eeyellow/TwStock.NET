using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TwStockNET.Extensions;
using TwStockNET.Interface;

namespace TwStockNET.Scraper
{
    public class TpexScraper : Scraper
    {
        private HttpClient httpClient = new HttpClient();

        public TpexScraper(IRateLimitOptions options = null) : base(options)
        {
        }

        public async Task<List<StocksHistoricalVM>> FetchStocksHistorical(IOption ticker)
        {
            var queryDate = $"{ticker.Date.Year - 1911}/{ticker.Date.Month.ToString().PadLeft(2, '0')}/{ticker.Date.Day.ToString().PadLeft(2, '0')}";
            var url = $"https://www.tpex.org.tw/web/stock/aftertrading/daily_close_quotes/stk_quote_result.php?d={queryDate}&o=json";

            var response = await httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<StocksHistoricalResponse>(content);

            if (json.iTotalRecords <= 0) return null;

            var data = new List<StocksHistoricalVM>();
            foreach (var row in json.aaData)
            {
                if (row[0].ToString().IsWarrant()) continue;

                var values = row.Skip(2).ToList();

                var record = new StocksHistoricalVM
                {
                    Date = ticker.Date,
                    Exchange = "TPEx",
                    Symbol = row[0].ToString(),
                    Name = row[1].ToString().Trim(),
                    Open = decimal.TryParse(values[2], out var result2) ? result2 : (decimal?)null,
                    High = decimal.TryParse(values[3], out var result3) ? result3 : (decimal?)null,
                    Low = decimal.TryParse(values[4], out var result4) ? result4 : (decimal?)null,
                    Close = decimal.TryParse(values[0], out var result0) ? result0 : (decimal?)null,
                    Volume = decimal.TryParse(values[6], out var result6) ? result6 : (decimal?)null,
                    Turnover = decimal.TryParse(values[7], out var result7) ? result7 : (decimal?)null,
                    Transaction = decimal.TryParse(values[8], out var result8) ? result8 : (decimal?)null,
                    Change = decimal.TryParse(values[1], out var result1) ? result1 : (decimal?)null
                };

                data.Add(record);
            }

            return ticker.Symbol != null ? 
                data.Where(record => record.Symbol == ticker.Symbol).ToList() : 
                data;
        }

        public async Task<List<Dictionary<string, object>>> FetchStocksInstitutional(string date, string symbol = null)
        {
            var dateParts = date.Split('-');
            string queryDate = $"{int.Parse(dateParts[0]) - 1911}/{dateParts[1]}/{dateParts[2]}";
            string url = $"https://www.tpex.org.tw/web/stock/3insti/daily_trade/3itrade_hedge_result.php?d={queryDate}&se=EW&t=D&o=json";

            var response = await httpClient.GetAsync(url);
            var json = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            if (json.iTotalRecords <= 0) return null;

            var data = new List<Dictionary<string, object>>();
            foreach (var row in json.aaData)
            {
                var values = ( (string[])row.Skip(2) ).Select(value => value.ToString()).ToList();
                var record = new Dictionary<string, object>
                {
                    {"date", date},
                    {"exchange", "TPEx"},
                    {"symbol", row[0].ToString()},
                    {"name", row[1].ToString().Trim()},
                    {"institutional", new List<Dictionary<string, object>>()}
                };

                var investors = new[]
                {
                    "外資及陸資(不含外資自營商)",
                    "外資自營商",
                    "外資及陸資合計",
                    "投信",
                    "自營商(自行買賣)",
                    "自營商(避險)",
                    "自營商合計",
                    "合計"
                };
                for (int i = 0; i < investors.Length; i++)
                {
                    var investor = new Dictionary<string, object>
                    {
                        {"investor", investors[i]},
                        {"totalBuy", i < investors.Length - 1 ? decimal.Parse(values[i * 3]) : 0},
                        {"totalSell", i < investors.Length - 1 ? decimal.Parse(values[i * 3 + 1]) : 0},
                        {"difference", decimal.Parse(values[i * 3 + (i < investors.Length - 1 ? 2 : 0)])}
                    };
                    ((List<Dictionary<string, object>>)record["institutional"]).Add(investor);
                }

                data.Add(record);
            }

            return symbol != null ? data.Where(record => record["symbol"].ToString() == symbol).ToList() : data;
        }

        public async Task<List<Dictionary<string, object>>> FetchStocksFiniHoldings(string date, string symbol = null)
        {
            var dateParts = date.Split('-');
            var form = new Dictionary<string, string>
            {
                {"years", dateParts[0]},
                {"months", dateParts[1]},
                {"days", dateParts[2]},
                {"bcode", ""},
                {"step", "2"}
            };
            string url = "https://mops.twse.com.tw/server-java/t13sa150_otc";
            var content = new FormUrlEncodedContent(form);
            var response = await httpClient.PostAsync(url, content);
            var page = Encoding.GetEncoding("big5").GetString(await response.Content.ReadAsByteArrayAsync());
            var document = new HtmlDocument();
            document.LoadHtml(page);

            var message = document.DocumentNode.SelectSingleNode("//h3").InnerText.Trim();
            if (message == "查無所需資料") return null;

            var data = new List<Dictionary<string, object>>();
            var rows = document.DocumentNode.SelectNodes("//table[1]//tr").Skip(2);
            foreach (var row in rows)
            {
                var td = row.SelectNodes("td").Select(node => node.InnerText).ToList();
                var record = new Dictionary<string, object>
                {
                    {"date", date},
                    {"exchange", "TPEx"},
                    {"symbol", td[0].Trim()},
                    {"name", td[1].Trim().Split('(')[0]},
                    {"issuedShares", decimal.Parse(td[2])},
                    {"availableShares", decimal.Parse(td[3])},
                    {"sharesHeld", decimal.Parse(td[4])},
                    {"availablePercent", decimal.Parse(td[5])},
                    {"heldPercent", decimal.Parse(td[6])},
                    {"upperLimitPercent", decimal.Parse(td[7])}
                };
                data.Add(record);
            }

            return symbol != null ? data.Where(record => record["symbol"].ToString() == symbol).ToList() : data;
        }

        public async Task<List<Dictionary<string, object>>> FetchStocksMarginTrades(string date, string symbol = null)
        {
            var dateParts = date.Split('-');
            string queryDate = $"{int.Parse(dateParts[0]) - 1911}/{dateParts[1]}/{dateParts[2]}";
            string url = $"https://www.tpex.org.tw/web/stock/margin_trading/margin_balance/margin_bal_result.php?d={queryDate}&o=json";

            var response = await httpClient.GetAsync(url);
            var json = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            if (json.iTotalRecords <= 0) return null;

            var data = new List<Dictionary<string, object>>();
            foreach (var row in json.aaData)
            {
                var values = ( (string[])row.Skip(2) ).Select(value => value.ToString()).ToList();
                var record = new Dictionary<string, object>
                {
                    {"date", date},
                    {"exchange", "TPEx"},
                    {"symbol", row[0].ToString()},
                    {"name", row[1].ToString().Trim()},
                    {"marginBuy", decimal.Parse(values[1])},
                    {"marginSell", decimal.Parse(values[2])},
                    {"marginRedeem", decimal.Parse(values[3])},
                    {"marginBalancePrev", decimal.Parse(values[0])},
                    {"marginBalance", decimal.Parse(values[4])},
                    {"marginQuota", decimal.Parse(values[7])},
                    {"shortBuy", decimal.Parse(values[10])},
                    {"shortSell", decimal.Parse(values[9])},
                    {"shortRedeem", decimal.Parse(values[11])},
                    {"shortBalancePrev", decimal.Parse(values[8])},
                    {"shortBalance", decimal.Parse(values[12])},
                    {"shortQuota", decimal.Parse(values[15])},
                    {"offset", decimal.Parse(values[16])},
                    {"note", values[17].Trim()}
                };
                data.Add(record);
            }

            return symbol != null ? data.Where(record => record["symbol"].ToString() == symbol).ToList() : data;
        }

        public async Task<List<Dictionary<string, object>>> FetchStocksShortSales(string date, string symbol = null)
        {
            var dateParts = date.Split('-');
            string queryDate = $"{int.Parse(dateParts[0]) - 1911}/{dateParts[1]}/{dateParts[2]}";
            string url = $"https://www.tpex.org.tw/web/stock/margin_trading/margin_sbl/margin_sbl_result.php?d={queryDate}&o=json";

            var response = await httpClient.GetAsync(url);
            var json = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            if (json.iTotalRecords <= 0) return null;

            var data = new List<Dictionary<string, object>>();
            foreach (var row in json.aaData)
            {
                var values = ( (string[])row.Skip(2) ).Select(value => value.ToString()).ToList();
                var record = new Dictionary<string, object>
                {
                    {"date", date},
                    {"exchange", "TPEx"},
                    {"symbol", row[0].ToString()},
                    {"name", row[1].ToString().Trim()},
                    {"marginShortBalancePrev", decimal.Parse(values[0])},
                    {"marginShortSell", decimal.Parse(values[1])},
                    {"marginShortBuy", decimal.Parse(values[2])},
                    {"marginShortRedeem", decimal.Parse(values[3])},
                    {"marginShortBalance", decimal.Parse(values[4])},
                    {"marginShortQuota", decimal.Parse(values[5])},
                    {"sblShortBalancePrev", decimal.Parse(values[6])},
                    {"sblShortSale", decimal.Parse(values[7])},
                    {"sblShortReturn", decimal.Parse(values[8])},
                    {"sblShortAdjustment", decimal.Parse(values[9])},
                    {"sblShortBalance", decimal.Parse(values[10])},
                    {"sblShortQuota", decimal.Parse(values[11])},
                    {"note", values[12].Trim()}
                };
                data.Add(record);
            }

            return symbol != null ? data.Where(record => record["symbol"].ToString() == symbol).ToList() : data;
        }

        public async Task<List<Dictionary<string, object>>> FetchStocksValues(string date, string symbol = null)
        {
            var dateParts = date.Split('-');
            string queryDate = $"{int.Parse(dateParts[0]) - 1911}/{dateParts[1]}/{dateParts[2]}";
            string url = $"https://www.tpex.org.tw/web/stock/aftertrading/peratio_analysis/pera_result.php?d={queryDate}&o=json";

            var response = await httpClient.GetAsync(url);
            var json = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            if (json.iTotalRecords <= 0) return null;

            var data = new List<Dictionary<string, object>>();
            foreach (var row in json.aaData)
            {
                var values = ( (string[])row.Skip(2) ).Select(value => value.ToString()).ToList();
                var record = new Dictionary<string, object>
                {
                    {"date", date},
                    {"exchange", "TPEx"},
                    {"symbol", row[0].ToString()},
                    {"name", row[1].ToString().Trim()},
                    {"peRatio", decimal.Parse(values[0])},
                    {"pbRatio", decimal.Parse(values[4])},
                    {"dividendYield", decimal.Parse(values[3])},
                    {"dividendYear", int.Parse(values[2]) + 1911}
                };
                data.Add(record);
            }

            return symbol != null ? data.Where(record => record["symbol"].ToString() == symbol).ToList() : data;
        }

        public async Task<List<Dictionary<string, object>>> FetchIndicesHistorical(string date, string symbol = null)
        {
            var dateParts = date.Split('-');
            string queryDate = $"{int.Parse(dateParts[0]) - 1911}/{dateParts[1]}/{dateParts[2]}";
            string url = $"https://www.tpex.org.tw/web/stock/iNdex_info/minute_index/1MIN_print.php?d={queryDate}";

            var response = await httpClient.GetAsync(url);
            var page = await response.Content.ReadAsStringAsync();
            var document = new HtmlDocument();
            document.LoadHtml(page);

            var total = document.DocumentNode.SelectSingleNode("//tfoot//tr//td").InnerText.Trim();
            if (total == "共0筆") return null;

            var indices = document.DocumentNode.SelectNodes("//thead//tr//th").Skip(1).Take(7).Select(node =>
            {
                var name = node.InnerText.Trim();
                var index = name != "櫃買指數" ? $"櫃買{name.Replace("類", "")}類指數" : name;
                return new { Symbol = index.AsIndex(), Name = index };
            }).ToList();

            var quotes = document.DocumentNode.SelectNodes("//tbody//tr").SelectMany(row =>
            {
                var td = row.SelectNodes("td").Select(node => node.InnerText.Trim()).ToList();
                var time = td[0];
                var values = td.Skip(1).Take(7).ToList();
                return values.Select((value, i) => new
                {
                    Date = date,
                    Time = time,
                    Symbol = indices[i].Symbol,
                    Name = indices[i].Name,
                    Price = decimal.Parse(value)
                });
            }).ToList();

            var data = quotes.GroupBy(quote => quote.Symbol).Select(group =>
            {
                var quotes = group.ToList();
                var prev = quotes[0];
                var rows = quotes.Skip(1).ToList();
                var record = new Dictionary<string, object>
                {
                    {"date", date},
                    {"exchange", "TPEx"},
                    {"symbol", prev.Symbol},
                    {"name", prev.Name.Trim()},
                    {"open", rows.Min(row => row.Price)},
                    {"high", rows.Max(row => row.Price)},
                    {"low", rows.Min(row => row.Price)},
                    {"close", rows.Max(row => row.Price)},
                    {"change", rows.Max(row => row.Price) - prev.Price}
                };
                return record;
            }).ToList();

            return symbol != null ? data.Where(record => record["symbol"].ToString() == symbol).ToList() : data;
        }

        public async Task<List<Dictionary<string, object>>> FetchIndicesTrades(string date, string symbol = null)
        {
            var dateParts = date.Split('-');
            string queryDate = $"{int.Parse(dateParts[0]) - 1911}/{dateParts[1]}/{dateParts[2]}";
            string url = $"https://www.tpex.org.tw/web/stock/historical/trading_vol_ratio/sectr_result.php?d={queryDate}&o=json";

            var response = await httpClient.GetAsync(url);
            var json = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            if (json.iTotalRecords <= 0) return null;

            var data = ((IEnumerable<dynamic>)json.aaData).Select(values =>
            {
                var index = $"櫃買{values[0]}類指數";
                var record = new Dictionary<string, object>
                {
                    {"date", date},
                    {"exchange", "TPEx"},
                    {"symbol", index.AsIndex()},
                    {"name", index},
                    {"tradeVolume", decimal.Parse(values[3].ToString())},
                    {"tradeValue", decimal.Parse(values[1].ToString())},
                    {"tradeWeight", decimal.Parse(values[2].ToString())}
                };
                return record;
            }).ToList();

            var electronics = new[] { "IX0053", "IX0054", "IX0055", "IX0056", "IX0057", "IX0058", "IX0059", "IX0099" };

            var electronic = data.Where(record => electronics.Contains(record["symbol"].ToString())).GroupBy(record => "IX0047").Select(group =>
            {
                var records = group.ToList();
                var record = new Dictionary<string, object>
                {
                    {"date", date},
                    {"exchange", "TPEx"},
                    {"symbol", group.Key},
                    {"name", "櫃買電子類指數"},
                    {"tradeVolume", records.Sum(record => (decimal)record["tradeVolume"])},
                    {"tradeValue", records.Sum(record => (decimal)record["tradeValue"])},
                    {"tradeWeight", records.Sum(record => (decimal)record["tradeWeight"])}
                };
                return record;
            }).ToList();

            data.AddRange(electronic);
            data = data.Where(record => record["symbol"] != null).ToList();

            return symbol != null ? data.Where(record => record["symbol"].ToString() == symbol).ToList() : data;
        }

        public async Task<Dictionary<string, object>> FetchMarketTrades(string date)
        {
            var dateParts = date.Split('-');
            string queryDate = $"{int.Parse(dateParts[0]) - 1911}/{dateParts[1]}/{dateParts[2]}";
            string url = $"https://www.tpex.org.tw/web/stock/aftertrading/daily_trading_index/st41_result.php?d={queryDate}&o=json";

            var response = await httpClient.GetAsync(url);
            var json = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            if (json.iTotalRecords <= 0) return null;

            var data = ((IEnumerable<dynamic>)json.aaData).Select(row =>
            {
                var dateParts = row[0].ToString().Split('/');
                var recordDate = $"{int.Parse(dateParts[0]) + 1911}-{dateParts[1]}-{dateParts[2]}";
                var record = new Dictionary<string, object>
                {
                    {"date", recordDate},
                    {"exchange", "TPEx"},
                    {"tradeVolume", decimal.Parse(row[1].ToString())},
                    {"tradeValue", decimal.Parse(row[2].ToString())},
                    {"transaction", decimal.Parse(row[3].ToString())},
                    {"index", decimal.Parse(row[4].ToString())},
                    {"change", decimal.Parse(row[5].ToString())}
                };
                return record;
            }).ToList();

            return data.FirstOrDefault(record => record["date"].ToString() == date);
        }

        public async Task<Dictionary<string, object>> FetchMarketBreadth(string date)
        {
            var dateParts = date.Split('-');
            string queryDate = $"{int.Parse(dateParts[0]) - 1911}/{dateParts[1]}/{dateParts[2]}";
            string url = $"https://www.tpex.org.tw/web/stock/aftertrading/market_highlight/highlight_result.php?d={queryDate}&o=json";

            var response = await httpClient.GetAsync(url);
            var json = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            if (json.iTotalRecords <= 0) return null;

            var data = new Dictionary<string, object>
            {
                {"date", date},
                {"exchange", "TPEx"},
                {"up", decimal.Parse(json.upNum.ToString())},
                {"limitUp", decimal.Parse(json.upStopNum.ToString())},
                {"down", decimal.Parse(json.downNum.ToString())},
                {"limitDown", decimal.Parse(json.downStopNum.ToString())},
                {"unchanged", decimal.Parse(json.noChangeNum.ToString())},
                {"unmatched", decimal.Parse(json.noTradeNum.ToString())}
            };

            return data;
        }

        public async Task<Dictionary<string, object>> FetchMarketInstitutional(string date)
        {
            var dateParts = date.Split('-');
            string queryDate = $"{int.Parse(dateParts[0]) - 1911}/{dateParts[1]}/{dateParts[2]}";
            string url = $"https://www.tpex.org.tw/web/stock/3insti/3insti_summary/3itrdsum_result.php?d={queryDate}&t=D&o=json";

            var response = await httpClient.GetAsync(url);
            var json = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            if (json.iTotalRecords <= 0) return null;

            var data = new Dictionary<string, object>
            {
                {"date", date},
                {"exchange", "TPEx"},
                {"institutional", ((IEnumerable<dynamic>)json.aaData).Select(row => new Dictionary<string, object>
                {
                    {"investor", row[0].ToString().Trim()},
                    {"totalBuy", decimal.Parse(row[1].ToString())},
                    {"totalSell", decimal.Parse(row[2].ToString())},
                    {"difference", decimal.Parse(row[3].ToString())}
                }).ToList()}
            };

            return data;
        }

        public async Task<Dictionary<string, object>> FetchMarketMarginTrades(string date)
        {
            var dateParts = date.Split('-');
            string queryDate = $"{int.Parse(dateParts[0]) - 1911}/{dateParts[1]}/{dateParts[2]}";
            string url = $"https://www.tpex.org.tw/web/stock/margin_trading/margin_balance/margin_bal_result.php?d={queryDate}&o=json";

            var response = await httpClient.GetAsync(url);
            var json = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            if (json.iTotalRecords <= 0) return null;

            var values = ((IEnumerable<dynamic>)json.tfootData_one).Concat((IEnumerable<dynamic>)json.tfootData_two).ToList();
            var data = new Dictionary<string, object>
            {
                {"date", date},
                {"exchange", "TPEx"},
                {"marginBuy", decimal.Parse(values[1].ToString())},
                {"marginSell", decimal.Parse(values[2].ToString())},
                {"marginRedeem", decimal.Parse(values[3].ToString())},
                {"marginBalancePrev", decimal.Parse(values[0].ToString())},
                {"marginBalance", decimal.Parse(values[4].ToString())},
                {"shortBuy", decimal.Parse(values[10].ToString())},
                {"shortSell", decimal.Parse(values[9].ToString())},
                {"shortRedeem", decimal.Parse(values[11].ToString())},
                {"shortBalancePrev", decimal.Parse(values[8].ToString())},
                {"shortBalance", decimal.Parse(values[12].ToString())},
                {"marginBuyValue", decimal.Parse(values[14].ToString())},
                {"marginSellValue", decimal.Parse(values[15].ToString())},
                {"marginRedeemValue", decimal.Parse(values[16].ToString())},
                {"marginBalancePrevValue", decimal.Parse(values[13].ToString())},
                {"marginBalanceValue", decimal.Parse(values[17].ToString())}
            };

            return data;
        }
    }

    public class StocksHistoricalResponse
    {
        public string reportDate { get; set; }
        public string reportTitle { get; set; }
        public int iTotalRecords { get; set; }
        public int iTotalDisplayRecords { get; set; }
        public int colNum { get; set; }
        public string listNum { get; set; }
        public string totalAmount { get; set; }
        public string totalVolumn { get; set; }
        public string totalCount { get; set; }
        public List<object> mmData { get; set; }
        public List<List<string>> aaData { get; set; }
    }

    public class StocksHistoricalVM
    {
        public DateTime Date { get; set; }
        public string Exchange { get; set; }
        public string Symbol { get; set; }
        public string Name { get; set; }
        public decimal? Open { get; set; }
        public decimal? High { get; set; }
        public decimal? Low { get; set; }
        public decimal? Close { get; set; }
        public decimal? Volume { get; set; }
        public decimal? Turnover { get; set; }
        public decimal? Transaction { get; set; }
        public decimal? Change { get; set; }
    }
}
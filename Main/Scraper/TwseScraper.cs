using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TwStockNET.Interface;

namespace TwStockNET.Scraper
{
    public class TwseScraper : Scraper
    {
        private HttpClient httpClient = new HttpClient();

        public TwseScraper(IRateLimitOptions options = null) : base(options)
        {
        }

        public async Task<List<StocksHistoricalVM>> FetchStocksHistorical(IOption ticker)
        {
            var queryDate = $"{ticker.Date.Year - 1911}/{ticker.Date.Month.ToString().PadLeft(2, '0')}/{ticker.Date.Day.ToString().PadLeft(2, '0')}";
            var url = $"https://www.twse.com.tw/rwd/zh/afterTrading/MI_INDEX?date={queryDate}&type=ALLBUT0999&response=json";

            var response = await httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<TwseStockHistoricalResponse>(content);

            var targetTable = json.tables.FirstOrDefault(table => table.title.Contains("每日收盤行情"));
            var allData = targetTable.data;

            var result = allData.Select(row => new StocksHistoricalVM
            {
                Date = ticker.Date,
                Exchange = "TWSE",
                Symbol = row[0].Trim(),
                Name = row[1].Trim(),
                Open = decimal.TryParse(row[4], out var result4) ? result4 : (decimal?)null,
                High = decimal.TryParse(row[5], out var result5) ? result5 : (decimal?)null,
                Low = decimal.TryParse(row[6], out var result6) ? result6 : (decimal?)null,
                Close = decimal.TryParse(row[7], out var result7) ? result7 : (decimal?)null,
                Volume = decimal.TryParse(row[2], out var result2) ? result2 : (decimal?)null,
                Turnover = decimal.TryParse(row[3], out var result3) ? result3 : (decimal?)null,
                Transaction = decimal.TryParse(row[8], out var result8) ? result8 : (decimal?)null,
                Change =  decimal.TryParse(row[10], out var result10) ?
                    (row[9].Contains("green") ? result10 * -1 : result10) :
                    (decimal?)null
            }).ToList();

            return result;
        }

        public async Task<List<Dictionary<string, object>>> FetchStocksInstitutional(string date, string symbol = null)
        {
            string queryDate = DateTime.Parse(date).ToString("yyyyMMdd");
            string url = $"https://www.twse.com.tw/rwd/zh/fund/T86?date={queryDate}&selectType=ALLBUT0999&response=json";

            var response = await httpClient.GetAsync(url);
            var json = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            if (json.stat != "OK") return null;

            var data = ((IEnumerable<dynamic>)json.data).Select(row =>
            {
                var record = new Dictionary<string, object>
                {
                    {"date", date},
                    {"exchange", "TWSE"},
                    {"symbol", row[0].ToString()},
                    {"name", row[1].ToString().Trim()},
                    {"institutional", new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object>
                        {
                            {"investor", "外資及陸資(不含外資自營商)"},
                            {"totalBuy", decimal.Parse(row[2].ToString())},
                            {"totalSell", decimal.Parse(row[3].ToString())},
                            {"difference", decimal.Parse(row[4].ToString())}
                        },
                        new Dictionary<string, object>
                        {
                            {"investor", "外資自營商"},
                            {"totalBuy", decimal.Parse(row[5].ToString())},
                            {"totalSell", decimal.Parse(row[6].ToString())},
                            {"difference", decimal.Parse(row[7].ToString())}
                        },
                        new Dictionary<string, object>
                        {
                            {"investor", "投信"},
                            {"totalBuy", decimal.Parse(row[8].ToString())},
                            {"totalSell", decimal.Parse(row[9].ToString())},
                            {"difference", decimal.Parse(row[10].ToString())}
                        },
                        new Dictionary<string, object>
                        {
                            {"investor", "自營商合計"},
                            {"difference", decimal.Parse(row[11].ToString())}
                        },
                        new Dictionary<string, object>
                        {
                            {"investor", "自營商(自行買賣)"},
                            {"totalBuy", decimal.Parse(row[12].ToString())},
                            {"totalSell", decimal.Parse(row[13].ToString())},
                            {"difference", decimal.Parse(row[14].ToString())}
                        },
                        new Dictionary<string, object>
                        {
                            {"investor", "自營商(避險)"},
                            {"totalBuy", decimal.Parse(row[15].ToString())},
                            {"totalSell", decimal.Parse(row[16].ToString())},
                            {"difference", decimal.Parse(row[17].ToString())}
                        },
                        new Dictionary<string, object>
                        {
                            {"investor", "合計"},
                            {"difference", decimal.Parse(row[18].ToString())}
                        }
                    }}
                };
                return record;
            }).ToList();

            return symbol != null ? data.Where(record => record["symbol"].ToString() == symbol).ToList() : data;
        }

        public async Task<List<Dictionary<string, object>>> FetchStocksFiniHoldings(string date, string symbol = null)
        {
            string queryDate = DateTime.Parse(date).ToString("yyyyMMdd");
            string url = $"https://www.twse.com.tw/rwd/zh/fund/MI_QFIIS?date={queryDate}&selectType=ALLBUT0999&response=json";

            var response = await httpClient.GetAsync(url);
            var json = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            if (json.stat != "OK") return null;

            var data = ((IEnumerable<dynamic>)json.data).Select(row =>
            {
                var record = new Dictionary<string, object>
                {
                    {"date", date},
                    {"exchange", "TWSE"},
                    {"symbol", row[0].ToString()},
                    {"name", row[1].ToString().Trim()},
                    {"issuedShares", decimal.Parse(row[3].ToString())},
                    {"availableShares", decimal.Parse(row[4].ToString())},
                    {"sharesHeld", decimal.Parse(row[5].ToString())},
                    {"availablePercent", decimal.Parse(row[6].ToString())},
                    {"heldPercent", decimal.Parse(row[7].ToString())},
                    {"upperLimitPercent", decimal.Parse(row[8].ToString())}
                };
                return record;
            }).ToList();

            return symbol != null ? data.Where(record => record["symbol"].ToString() == symbol).ToList() : data;
        }

        public async Task<List<Dictionary<string, object>>> FetchStocksMarginTrades(string date, string symbol = null)
        {
            string queryDate = DateTime.Parse(date).ToString("yyyyMMdd");
            string url = $"https://www.twse.com.tw/rwd/zh/marginTrading/MI_MARGN?date={queryDate}&selectType=ALL&response=json";

            var response = await httpClient.GetAsync(url);
            var json = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            if (json.stat != "OK") return null;

            var data = ((IEnumerable<dynamic>)json.tables[1].data).Select(row =>
            {
                var record = new Dictionary<string, object>
                {
                    {"date", date},
                    {"exchange", "TWSE"},
                    {"symbol", row[0].ToString()},
                    {"name", row[1].ToString().Trim()},
                    {"marginBuy", decimal.Parse(row[2].ToString())},
                    {"marginSell", decimal.Parse(row[3].ToString())},
                    {"marginRedeem", decimal.Parse(row[4].ToString())},
                    {"marginBalancePrev", decimal.Parse(row[5].ToString())},
                    {"marginBalance", decimal.Parse(row[6].ToString())},
                    {"marginQuota", decimal.Parse(row[7].ToString())},
                    {"shortBuy", decimal.Parse(row[8].ToString())},
                    {"shortSell", decimal.Parse(row[9].ToString())},
                    {"shortRedeem", decimal.Parse(row[10].ToString())},
                    {"shortBalancePrev", decimal.Parse(row[11].ToString())},
                    {"shortBalance", decimal.Parse(row[12].ToString())},
                    {"shortQuota", decimal.Parse(row[13].ToString())},
                    {"offset", decimal.Parse(row[14].ToString())},
                    {"note", row[15].ToString().Trim()}
                };
                return record;
            }).ToList();

            return symbol != null ? data.Where(record => record["symbol"].ToString() == symbol).ToList() : data;
        }

        public async Task<List<Dictionary<string, object>>> FetchStocksShortSales(string date, string symbol = null)
        {
            string queryDate = DateTime.Parse(date).ToString("yyyyMMdd");
            string url = $"https://www.twse.com.tw/rwd/zh/marginTrading/TWT93U?date={queryDate}&response=json";

            var response = await httpClient.GetAsync(url);
            var json = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            if (json.stat != "OK" || !json.data.Any()) return null;

            var data = ((IEnumerable<dynamic>)json.data).Select(row =>
            {
                var record = new Dictionary<string, object>
                {
                    {"date", date},
                    {"exchange", "TWSE"},
                    {"symbol", row[0].ToString()},
                    {"name", row[1].ToString().Trim()},
                    {"marginShortBalancePrev", decimal.Parse(row[2].ToString())},
                    {"marginShortSell", decimal.Parse(row[3].ToString())},
                    {"marginShortBuy", decimal.Parse(row[4].ToString())},
                    {"marginShortRedeem", decimal.Parse(row[5].ToString())},
                    {"marginShortBalance", decimal.Parse(row[6].ToString())},
                    {"marginShortQuota", decimal.Parse(row[7].ToString())},
                    {"sblShortBalancePrev", decimal.Parse(row[8].ToString())},
                    {"sblShortSale", decimal.Parse(row[9].ToString())},
                    {"sblShortReturn", decimal.Parse(row[10].ToString())},
                    {"sblShortAdjustment", decimal.Parse(row[11].ToString())},
                    {"sblShortBalance", decimal.Parse(row[12].ToString())},
                    {"sblShortQuota", decimal.Parse(row[13].ToString())},
                    {"note", row[14].ToString().Trim()}
                };
                return record;
            }).ToList();

            return symbol != null ? data.Where(record => record["symbol"].ToString() == symbol).ToList() : data;
        }

        public async Task<List<Dictionary<string, object>>> FetchStocksValues(string date, string symbol = null)
        {
            string queryDate = DateTime.Parse(date).ToString("yyyyMMdd");
            string url = $"https://www.twse.com.tw/rwd/zh/afterTrading/BWIBBU_d?date={queryDate}&selectType=ALL&response=json";

            var response = await httpClient.GetAsync(url);
            var json = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            if (json.stat != "OK") return null;

            var data = ((IEnumerable<dynamic>)json.data).Select(row =>
            {
                var record = new Dictionary<string, object>
                {
                    {"date", date},
                    {"exchange", "TWSE"},
                    {"symbol", row[0].ToString()},
                    {"name", row[1].ToString().Trim()},
                    {"peRatio", decimal.Parse(row[3].ToString())},
                    {"pbRatio", decimal.Parse(row[4].ToString())},
                    {"dividendYield", decimal.Parse(row[2].ToString())},
                    {"dividendYear", decimal.Parse(row[3].ToString()) + 1911}
                };
                return record;
            }).ToList();

            return symbol != null ? data.Where(record => record["symbol"].ToString() == symbol).ToList() : data;
        }

        public async Task<List<Dictionary<string, object>>> FetchIndicesHistorical(string date, string symbol = null)
        {
            string queryDate = DateTime.Parse(date).ToString("yyyyMMdd");
            string url = $"https://www.twse.com.tw/rwd/zh/TAIEX/MI_5MINS_INDEX?date={queryDate}&response=json";

            var response = await httpClient.GetAsync(url);
            var json = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            if (json.stat != "OK") return null;

            var indices = ((IEnumerable<dynamic>)json.fields).Skip(1).Select(index => new { symbol = index.ToString(), name = index.ToString() }).ToList();

            var quotes = ((IEnumerable<dynamic>)json.data).SelectMany(row =>
            {
                var time = row[0];
                return ((IEnumerable<dynamic>)row).Skip(1).Select((value, i) => new
                {
                    date,
                    time,
                    symbol = indices[i].symbol,
                    name = indices[i].name,
                    price = decimal.Parse(value.ToString())
                });
            }).ToList();

            var data = quotes.GroupBy(q => q.symbol).Select(group =>
            {
                var quotesBySymbol = group.ToList();
                var prev = quotesBySymbol[0];
                var rows = quotesBySymbol.Skip(1).ToList();
                var record = new Dictionary<string, object>
                {
                    {"date", date},
                    {"exchange", "TWSE"},
                    {"symbol", group.Key},
                    {"name", prev.name.Trim()},
                    {"open", rows.Min(r => r.price)},
                    {"high", rows.Max(r => r.price)},
                    {"low", rows.Min(r => r.price)},
                    {"close", rows.Max(r => r.price)},
                    {"change", rows.Max(r => r.price) - prev.price}
                };
                return record;
            }).ToList();

            return symbol != null ? data.Where(record => record["symbol"].ToString() == symbol).ToList() : data;
        }

        public async Task<List<Dictionary<string, object>>> FetchIndicesTrades(string date, string symbol = null)
        {
            string queryDate = DateTime.Parse(date).ToString("yyyyMMdd");
            string url = $"https://www.twse.com.tw/rwd/zh/afterTrading/BFIAMU?date={queryDate}&response=json";

            var response = await httpClient.GetAsync(url);
            var json = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            if (json.stat != "OK") return null;

            var market = await FetchMarketTrades(date);
            if (market == null) return null;

            var data = ((IEnumerable<dynamic>)json.data).Select(row =>
            {
                var record = new Dictionary<string, object>
                {
                    {"date", date},
                    {"exchange", "TWSE"},
                    {"symbol", row[0].ToString().Trim()},
                    {"name", row[0].ToString().Trim()},
                    {"tradeVolume", decimal.Parse(row[1].ToString())},
                    {"tradeValue", decimal.Parse(row[2].ToString())},
                    {"tradeWeight", decimal.Parse(row[2].ToString()) / market["tradeValue"] * 100}
                };
                return record;
            }).ToList();

            return symbol != null ? data.Where(record => record["symbol"].ToString() == symbol).ToList() : data;
        }

        public async Task<Dictionary<string, object>> FetchMarketTrades(string date)
        {
            string queryDate = DateTime.Parse(date).ToString("yyyyMMdd");
            string url = $"https://www.twse.com.tw/rwd/zh/afterTrading/FMTQIK?date={queryDate}&response=json";

            var response = await httpClient.GetAsync(url);
            var json = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            if (json.stat != "OK") return null;

            var data = ((IEnumerable<dynamic>)json.data).Select(row =>
            {
                var dateParts = row[0].ToString().Split('/');
                var recordDate = new DateTime(int.Parse(dateParts[0]) + 1911, int.Parse(dateParts[1]), int.Parse(dateParts[2])).ToString("yyyy-MM-dd");
                var record = new Dictionary<string, object>
                {
                    {"date", recordDate},
                    {"exchange", "TWSE"},
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
            string queryDate = DateTime.Parse(date).ToString("yyyyMMdd");
            string url = $"https://www.twse.com.tw/rwd/zh/afterTrading/MI_INDEX?date={queryDate}&response=json";

            var response = await httpClient.GetAsync(url);
            var json = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            if (json.stat != "OK") return null;

            var raw = ((IEnumerable<dynamic>)json.tables[7].data).Select(row => row[2].ToString()).ToList();
            var up = raw[0].Replace(")", "").Split('(')[0];
            var limitUp = raw[0].Replace(")", "").Split('(')[1];
            var down = raw[1].Replace(")", "").Split('(')[0];
            var limitDown = raw[1].Replace(")", "").Split('(')[1];
            var unchanged = raw[2];
            var unmatched = raw[3];
            var notApplicable = raw[4];

            var data = new Dictionary<string, object>
            {
                {"date", date},
                {"exchange", "TWSE"},
                {"up", decimal.Parse(up)},
                {"limitUp", decimal.Parse(limitUp)},
                {"down", decimal.Parse(down)},
                {"limitDown", decimal.Parse(limitDown)},
                {"unchanged", decimal.Parse(unchanged)},
                {"unmatched", decimal.Parse(unmatched)},
                {"notApplicable", decimal.Parse(notApplicable)}
            };

            return data;
        }

        public async Task<Dictionary<string, object>> FetchMarketInstitutional(string date)
        {
            string queryDate = DateTime.Parse(date).ToString("yyyyMMdd");
            string url = $"https://www.twse.com.tw/rwd/zh/fund/BFI82U?dayDate={queryDate}&type=day&response=json";

            var response = await httpClient.GetAsync(url);
            var json = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            if (json.stat != "OK") return null;

            var data = new Dictionary<string, object>
            {
                {"date", date},
                {"exchange", "TWSE"},
                {"institutional", ((IEnumerable<dynamic>)json.data).Select(row => new Dictionary<string, object>
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
            string queryDate = DateTime.Parse(date).ToString("yyyyMMdd");
            string url = $"https://www.twse.com.tw/rwd/zh/marginTrading/MI_MARGN?date={queryDate}&selectType=MS&response=json";

            var response = await httpClient.GetAsync(url);
            var json = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            if (json.stat != "OK") return null;

            var values = ((IEnumerable<dynamic>)json.tables[0].data).SelectMany(row => ((IEnumerable<dynamic>)row).Skip(1).Select(value => value.ToString())).ToList();

            var data = new Dictionary<string, object>
            {
                {"date", date},
                {"exchange", "TWSE"},
                {"marginBuy", decimal.Parse(values[0])},
                {"marginSell", decimal.Parse(values[1])},
                {"marginRedeem", decimal.Parse(values[2])},
                {"marginBalancePrev", decimal.Parse(values[3])},
                {"marginBalance", decimal.Parse(values[4])},
                {"shortBuy", decimal.Parse(values[5])},
                {"shortSell", decimal.Parse(values[6])},
                {"shortRedeem", decimal.Parse(values[7])},
                {"shortBalancePrev", decimal.Parse(values[8])},
                {"shortBalance", decimal.Parse(values[9])},
                {"marginBuyValue", decimal.Parse(values[10])},
                {"marginSellValue", decimal.Parse(values[11])},
                {"marginRedeemValue", decimal.Parse(values[12])},
                {"marginBalancePrevValue", decimal.Parse(values[13])},
                {"marginBalanceValue", decimal.Parse(values[14])}
            };

            return data;
        }
    }


    public class TwseStockHistoricalResponse
    {
        public List<TwseStockHistoricalResponseTable> tables { get; set; }
    }

    public class TwseStockHistoricalResponseTable
    {
        public string title { get; set; }
        public List<string> fields { get; set; }
        public List<List<string>> data { get; set; }
    }
}
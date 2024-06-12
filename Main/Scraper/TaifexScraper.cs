using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CsvHelper;
using System.Globalization;
using TwStockNET.Interface;

namespace TwStockNET.Scraper
{
    public class TaifexScraper : Scraper
    {
        private HttpClient httpClient = new HttpClient();

        public TaifexScraper(IRateLimitOptions options = null) : base(options)
        {
        }

        public async Task<List<Dictionary<string, object>>> FetchListedStockFutOpt()
        {
            string url = "https://www.taifex.com.tw/cht/2/stockLists";

            var response = await httpClient.GetAsync(url);
            var html = await response.Content.ReadAsStringAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var list = new List<Dictionary<string, object>>();
            foreach (var row in doc.DocumentNode.SelectNodes("//table[@id='myTable']//tbody//tr"))
            {
                var td = row.SelectNodes("td").Select(node => node.InnerText.Trim()).ToList();
                var record = new Dictionary<string, object>
                {
                    {"symbol", td[0]},
                    {"underlyingStock", td[1]},
                    {"underlyingSymbol", td[2]},
                    {"underlyingName", td[3]},
                    {"hasFutures", td[4].Contains("是股票期貨標的")},
                    {"hasOptions", td[5].Contains("是股票選擇權標的")},
                    {"isTwseStock", td[6].Contains("是上市普通股標的證券")},
                    {"isTpexStock", td[7].Contains("是上櫃普通股標的證券")},
                    {"isTwseETF", td[8].Contains("是上市ETF標的證券")},
                    {"isTpexETF", td[9].Contains("是上櫃ETF標的證券")},
                    {"shares", decimal.Parse(td[10])}
                };
                list.Add(record);
            }

            var data = new List<Dictionary<string, object>>();
            foreach (var row in list)
            {
                bool isMicro = (decimal)row["shares"] == 100;
                var futures = new Dictionary<string, object>
                {
                    {"symbol", $"{row["symbol"]}F"},
                    {"name", $"{(isMicro ? "小型" : "")}{row["underlyingName"]}期貨"},
                    {"exchange", "TAIFEX"},
                    {"type", "股票期貨"},
                    {"underlyingSymbol", row["underlyingSymbol"]},
                    {"underlyingName", row["underlyingName"]}
                };
                var options = new Dictionary<string, object>
                {
                    {"symbol", $"{row["symbol"]}O"},
                    {"name", $"{(isMicro ? "小型" : "")}{row["underlyingName"]}選擇權"},
                    {"exchange", "TAIFEX"},
                    {"type", "股票選擇權"},
                    {"underlyingName", row["underlyingName"]},
                    {"underlying", row["underlyingSymbol"]}
                };
                if ((bool)row["hasFutures"]) data.Add(futures);
                if ((bool)row["hasOptions"]) data.Add(options);
            }

            return data;
        }
        
        public async Task<List<Dictionary<string, object>>> FetchFuturesHistorical(string date, string symbol = null, bool afterhours = false)
        {
            var alias = new Dictionary<string, string>
            {
                {"TX", "TXF"},  // 臺股期貨
                {"TE", "EXF"},  // 電子期貨
                {"TF", "FXF"},  // 金融期貨
                {"MTX", "MXF"}  // 小型臺指期貨
            };

            string queryDate = DateTime.Parse(date).ToString("yyyy/MM/dd");
            var form = new Dictionary<string, string>
            {
                {"down_type", "1"},
                {"queryStartDate", queryDate},
                {"queryEndDate", queryDate},
                {"commodity_id", "all"}
            };
            string url = "https://www.taifex.com.tw/cht/3/futDataDown";

            var content = new FormUrlEncodedContent(form);
            var response = await httpClient.PostAsync(url, content);
            var csv = Encoding.GetEncoding("big5").GetString(await response.Content.ReadAsByteArrayAsync());

            using var reader = new StringReader(csv);
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csvReader.GetRecords<dynamic>().ToList();

            if (!records.Any()) return null;

            var data = new List<Dictionary<string, object>>();
            foreach (var record in records)
            {
                var row = ((IDictionary<string, object>)record).Values.Select(value => value.ToString()).ToList();
                var recordData = new Dictionary<string, object>
                {
                    {"date", DateTime.ParseExact(row[0], "yyyy/MM/dd", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd")},
                    {"exchange", "TAIFEX"},
                    {"symbol", alias.ContainsKey(row[1]) ? alias[row[1]] : row[1]},
                    {"contractMonth", row[2]},
                    {"open", decimal.Parse(row[3])},
                    {"high", decimal.Parse(row[4])},
                    {"low", decimal.Parse(row[5])},
                    {"close", decimal.Parse(row[6])},
                    {"change", decimal.Parse(row[7])},
                    {"changePercent", decimal.Parse(row[8].Replace("%", ""))},
                    {"volume", decimal.Parse(row[9])},
                    {"settlementPrice", decimal.Parse(row[10])},
                    {"openInterest", decimal.Parse(row[11])},
                    {"bestBid", decimal.Parse(row[12])},
                    {"bestAsk", decimal.Parse(row[13])},
                    {"historicalHigh", decimal.Parse(row[14])},
                    {"historicalLow", decimal.Parse(row[15])},
                    {"session", row[16]},
                    {"volumeSpread", decimal.Parse(row[17])}
                };
                if (afterhours ? recordData["session"].ToString() == "盤後" : recordData["session"].ToString() == "一般")
                {
                    data.Add(recordData);
                }
            }

            return symbol != null ? data.Where(row => row["symbol"].ToString() == symbol).ToList() : data;
        }
        
        public async Task<List<Dictionary<string, object>>> FetchOptionsHistorical(string date, string symbol = null, bool afterhours = false)
        {
            string queryDate = DateTime.Parse(date).ToString("yyyy/MM/dd");
            var form = new Dictionary<string, string>
            {
                {"down_type", "1"},
                {"queryStartDate", queryDate},
                {"queryEndDate", queryDate},
                {"commodity_id", "all"}
            };
            string url = "https://www.taifex.com.tw/cht/3/optDataDown";

            var content = new FormUrlEncodedContent(form);
            var response = await httpClient.PostAsync(url, content);
            var csv = Encoding.GetEncoding("big5").GetString(await response.Content.ReadAsByteArrayAsync());

            using var reader = new StringReader(csv);
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csvReader.GetRecords<dynamic>().ToList();

            if (!records.Any()) return null;

            var data = new List<Dictionary<string, object>>();
            foreach (var record in records)
            {
                var row = ((IDictionary<string, object>)record).Values.Select(value => value.ToString()).ToList();
                var recordData = new Dictionary<string, object>
                {
                    {"date", DateTime.ParseExact(row[0], "yyyy/MM/dd", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd")},
                    {"exchange", "TAIFEX"},
                    {"symbol", row[1]},
                    {"contractMonth", row[2]},
                    {"strikePrice", decimal.Parse(row[3])},
                    {"type", row[4]},
                    {"open", decimal.Parse(row[5])},
                    {"high", decimal.Parse(row[6])},
                    {"low", decimal.Parse(row[7])},
                    {"close", decimal.Parse(row[8])},
                    {"volume", decimal.Parse(row[9])},
                    {"settlementPrice", decimal.Parse(row[10])},
                    {"openInterest", decimal.Parse(row[11])},
                    {"bestBid", decimal.Parse(row[12])},
                    {"bestAsk", decimal.Parse(row[13])},
                    {"historicalHigh", decimal.Parse(row[14])},
                    {"historicalLow", decimal.Parse(row[15])},
                    {"session", row[16]},
                    {"change", decimal.Parse(row[17])},
                    {"changePercent", decimal.Parse(row[18].Replace("%", ""))}
                };
                if (afterhours ? recordData["session"].ToString() == "盤後" : recordData["session"].ToString() == "一般")
                {
                    data.Add(recordData);
                }
            }

            return symbol != null ? data.Where(row => row["symbol"].ToString() == symbol).ToList() : data;
        }
        
        public async Task<Dictionary<string, object>> FetchFuturesInstitutional(string date, string symbol)
        {
            string queryDate = DateTime.Parse(date).ToString("yyyy/MM/dd");
            var form = new Dictionary<string, string>
            {
                {"queryStartDate", queryDate},
                {"queryEndDate", queryDate},
                {"commodityId", symbol}
            };
            string url = "https://www.taifex.com.tw/cht/3/futContractsDateDown";

            var content = new FormUrlEncodedContent(form);
            var response = await httpClient.PostAsync(url, content);
            var responseString = Encoding.GetEncoding("big5").GetString(await response.Content.ReadAsByteArrayAsync());

            if (responseString.Contains("查無資料")) return null;

            using var reader = new StringReader(responseString);
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csvReader.GetRecords<dynamic>().ToList();

            var data = new Dictionary<string, object>
            {
                {"date", date},
                {"exchange", "TAIFEX"},
                {"symbol", symbol},
                {"name", ((IDictionary<string, object>)records[0])["Field1"]},
                {"institutional", new List<Dictionary<string, object>>()}
            };

            foreach (var record in records.Skip(1))
            {
                var row = ((IDictionary<string, object>)record).Values.Select(value => value.ToString()).ToList();
                var institutional = new Dictionary<string, object>
                {
                    {"investor", row[1]},
                    {"longTradeVolume", decimal.Parse(row[2])},
                    {"longTradeValue", decimal.Parse(row[3])},
                    {"shortTradeVolume", decimal.Parse(row[4])},
                    {"shortTradeValue", decimal.Parse(row[5])},
                    {"netTradeVolume", decimal.Parse(row[6])},
                    {"netTradeValue", decimal.Parse(row[7])},
                    {"longOiVolume", decimal.Parse(row[8])},
                    {"longOiValue", decimal.Parse(row[9])},
                    {"shortOiVolume", decimal.Parse(row[10])},
                    {"shortOiValue", decimal.Parse(row[11])},
                    {"netOiVolume", decimal.Parse(row[12])},
                    {"netOiValue", decimal.Parse(row[13])}
                };
                ((List<Dictionary<string, object>>)data["institutional"]).Add(institutional);
            }

            return data;
        }
        
        public async Task<Dictionary<string, object>> FetchOptionsInstitutional(string date, string symbol)
        {
            string queryDate = DateTime.Parse(date).ToString("yyyy/MM/dd");
            var form = new Dictionary<string, string>
            {
                {"queryStartDate", queryDate},
                {"queryEndDate", queryDate},
                {"commodityId", symbol}
            };
            string url = "https://www.taifex.com.tw/cht/3/callsAndPutsDateDown";

            var content = new FormUrlEncodedContent(form);
            var response = await httpClient.PostAsync(url, content);
            var responseString = Encoding.GetEncoding("big5").GetString(await response.Content.ReadAsByteArrayAsync());

            if (responseString.Contains("查無資料")) return null;

            using var reader = new StringReader(responseString);
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csvReader.GetRecords<dynamic>().ToList();

            var data = new Dictionary<string, object>
            {
                {"date", date},
                {"exchange", "TAIFEX"},
                {"symbol", symbol},
                {"name", ((IDictionary<string, object>)records[0])["Field1"]},
                {"institutional", new List<Dictionary<string, object>>()}
            };

            foreach (var record in records.Skip(1))
            {
                var row = ((IDictionary<string, object>)record).Values.Select(value => value.ToString()).ToList();
                var institutional = new Dictionary<string, object>
                {
                    {"type", row[1]},
                    {"investor", row[2]},
                    {"longTradeVolume", decimal.Parse(row[3])},
                    {"longTradeValue", decimal.Parse(row[4])},
                    {"shortTradeVolume", decimal.Parse(row[5])},
                    {"shortTradeValue", decimal.Parse(row[6])},
                    {"netTradeVolume", decimal.Parse(row[7])},
                    {"netTradeValue", decimal.Parse(row[8])},
                    {"longOiVolume", decimal.Parse(row[9])},
                    {"longOiValue", decimal.Parse(row[10])},
                    {"shortOiVolume", decimal.Parse(row[11])},
                    {"shortOiValue", decimal.Parse(row[12])},
                    {"netOiVolume", decimal.Parse(row[13])},
                    {"netOiValue", decimal.Parse(row[14])}
                };
                ((List<Dictionary<string, object>>)data["institutional"]).Add(institutional);
            }

            return data;
        }
        
        public async Task<Dictionary<string, object>> FetchFuturesLargeTraders(string date, string symbol)
        {
            var alias = new Dictionary<string, string>
            {
                {"TXF", "TX"},  // 臺股期貨
                {"EXF", "TE"},  // 電子期貨
                {"FXF", "TF"}   // 金融期貨
            };

            string queryDate = DateTime.Parse(date).ToString("yyyy/MM/dd");
            var form = new Dictionary<string, string>
            {
                {"queryStartDate", queryDate},
                {"queryEndDate", queryDate}
            };
            string url = "https://www.taifex.com.tw/cht/3/largeTraderFutDown";

            var content = new FormUrlEncodedContent(form);
            var response = await httpClient.PostAsync(url, content);
            var responseString = Encoding.GetEncoding("big5").GetString(await response.Content.ReadAsByteArrayAsync());

            if (responseString.Contains("查無資料")) return null;

            using var reader = new StringReader(responseString);
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csvReader.GetRecords<dynamic>().ToList();

            var targetRows = records.Where(record =>
            {
                var row = ((IDictionary<string, object>)record).Values.Select(value => value.ToString()).ToList();
                return row[0] == (alias.ContainsKey(symbol) ? alias[symbol] : symbol) || row[0] == symbol.Substring(0, 2);
            }).ToList();

            var data = new Dictionary<string, object>
            {
                {"date", date},
                {"exchange", "TAIFEX"},
                {"symbol", symbol},
                {"name", ((IDictionary<string, object>)targetRows[0])["Field2"]},
                {"largeTraders", new List<Dictionary<string, object>>()}
            };

            foreach (var record in targetRows)
            {
                var row = ((IDictionary<string, object>)record).Values.Select(value => value.ToString()).ToList();
                var largeTraders = new Dictionary<string, object>
                {
                    {"contractMonth", row[2]},
                    {"traderType", row[3]},
                    {"topFiveLongOi", decimal.Parse(row[4])},
                    {"topFiveShortOi", decimal.Parse(row[5])},
                    {"topTenLongOi", decimal.Parse(row[6])},
                    {"topTenShortOi", decimal.Parse(row[7])},
                    {"marketOi", decimal.Parse(row[8])}
                };
                ((List<Dictionary<string, object>>)data["largeTraders"]).Add(largeTraders);
            }

            return data;
        }
        
        public async Task<Dictionary<string, object>> FetchOptionsLargeTraders(string date, string symbol)
        {
            string queryDate = DateTime.Parse(date).ToString("yyyy/MM/dd");
            var form = new Dictionary<string, string>
            {
                {"queryStartDate", queryDate},
                {"queryEndDate", queryDate}
            };
            string url = "https://www.taifex.com.tw/cht/3/largeTraderOptDown";

            var content = new FormUrlEncodedContent(form);
            var response = await httpClient.PostAsync(url, content);
            var responseString = Encoding.GetEncoding("big5").GetString(await response.Content.ReadAsByteArrayAsync());

            if (responseString.Contains("查無資料")) return null;

            using var reader = new StringReader(responseString);
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csvReader.GetRecords<dynamic>().ToList();

            var targetRows = records.Where(record =>
            {
                var row = ((IDictionary<string, object>)record).Values.Select(value => value.ToString()).ToList();
                return row[0] == symbol || row[0] == symbol.Substring(0, 2);
            }).ToList();

            var data = new Dictionary<string, object>
            {
                {"date", date},
                {"exchange", "TAIFEX"},
                {"symbol", symbol},
                {"name", ((IDictionary<string, object>)targetRows[0])["Field2"]},
                {"largeTraders", new List<Dictionary<string, object>>()}
            };

            foreach (var record in targetRows)
            {
                var row = ((IDictionary<string, object>)record).Values.Select(value => value.ToString()).ToList();
                var largeTraders = new Dictionary<string, object>
                {
                    {"type", row[2]},
                    {"contractMonth", row[3]},
                    {"traderType", row[4]},
                    {"topFiveLongOi", decimal.Parse(row[5])},
                    {"topFiveShortOi", decimal.Parse(row[6])},
                    {"topTenLongOi", decimal.Parse(row[7])},
                    {"topTenShortOi", decimal.Parse(row[8])},
                    {"marketOi", decimal.Parse(row[9])}
                };
                ((List<Dictionary<string, object>>)data["largeTraders"]).Add(largeTraders);
            }

            return data;
        }

        public async Task<Dictionary<string, object>> FetchMxfRetailPosition(string date)
        {
            var fetchedMxfHistorical = await FetchFuturesHistorical(date, "MXF");
            var fetchedMxfInstitutional = await FetchFuturesInstitutional(date, "MXF");

            if (fetchedMxfHistorical == null || fetchedMxfInstitutional == null) return null;

            decimal mxfMarketOi = fetchedMxfHistorical
                .Where(row => row["session"].ToString() == "一般" && !row.ContainsKey("volumeSpread"))
                .Sum(row => decimal.Parse(row["openInterest"].ToString()));

            var institutional = fetchedMxfInstitutional["institutional"] as List<Dictionary<string, object>>;
            decimal mxfInstitutionalLongOi = institutional.Sum(row => decimal.Parse(row["longOiVolume"].ToString()));
            decimal mxfInstitutionalShortOi = institutional.Sum(row => decimal.Parse(row["shortOiVolume"].ToString()));

            var data = new Dictionary<string, object>
            {
                {"date", date},
                {"mxfRetailLongOi", mxfMarketOi - mxfInstitutionalLongOi},
                {"mxfRetailShortOi", mxfMarketOi - mxfInstitutionalShortOi}
            };
            data["mxfRetailNetOi"] = (decimal)data["mxfRetailLongOi"] - (decimal)data["mxfRetailShortOi"];
            data["mxfRetailLongShortRatio"] = Math.Round((decimal)data["mxfRetailNetOi"] / mxfMarketOi, 4);

            return data;
        }

        public async Task<Dictionary<string, object>> FetchTxoPutCallRatio(string date)
        {
            string queryDate = DateTime.Parse(date).ToString("yyyy/MM/dd");
            var form = new Dictionary<string, string>
            {
                {"queryStartDate", queryDate},
                {"queryEndDate", queryDate}
            };
            string url = "https://www.taifex.com.tw/cht/3/pcRatioDown";

            var content = new FormUrlEncodedContent(form);
            var response = await httpClient.PostAsync(url, content);
            var responseString = Encoding.GetEncoding("big5").GetString(await response.Content.ReadAsByteArrayAsync());

            using var reader = new StringReader(responseString);
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csvReader.GetRecords<dynamic>().ToList();

            if (records.Count < 2) return null;

            var row = ((IDictionary<string, object>)records[1]).Values.Select(value => value.ToString()).ToList();

            var data = new Dictionary<string, object>
            {
                {"date", date},
                {"txoPutVolume", decimal.Parse(row[0])},
                {"txoCallVolume", decimal.Parse(row[1])},
                {"txoPutCallVolumeRatio", decimal.Parse(row[2]) / 100},
                {"txoPutOi", decimal.Parse(row[3])},
                {"txoCallOi", decimal.Parse(row[4])},
                {"txoPutCallOiRatio", decimal.Parse(row[5]) / 100}
            };

            return data;
        }
        
        public async Task<Dictionary<string, object>> FetchExchangeRates(string date)
        {
            string queryDate = DateTime.Parse(date).ToString("yyyy/MM/dd");
            var form = new Dictionary<string, string>
            {
                {"queryStartDate", queryDate},
                {"queryEndDate", queryDate}
            };
            string url = "https://www.taifex.com.tw/cht/3/dailyFXRateDown";

            var content = new FormUrlEncodedContent(form);
            var response = await httpClient.PostAsync(url, content);
            var responseString = Encoding.GetEncoding("big5").GetString(await response.Content.ReadAsByteArrayAsync());

            using var reader = new StringReader(responseString);
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csvReader.GetRecords<dynamic>().ToList();

            if (records.Count < 2) return null;

            var row = ((IDictionary<string, object>)records[1]).Values.Select(value => value.ToString()).ToList();

            var data = new Dictionary<string, object>
            {
                {"date", DateTime.ParseExact(row[0], "yyyy/MM/dd", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd")},
                {"usdtwd", decimal.Parse(row[1])},
                {"cnytwd", decimal.Parse(row[2])},
                {"eurusd", decimal.Parse(row[3])},
                {"usdjpy", decimal.Parse(row[4])},
                {"gbpusd", decimal.Parse(row[5])},
                {"audusd", decimal.Parse(row[6])},
                {"usdhkd", decimal.Parse(row[7])},
                {"usdcny", decimal.Parse(row[8])},
                {"usdzar", decimal.Parse(row[9])},
                {"nzdusd", decimal.Parse(row[10])}
            };

            return data;
        }
        
        
    }
}
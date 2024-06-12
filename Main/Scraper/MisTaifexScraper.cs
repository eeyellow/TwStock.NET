using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using HtmlAgilityPack;
using TwStockNET.Extensions;
using TwStockNET.Interface;
using Newtonsoft.Json;
using System.Text;
using TwStockNET.Enums;
using System.Globalization;

namespace TwStockNET.Scraper
{
    public class MisTaifexScraper : Scraper
    {
        private HttpClient httpClient = new HttpClient();

        public MisTaifexScraper(IRateLimitOptions options = null) : base(options)
        {
        }

        private string ExtractSymbolIdFromTicker(IOption ticker, bool afterhours = false)
        {
            string symbol = ticker.Symbol;
            string type = ticker.Type;

            if (type != null && type.Contains("期貨"))
            {
                return afterhours ? $"{symbol}-M" : $"{symbol}-F";
            }
            if (type != null && type.Contains("選擇權"))
            {
                return afterhours ? $"{symbol}-N" : $"{symbol}-O";
            }

            return null;
        }

        private string ExtractTypeFromCmdyDDLItem(ListedFutOptResponseItem item)
        {
            string type = item.CID.ToString()[2].ToString();
            if (type == "F" || type == "O")
            {
                return type;
            }
            if (item.DispCName.ToString().Contains("期貨"))
            {
                return "F";
            }
            if (item.DispCName.ToString().Contains("選擇權"))
            {
                return "O";
            }

            return null;
        }
        
        public async Task<List<ListedFutOptVM>> FetchListedFutOpt(string type)
        {
            string url = "https://mis.taifex.com.tw/futures/api/getCmdyDDLItemByKind";
            var body = new StringContent(JsonConvert.SerializeObject(new { SymbolType = type }), Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(url, body);
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<ListedFutOptResponse>(content);

            var data = new List<ListedFutOptVM>();
            foreach (var row in json.RtData.Items)
            {
                var record = new ListedFutOptVM
                {
                    Symbol = row.CID,
                    Name = row.DispCName,
                    Exchange = ExchangeEnum.TAIFEX,
                    Type = type ?? ExtractTypeFromCmdyDDLItem(row)
                };

                //if (row.SpotID != null)
                //{
                //    record.Add("underlying", row.SpotID);
                //}

                data.Add(record);
            }

            return data;
        }
        
        public async Task<List<Dictionary<string, object>>> FetchFutOptQuoteList(IOption ticker, bool afterhours = false)
        {
            string url = "https://mis.taifex.com.tw/futures/api/getQuoteList";
            var body = new StringContent(JsonConvert.SerializeObject(new { CID = ticker.Symbol, SymbolType = ticker.Type, MarketType = afterhours ? 1 : 0 }), Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(url, body);
            var json = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            if (json.RtCode != "0") return null;

            var data = new List<Dictionary<string, object>>();
            foreach (var row in json.RtData.QuoteList)
            {
                var record = new Dictionary<string, object>
                {
                    {"symbol", row.SymbolID.Split('-')[0]},
                    {"name", row.DispCName},
                    {"status", row.Status},
                    {"openPrice", row.COpenPrice != null ? decimal.Parse(row.COpenPrice) : (decimal?)null},

                    {"highPrice", row.CHighPrice != null ? decimal.Parse(row.CHighPrice) : (decimal?)null},
                    {"lowPrice", row.CLowPrice != null ? decimal.Parse(row.CLowPrice) : (decimal?)null},
                    {"lastPrice", row.CLastPrice != null ? decimal.Parse(row.CLastPrice) : (decimal?)null},
                    {"referencePrice", row.CRefPrice != null ? decimal.Parse(row.CRefPrice) : (decimal?)null},
                    {"limitUpPrice", row.CCeilPrice != null ? decimal.Parse(row.CCeilPrice) : (decimal?)null},
                    {"limitDownPrice", row.CFloorPrice != null ? decimal.Parse(row.CFloorPrice) : (decimal?)null},
                    {"settlementPrice", row.SettlementPrice != null ? decimal.Parse(row.SettlementPrice) : (decimal?)null},
                    {"change", row.CDiff != null ? decimal.Parse(row.CDiff) : (decimal?)null},
                    {"changePercent", row.CDiffRate != null ? decimal.Parse(row.CDiffRate) : (decimal?)null},
                    {"amplitude", row.CAmpRate != null ? decimal.Parse(row.CAmpRate) : (decimal?)null},
                    {"totalVoluem", row.CTotalVolume != null ? decimal.Parse(row.CTotalVolume) : (decimal?)null},
                    {"openInterest", row.OpenInterest != null ? decimal.Parse(row.OpenInterest) : (decimal?)null},
                    {"bestBidPrice", row.CBestBidPrice != null ? decimal.Parse(row.CBestBidPrice) : (decimal?)null},
                    {"bestAskPrice", row.CBestAskPrice != null ? decimal.Parse(row.CBestAskPrice) : (decimal?)null},
                    {"bestBidSize", row.CBestBidSize != null ? decimal.Parse(row.CBestBidSize) : (decimal?)null},
                    {"bestAskSize", row.CBestAskSize != null ? decimal.Parse(row.CBestAskSize) : (decimal?)null},
                    {"testPrice", row.CTestPrice != null ? decimal.Parse(row.CTestPrice) : (decimal?)null},
                    {"testSize", row.CTestVolume != null ? decimal.Parse(row.CTestVolume) : (decimal?)null},
                    {"lastUpdated", row.CTime != null ? DateTimeOffset.ParseExact($"{row.CDate} {row.CTime}", "yyyyMMdd hhmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToUnixTimeMilliseconds() : (long?)null}
                };
                data.Add(record);
            }

            return data;
        }
        
        public async Task<Dictionary<string, object>> FetchFutOptQuoteDetail(IOption ticker, bool afterhours = false)
        {
            string url = "https://mis.taifex.com.tw/futures/api/getQuoteDetail";
            var body = new StringContent(JsonConvert.SerializeObject(new { SymbolID = new List<string> { ExtractSymbolIdFromTicker(ticker, afterhours) } }), Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(url, body);
            var json = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            if (json.RtCode != "0") return null;

            var data = new List<Dictionary<string, object>>();
            foreach (var row in json.RtData.QuoteList)
            {
                var record = new Dictionary<string, object>
                {
                    {"symbol", ticker.Symbol},
                    {"name", row.DispCName},
                    {"status", row.Status},
                    {"referencePrice", row.CRefPrice != null ? decimal.Parse(row.CRefPrice) : (decimal?)null},
                    {"limitUpPrice", row.CCeilPrice != null ? decimal.Parse(row.CCeilPrice) : (decimal?)null},
                    {"limitDownPrice", row.CFloorPrice != null ? decimal.Parse(row.CFloorPrice) : (decimal?)null},
                    {"openPrice", row.COpenPrice != null ? decimal.Parse(row.COpenPrice) : (decimal?)null},
                    {"highPrice", row.CHighPrice != null ? decimal.Parse(row.CHighPrice) : (decimal?)null},
                    {"lowPrice", row.CLowPrice != null ? decimal.Parse(row.CLowPrice) : (decimal?)null},
                    {"lastPrice", row.CLastPrice != null ? decimal.Parse(row.CLastPrice) : (decimal?)null},
                    {"lastSize", row.CSingleVolume != null ? decimal.Parse(row.CSingleVolume) : (decimal?)null},
                    {"testPrice", row.CTestPrice != null ? decimal.Parse(row.CTestPrice) : (decimal?)null},
                    {"testSize", row.CTestVolume != null ? decimal.Parse(row.CTestVolume) : (decimal?)null},
                    {"testTime", row.CTestTime != null ? DateTimeOffset.ParseExact($"{row.CDate} {row.CTestTime}", "yyyyMMdd hhmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToUnixTimeMilliseconds() : (long?)null},
                    {"totalVoluem", row.CTotalVolume != null ? decimal.Parse(row.CTotalVolume) : (decimal?)null},
                    {"openInterest", row.OpenInterest != null ? decimal.Parse(row.OpenInterest) : (decimal?)null},
                    {"bidOrders", row.CBidCount != null ? decimal.Parse(row.CBidCount) : (decimal?)null},
                    {"askOrders", row.CAskCount != null ? decimal.Parse(row.CAskCount) : (decimal?)null},
                    {"bidVolume", row.CBidUnit != null ? decimal.Parse(row.CBidUnit) : (decimal?)null},
                    {"askVolume", row.CAskUnit != null ? decimal.Parse(row.CAskUnit) : (decimal?)null},
                    {"bidPrice", new List<string> { row.CBidPrice1, row.CBidPrice2, row.CBidPrice3, row.CBidPrice4, row.CBidPrice5 }.Select(price => price != null ? decimal.Parse(price) : (decimal?)null).ToList()},
                    {"askPrice", new List<string> { row.CAskPrice1, row.CAskPrice2, row.CAskPrice3, row.CAskPrice4, row.CAskPrice5 }.Select(price => price != null ? decimal.Parse(price) : (decimal?)null).ToList()},
                    {"bidSize", new List<string> { row.CBidSize1, row.CBidSize2, row.CBidSize3, row.CBidSize4, row.CBidSize5 }.Select(size => size != null ? decimal.Parse(size) : (decimal?)null).ToList()},
                    {"askSize", new List<string> { row.CAskSize1, row.CAskSize2, row.CAskSize3, row.CAskSize4, row.CAskSize5 }.Select(size => size != null ? decimal.Parse(size) : (decimal?)null).ToList()},
                    {"extBidPrice", row.CExtBidPrice != null ? decimal.Parse(row.CExtBidPrice) : (decimal?)null},
                    {"extAskPrice", row.CExtAskPrice != null ? decimal.Parse(row.CExtAskPrice) : (decimal?)null},
                    {"extBidSize", row.CExtBidSize != null ? decimal.Parse(row.CExtBidSize) : (decimal?)null},
                    {"extAskSize", row.CExtAskSize != null ? decimal.Parse(row.CExtAskSize) : (decimal?)null},
                    {"lastUpdated", row.CTime != null ? DateTimeOffset.ParseExact($"{row.CDate} {row.CTime}", "yyyyMMdd hhmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToUnixTimeMilliseconds() : (long?)null}
                };
                data.Add(record);
            }

            return data.FirstOrDefault(row => row["symbol"].ToString().Contains(ticker.Symbol));
        }
    }

    public class ListedFutOptResponseItem
    {
        public string SpotID { get; set; }
        public string DispEName { get; set; }
        public string DispCName { get; set; }
        public string CID { get; set; }
    }
    
    public class ListedFutOptResponseRtData
    {
        public List<ListedFutOptResponseItem> Items { get; set; }
    }

    public class ListedFutOptResponse
    {
        public string RtCode { get; set; }
        public string RtMsg { get; set; }
        public ListedFutOptResponseRtData RtData { get; set; }
    }

    public class ListedFutOptVM
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public ExchangeEnum Exchange { get; set; }
        public string Type { get; set; }
    }
}
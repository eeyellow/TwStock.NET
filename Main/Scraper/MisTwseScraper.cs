using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TwStockNET.Enums;
using TwStockNET.Extensions;
using TwStockNET.Interface;

namespace TwStockNET.Scraper
{
    public class MisTwseScraper : Scraper
    {
        private HttpClient httpClient = new HttpClient();

        public MisTwseScraper(IRateLimitOptions options = null) : base(options)
        {
        }

        public async Task<List<ListedIndicesVM>> FetchListedIndices(string exchange)
        {
            var ex = new Dictionary<string, string> { { "TWSE", "tse" }, { "TPEx", "otc" } };
            var i = new Dictionary<string, string> { { "TWSE", "TIDX" }, { "TPEx", "OIDX" } };
            var query = new Dictionary<string, string> { { "ex", ex[exchange] }, { "i", i[exchange] } };
            string url = $"https://mis.twse.com.tw/stock/api/getCategory.jsp?{string.Join("&", query.Select(kvp => $"{kvp.Key}={kvp.Value}"))}";

            var response = await httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<ListedIndicesResponse>(content);

            if (json.rtmessage != "OK") return null;

            var data = new List<ListedIndicesVM>();
            foreach (var row in json.msgArray)
            {
                var record = new ListedIndicesVM
                {
                    Symbol = row.n ?? row.ch.Replace(".tw", ""),
                    Name = row.n,
                    Exchange = exchange.AsExchange(),
                    ExCh = $"{row.ex}_{row.ch}"
                };
                
                data.Add(record);
            }

            return data;
        }
        
        public async Task<StocksQuoteVM> FetchStocksQuote(IOption ticker)
        {
            var query = new Dictionary<string, string> { { "ex_ch", ExtractExChFromTicker(ticker) } };
            string url = $"https://mis.twse.com.tw/stock/api/getStockInfo.jsp?{string.Join("&", query.Select(kvp => $"{kvp.Key}={kvp.Value}"))}";

            var response = await httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<StocksQuoteResponse>(content);

            if (json.rtmessage != "OK") return null;

            var data = new List<StocksQuoteVM>();
            foreach (var row in json.msgArray)
            {
                var record = new StocksQuoteVM
                {
                    Date = DateTime.ParseExact(row.d, "yyyyMMdd", CultureInfo.InvariantCulture),
                    Symbol = ticker.Symbol,
                    Name = row.n,
                    PreviousClose = row.y != null ? decimal.Parse(row.y) : (decimal?)null,
                    Open = row.o != null ? decimal.Parse(row.o) : (decimal?)null,
                    High = row.h != null ? decimal.Parse(row.h) : (decimal?)null,
                    Low = row.l != null ? decimal.Parse(row.l) : (decimal?)null,
                    Close = row.z != null ? decimal.Parse(row.z) : (decimal?)null,
                    Volume = row.v != null ? decimal.Parse(row.v) : (decimal?)null,
                    LastUpdated = row.tlong != null ? long.Parse(row.tlong) : (long?)null
                };

                data.Add(record);
            }

            return data.FirstOrDefault(row => row.Symbol == ticker.Symbol);
        }

        public async Task<IndicesQuoteVM> FetchIndicesQuote(IOption ticker)
        {
            var query = new Dictionary<string, string> { { "ex_ch", ExtractExChFromTicker(ticker) } };
            string url = ticker.Odd
                ? $"https://mis.twse.com.tw/stock/api/getOddInfo.jsp?{string.Join("&", query.Select(kvp => $"{kvp.Key}={kvp.Value}"))}"
                : $"https://mis.twse.com.tw/stock/api/getStockInfo.jsp?{string.Join("&", query.Select(kvp => $"{kvp.Key}={kvp.Value}"))}";

            var response = await httpClient.GetAsync(url);
            var json = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            if (json.rtmessage != "OK") return null;

            var data = new List<IndicesQuoteVM>();
            foreach (var row in json.msgArray)
            {
                var record = new IndicesQuoteVM
                {
                    Date = DateTime.ParseExact(row.d.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture),
                    Symbol = row.c,
                    Name = row.n,
                    ReferencePrice = row.y != null ? decimal.Parse(row.y.ToString()) : (decimal?)null,
                    LimitUpPrice = row.u != null ? decimal.Parse(row.u.ToString()) : (decimal?)null,
                    LimitDownPrice = row.w != null ? decimal.Parse(row.w.ToString()) : (decimal?)null,
                    OpenPrice = row.o != null ? decimal.Parse(row.o.ToString()) : (decimal?)null,
                    HighPrice = row.h != null ? decimal.Parse(row.h.ToString()) : (decimal?)null,
                    LowPrice = row.l != null ? decimal.Parse(row.l.ToString()) : (decimal?)null,
                    LastPrice = row.z != null ? decimal.Parse(row.z.ToString()) : (decimal?)null,
                    LastSize = row.s != null ? decimal.Parse(row.s.ToString()) : (decimal?)null,
                    TotalVolume = row.v != null ? decimal.Parse(row.v.ToString()) : (decimal?)null,
                    BidPrice = row.b != null ? ((string[])row.b.ToString().Split('_')).Where(s => !string.IsNullOrEmpty(s)).Select(s => decimal.Parse(s)).ToList() : new List<decimal>(),
                    AskPrice = row.a != null ? ((string[])row.a.ToString().Split('_')).Where(s => !string.IsNullOrEmpty(s)).Select(s => decimal.Parse(s)).ToList() : new List<decimal>(),
                    BidSize = row.g != null ? ((string[])row.g.ToString().Split('_')).Where(s => !string.IsNullOrEmpty(s)).Select(s => decimal.Parse(s)).ToList() : new List<decimal>(),
                    AskSize = row.f != null ? ((string[])row.f.ToString().Split('_')).Where(s => !string.IsNullOrEmpty(s)).Select(s => decimal.Parse(s)).ToList() : new List<decimal>(),
                    LastUpdated = row.tlong != null ? long.Parse(row.tlong.ToString()) : (long?)null
                };

                data.Add(record);
            }

            return data.FirstOrDefault(row => row.Symbol == ticker.Symbol);
        }
        private string ExtractExChFromTicker(IOption ticker)
        {
            var ex = new Dictionary<string, string> { { "TWSE", "tse" }, { "TPEx", "otc" } };
            string ch = $"{ticker.Symbol}.tw";
            return !string.IsNullOrEmpty(ticker.ExCh) ? ticker.ExCh : $"{ex[ticker.Exchange.ToString()]}_{ch}";
        }
    }

    public class ListedIndicesResponseMsgArray
    {
        public string ex { get; set; }
        public string ch { get; set; }
        public string nf { get; set; }
        public string key { get; set; }
        public string n { get; set; }
    }
    public class ListedIndicesResponseQueryTime
    {
        public int stockDetail { get; set; }
        public int totalMicroTime { get; set; }
    }
    public class ListedIndicesResponse
    {
        public List<ListedIndicesResponseMsgArray> msgArray { get; set; }
        public int size { get; set; }
        public string rtcode { get; set; }
        public ListedIndicesResponseQueryTime queryTime { get; set; }
        public string rtmessage { get; set; }
    }
    public class ListedIndicesVM
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public ExchangeEnum Exchange { get; set; }
        public string ExCh { get; set; }
    }


    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class StocksQuoteResponseMsgArray
    {
        public string tv { get; set; }
        public string ps { get; set; }
        public string nu { get; set; }
        public string pz { get; set; }
        public string bp { get; set; }
        public string a { get; set; }
        public string b { get; set; }
        public string c { get; set; }
        public string d { get; set; }
        public string ch { get; set; }
        public string tlong { get; set; }
        public string f { get; set; }
        public string ip { get; set; }
        public string g { get; set; }
        public string mt { get; set; }
        public string h { get; set; }
        public string it { get; set; }
        public string l { get; set; }
        public string n { get; set; }
        public string o { get; set; }
        public string p { get; set; }
        public string ex { get; set; }
        public string s { get; set; }
        public string t { get; set; }
        public string u { get; set; }
        public string v { get; set; }
        public string w { get; set; }
        public string nf { get; set; }
        public string y { get; set; }
        public string z { get; set; }
        public string ts { get; set; }
    }

    public class StocksQuoteResponseQueryTime
    {
        public string sysDate { get; set; }
        public int stockInfoItem { get; set; }
        public int stockInfo { get; set; }
        public string sessionStr { get; set; }
        public string sysTime { get; set; }
        public bool showChart { get; set; }
        public int sessionFromTime { get; set; }
        public int sessionLatestTime { get; set; }
    }

    public class StocksQuoteResponse
    {
        public List<StocksQuoteResponseMsgArray> msgArray { get; set; }
        public string referer { get; set; }
        public int userDelay { get; set; }
        public string rtcode { get; set; }
        public StocksQuoteResponseQueryTime queryTime { get; set; }
        public string rtmessage { get; set; }
    }

    public class StocksQuoteVM
    {
        public DateTime Date { get; set; }
        public string Symbol { get; set; }
        public string Name { get; set; }
        public decimal? PreviousClose { get; set; }
        public decimal? Open { get; set; }
        public decimal? High { get; set; }
        public decimal? Low { get; set; }
        public decimal? Close { get; set; }
        public decimal? Volume { get; set; }
        public long? LastUpdated { get; set; }
    }

    public class IndicesQuoteVM
    {
        public DateTime Date { get; set; }
        public string Symbol { get; set; }
        public string Name { get; set; }
        public decimal? ReferencePrice { get; set; }
        public decimal? LimitUpPrice { get; set; }
        public decimal? LimitDownPrice { get; set; }
        public decimal? OpenPrice { get; set; }
        public decimal? HighPrice { get; set; }
        public decimal? LowPrice { get; set; }
        public decimal? LastPrice { get; set; }
        public decimal? LastSize { get; set; }
        public decimal? TotalVolume { get; set; }
        public List<decimal> BidPrice { get; set; }
        public List<decimal> AskPrice { get; set; }
        public List<decimal> BidSize { get; set; }
        public List<decimal> AskSize { get; set; }
        public long? LastUpdated { get; set; }
    }
}
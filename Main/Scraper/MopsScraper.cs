using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TwStockNET.Interface;

namespace TwStockNET.Scraper
{
    public class MopsScraper : Scraper
    {
        private HttpClient httpClient = new HttpClient();

        public MopsScraper(IRateLimitOptions options = null) : base(options)
        {
        }

        public async Task<List<Dictionary<string, object>>> FetchStocksEps(string exchange, int year, int quarter, string symbol = null)
        {
            var type = new Dictionary<string, string> { { "TWSE", "sii" }, { "TPEx", "otc" } };
            var form = new Dictionary<string, string>
            {
                {"encodeURIComponent", "1"},
                {"step", "1"},
                {"firstin", "1"},
                {"off", "1"},
                {"isQuery", "Y"},
                {"TYPEK", type[exchange]},
                {"year", (year - 1911).ToString()},
                {"season", quarter.ToString("D2")}
            };
            string url = "https://mops.twse.com.tw/mops/web/t163sb04";

            var content = new FormUrlEncodedContent(form);
            var response = await httpClient.PostAsync(url, content);
            var html = await response.Content.ReadAsStringAsync();

            if (html.Contains("查詢無資料!")) return null;

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var data = new List<Dictionary<string, object>>();
            foreach (var row in doc.DocumentNode.SelectNodes("//tr[@class='even'] | //tr[@class='odd']"))
            {
                var td = row.SelectNodes("td").Select(node => node.InnerText.Trim()).ToList();
                var record = new Dictionary<string, object>
                {
                    {"exchange", exchange},
                    {"symbol", td[0]},
                    {"name", td[1]},
                    {"eps", decimal.Parse(td[td.Count - 1])},
                    {"year", year},
                    {"quarter", quarter}
                };
                data.Add(record);
            }

            return symbol != null ? data.Where(row => row["symbol"].ToString() == symbol).ToList() : data.OrderBy(row => row["symbol"]).ToList();
        }
        
        public async Task<List<Dictionary<string, object>>> FetchStocksRevenue(string exchange, int year, int month, bool foreign = false, string symbol = null)
        {
            var type = new Dictionary<string, string> { { "TWSE", "sii" }, { "TPEx", "otc" } };
            string suffix = $"{year - 1911}_{month}_{Convert.ToInt32(foreign)}";
            string url = $"https://mops.twse.com.tw/nas/t21/{type[exchange]}/t21sc03_{suffix}.html";

            var response = await httpClient.GetAsync(url);
            var page = Encoding.GetEncoding("big5").GetString(await response.Content.ReadAsByteArrayAsync());

            if (page.Contains("查無資料")) return null;

            var doc = new HtmlDocument();
            doc.LoadHtml(page);

            var data = new List<Dictionary<string, object>>();
            foreach (var row in doc.DocumentNode.SelectNodes("//tr[td[@align='right']]"))
            {
                var td = row.SelectNodes("td").Select(node => node.InnerText.Trim()).ToList();
                if (string.IsNullOrEmpty(td[0])) continue;

                var record = new Dictionary<string, object>
                {
                    {"exchange", exchange},
                    {"symbol", td[0]},
                    {"name", td[1]},
                    {"revenue", decimal.Parse(td[2])},
                    {"year", year},
                    {"month", month}
                };
                data.Add(record);
            }

            return symbol != null ? data.Where(row => row["symbol"].ToString() == symbol).ToList() : data.OrderBy(row => row["symbol"]).ToList();
        }
    }
}
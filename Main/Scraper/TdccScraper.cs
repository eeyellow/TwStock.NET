using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TwStockNET.Interface;

namespace TwStockNET.Scraper
{
    public class TdccScraper : Scraper
    {
        private HttpClient httpClient = new HttpClient();

        public TdccScraper(IRateLimitOptions options = null) : base(options)
        {
        }

        public async Task<Dictionary<string, object>> FetchStocksShareholders(string date, string symbol)
        {
            string url = "https://www.tdcc.com.tw/portal/zh/smWeb/qryStock";
            var request = await httpClient.GetAsync(url);
            var document = new HtmlDocument();
            document.LoadHtml(await request.Content.ReadAsStringAsync());

            var token = document.DocumentNode.SelectSingleNode("//input[@id='SYNCHRONIZER_TOKEN']").GetAttributeValue("value", "");
            var uri = document.DocumentNode.SelectSingleNode("//input[@id='SYNCHRONIZER_URI']").GetAttributeValue("value", "");
            var method = document.DocumentNode.SelectSingleNode("//input[@id='method']").GetAttributeValue("value", "");
            var firDate = document.DocumentNode.SelectSingleNode("//input[@id='firDate']").GetAttributeValue("value", "");
            var scaDate = DateTime.Parse(date).ToString("yyyyMMdd");
            var cookie = request.Headers.GetValues("Set-Cookie").First();

            var form = new Dictionary<string, string>
            {
                {"SYNCHRONIZER_TOKEN", token},
                {"SYNCHRONIZER_URI", uri},
                {"method", method},
                {"firDate", firDate},
                {"scaDate", scaDate},
                {"sqlMethod", "StockNo"},
                {"stockNo", symbol},
                {"stockName", ""}
            };
            var content = new FormUrlEncodedContent(form);
            var response = await httpClient.PostAsync(url, content);
            document.LoadHtml(await response.Content.ReadAsStringAsync());

            var message = document.DocumentNode.SelectSingleNode("//table//tr//td").InnerText;
            if (message == "查無此資料") return null;

            var info = document.DocumentNode.SelectSingleNode("//p").InnerText;
            var data = new Dictionary<string, object>
            {
                {"date", date},
                {"symbol", Regex.Match(info, "證券代號：(\\S+)").Groups[1].Value},
                {"name", Regex.Match(info, "證券名稱：(.+)").Groups[1].Value},
                {"shareholders", new List<Dictionary<string, object>>()}
            };

            var rows = document.DocumentNode.SelectNodes("//table//tr").Skip(1);
            foreach (var row in rows)
            {
                var td = row.SelectNodes("td").Select(node => node.InnerText).ToList();
                var shareholder = new Dictionary<string, object>
                {
                    {"level", td[0].Trim()},
                    {"holders", decimal.Parse(td[1])},
                    {"shares", decimal.Parse(td[2])},
                    {"proportion", decimal.Parse(td[3])}
                };
                ((List<Dictionary<string, object>>)data["shareholders"]).Add(shareholder);
            }

            return data;
        }
    }
}
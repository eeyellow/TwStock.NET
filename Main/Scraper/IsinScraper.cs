using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TwStockNET.Enums;
using TwStockNET.Extensions;
using TwStockNET.Interface;

namespace TwStockNET.Scraper
{
    public class IsinScraper : Scraper
    {
        private HttpClient httpClient = new HttpClient();

        public IsinScraper(IRateLimitOptions options = null) : base(options)
        {
        }

        public async Task<List<IsinScraperVM>> FetchListed(string symbol)
        {
            string url = $"https://isin.twse.com.tw/isin/single_main.jsp?owncode={symbol}";
            var response = await httpClient.GetByteArrayAsync(url);
            var page = Encoding.GetEncoding(950).GetString(response);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(page);

            var data = new List<IsinScraperVM>();
            foreach (var row in htmlDocument.DocumentNode.SelectNodes("//tr"))
            {
                var cells = row.SelectNodes("td");
                if (cells != null)
                {
                    try
                    {
                        var record = new IsinScraperVM
                        {
                            Symbol = cells[2].InnerText.Trim(),
                            Name = cells[3].InnerText.Trim(),
                            Exchange = cells[4].InnerText.Trim().AsExchange(),
                            Type = cells[5].InnerText.Trim(),
                            Industry = cells[6].InnerText.Trim().AsIndustry(),
                            ListedDate = DateTime.ParseExact(cells[7].InnerText.Trim(), "yyyy/MM/dd", null)
                        };
                        
                        data.Add(record);
                    }
                    catch(Exception e)
                    {
                        
                    }
                }
            }

            return data;
        }

        private async Task<List<IsinScraperVM>> _FetchListedStocks(string url)
        {
            var response = await httpClient.GetByteArrayAsync(url);
            var page = Encoding.GetEncoding(950).GetString(response);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(page);

            var data = new List<IsinScraperVM>();
            foreach (var row in htmlDocument.DocumentNode.SelectNodes("//tr").Skip(1))
            {
                var cells = row.SelectNodes("td");
                if (cells != null)
                {
                    var record = new IsinScraperVM
                    {
                        Symbol = cells[2].InnerText.Trim(),
                        Name = cells[3].InnerText.Trim(),
                        Exchange = cells[4].InnerText.Trim().AsExchange(),
                        Type = cells[5].InnerText.Trim(),
                        Industry = cells[6].InnerText.Trim().AsIndustry(),
                        ListedDate = DateTime.ParseExact(cells[7].InnerText.Trim(), "yyyy/MM/dd", null)
                    };

                    data.Add(record);
                }
            }

            return data;
        }

        public async Task<List<IsinScraperVM>> FetchListedStocks(ExchangeEnum exchange)
        {
            var data = new List<IsinScraperVM>();
            var url = "";
            switch (exchange)
            {
                case ExchangeEnum.TWSE:
                    url = "https://isin.twse.com.tw/isin/class_main.jsp?market=1";
                    data.AddRange(await _FetchListedStocks(url));                    
                    break;
                case ExchangeEnum.TPEx:
                    url = "https://isin.twse.com.tw/isin/class_main.jsp?market=2";
                    data.AddRange(await _FetchListedStocks(url));
                    break;
                default:
                    url = "https://isin.twse.com.tw/isin/class_main.jsp?market=1";
                    data.AddRange(await _FetchListedStocks(url));
                    url = "https://isin.twse.com.tw/isin/class_main.jsp?market=2";
                    data.AddRange(await _FetchListedStocks(url));
                    break;
            }

            return data;
        }
        
        public async Task<List<IsinScraperVM>> FetchListedFutOpt(string type = null)
        {
            string url = "https://isin.twse.com.tw/isin/class_main.jsp?market=7";
            string page = await httpClient.GetStringAsync(url);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(page);

            var data = new List<IsinScraperVM>();
            foreach (var row in htmlDocument.DocumentNode.SelectNodes("//tr"))
            {
                var cells = row.SelectNodes("td");
                if (cells != null)
                {
                    var record = new IsinScraperVM
                    {
                        Symbol = cells[2].InnerText.Trim(),
                        Name = cells[3].InnerText.Trim(),
                        Exchange = cells[4].InnerText.Trim().AsExchange(),
                        Type = cells[5].InnerText.Trim(),
                        ListedDate = DateTime.ParseExact(cells[7].InnerText.Trim(), "yyyy/MM/dd", null)
                    };

                    data.Add(record);
                }
            }

            if (type == "F")
            {
                data = data.Where(row => row.Type.Contains("´Á³f")).ToList();
            }
            else if (type == "O")
            {
                data = data.Where(row => row.Type.Contains("¿ï¾ÜÅv")).ToList();
            }

            return data;
        }
    }

    public class IsinScraperVM
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public ExchangeEnum Exchange { get; set; }
        public string Type { get; set; }
        public IndustryEnum Industry { get; set; }
        public DateTime ListedDate { get; set; }
    }
}
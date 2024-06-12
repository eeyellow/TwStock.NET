using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwStockNET.Enums;
using TwStockNET.Interface;
using TwStockNET.Scraper;

namespace TwStockNET
{
    public class TwStock
    {
        private readonly ScraperFactory scraper;
        private readonly Dictionary<string, IOption> stocks = new Dictionary<string, IOption>();
        private readonly Dictionary<string, IOption> indices = new Dictionary<string, IOption>();
        private readonly Dictionary<string, IOption> futopt = new Dictionary<string, IOption>();

        public TwStock(IRateLimitOptions options = null)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            scraper = new ScraperFactory(options);
        }

        public async Task<List<IsinScraperVM>> LoadStocks(IOption ticker)
        {
            var isinScraper = scraper.GetIsinScraper();


            var results = new List<IsinScraperVM>();

            if(!string.IsNullOrWhiteSpace(ticker.Symbol))
            {
                results.AddRange(await isinScraper.FetchListed(ticker.Symbol));
            }
            else
            {
                results.AddRange(await isinScraper.FetchListedStocks(ticker.Exchange));
            }

            return results;
        }

        public async Task<List<ListedIndicesVM>> LoadIndices(IOption ticker)
        {
            var misScraper = scraper.GetMisTwseScraper();

            var results = new List<ListedIndicesVM>();

            results.AddRange(await misScraper.FetchListedIndices(ExchangeEnum.TWSE.ToString()));
            results.AddRange(await misScraper.FetchListedIndices(ExchangeEnum.TPEx.ToString()));

            return results;
        }

        public async Task<List<ListedFutOptVM>> LoadFutOpt(IOption ticker)
        {
            var misTaifexScraper = scraper.GetMisTaifexScraper();

            List<ListedFutOptVM> futopt = new List<ListedFutOptVM>();
            switch (ticker.Type)
            {
                case "F":
                    futopt.AddRange(await misTaifexScraper.FetchListedFutOpt("F"));
                    break;
                case "O":
                    futopt.AddRange(await misTaifexScraper.FetchListedFutOpt("O"));
                    break;
                default:
                    futopt.AddRange(await misTaifexScraper.FetchListedFutOpt("F"));
                    futopt.AddRange(await misTaifexScraper.FetchListedFutOpt("O"));
                    break;
            }

            return futopt;
        }

        public async Task<List<IsinScraperVM>> LoadFutOptContracts(IOption ticker)
        {
            var symbol = ticker.Symbol;
            var type = ticker.Type;
            var isinScraper = scraper.GetIsinScraper();

            var futopt = (symbol != null)
                ? await isinScraper.FetchListed(symbol)
                : await isinScraper.FetchListedFutOpt(type);

            return futopt;
        }

        public async Task<List<IsinScraperVM>> FetchStocksList(IOption ticker)
        {
            var exchange = ticker.Exchange;
            var data = await LoadStocks(ticker);
            return exchange != null ? 
                data.Where(ticker => ticker.Exchange == exchange).ToList() : 
                data;
        }

        public async Task<StocksQuoteVM> FetchStocksQuote(IOption ticker)
        {
            return await scraper.GetMisTwseScraper().FetchStocksQuote(ticker);
        }

        public async Task<List<StocksHistoricalVM>> FetchStocksHistorical(IOption ticker)
        {
            var result = new List<StocksHistoricalVM>();
            switch (ticker.Exchange)
            {
                case ExchangeEnum.TWSE:
                    result = await scraper.GetTwseScraper().FetchStocksHistorical(ticker);
                    break;
                case ExchangeEnum.TPEx:
                    result = await scraper.GetTpexScraper().FetchStocksHistorical(ticker);
                    break;
            }
            return result;
        }
    }
}
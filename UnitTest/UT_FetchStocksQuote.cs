using TwStockNET.Enums;
using TwStockNET.Interface;

namespace TwStockUnitTest
{
    public class UT_FetchStocksQuote
    {
        private static RateLimitOptions _option = new RateLimitOptions { Limit = 10, Ttl = 1000 };

        [Fact]
        public async Task FetchStocksQuote_01()
        {
            var twStock = new TwStockNET.TwStock(_option);
            var ticker = new Option { Symbol = "0050" };
            var data = await twStock.FetchStocksQuote(ticker);


            Assert.Equal("元大台灣50", data.Name);
        }
    }
}
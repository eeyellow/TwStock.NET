using TwStockNET.Enums;
using TwStockNET.Interface;

namespace TwStockUnitTest
{
    public class UT_FetchStocksList
    {
        private static RateLimitOptions _option = new RateLimitOptions { Limit = 10, Ttl = 1000 };

        [Fact]
        public async Task FetchStocksList_01()
        {
            var twStock = new TwStockNET.TwStock(_option);
            var ticker = new Option();
            var data = await twStock.FetchStocksList(ticker);

            var testData = data.FirstOrDefault(d => d.Symbol == "0050");

            Assert.Equal("元大台灣50", testData?.Name ?? "");
        }
    }
}
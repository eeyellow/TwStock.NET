using TwStockNET.Enums;
using TwStockNET.Interface;

namespace TwStockUnitTest
{
    public class UT_FetchStocksHistorical
    {
        private static RateLimitOptions _option = new RateLimitOptions { Limit = 10, Ttl = 1000 };

        [Fact]
        public async Task FetchStocksHistorical_01()
        {
            var twStock = new TwStockNET.TwStock(_option);
            var ticker = new Option
            {
                Exchange = ExchangeEnum.TPEx,
                Date = new DateTime(2024, 6, 11),
            };

            var data = await twStock.FetchStocksHistorical(ticker);

            var testData = data.FirstOrDefault(a => a.Symbol == "006201");

            Assert.Equal("元大富櫃50", testData?.Name ?? "");
        }

        [Fact]
        public async Task FetchStocksHistorical_02()
        {
            var twStock = new TwStockNET.TwStock(_option);
            var ticker = new Option
            {
                Exchange = ExchangeEnum.TWSE,
                Date = new DateTime(2024, 6, 11),
            };

            var data = await twStock.FetchStocksHistorical(ticker);

            var testData = data.FirstOrDefault(a => a.Symbol == "0050");

            Assert.Equal("元大台灣50", testData?.Name ?? "");
        }
    }

}
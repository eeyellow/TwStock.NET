using TwStockNET.Enums;
using TwStockNET.Interface;

namespace TwStockUnitTest
{
    public class UT_LoadFutOpt
    {
        private static RateLimitOptions _option = new RateLimitOptions { Limit = 10, Ttl = 1000 };

        [Fact]
        public async Task LoadFutOpt_01()
        {
            var twStock = new TwStockNET.TwStock(_option);
            var ticker = new Option();
            var data = await twStock.LoadFutOpt(ticker);

            var testData = data.FirstOrDefault(d => d.Symbol == "TXF");
            Assert.Equal("»O«ü´Á", testData?.Name ?? "");
        }
    }
}
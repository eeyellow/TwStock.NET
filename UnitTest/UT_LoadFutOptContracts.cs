using TwStockNET.Enums;
using TwStockNET.Interface;

namespace TwStockUnitTest
{
    public class UT_LoadFutOptContracts
    {
        private static RateLimitOptions _option = new RateLimitOptions { Limit = 10, Ttl = 1000 };

        [Fact]
        public async Task LoadFutOptContracts_01()
        {
            var twStock = new TwStockNET.TwStock(_option);
            var ticker = new Option { Symbol = "2330" };
            var data = await twStock.LoadFutOptContracts(ticker);
            Assert.Single(data);
            Assert.Equal("¥x¿n¹q", data.First().Name);
        }
    }
}
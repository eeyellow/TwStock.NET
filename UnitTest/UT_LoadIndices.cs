using TwStockNET.Enums;
using TwStockNET.Interface;

namespace TwStockUnitTest
{
    public class UT_LoadIndices
    {
        private static RateLimitOptions _option = new RateLimitOptions { Limit = 10, Ttl = 1000 };

        [Fact]
        public async Task LoadIndices_01()
        {
            var twStock = new TwStockNET.TwStock(_option);
            var ticker = new Option();
            var data = await twStock.LoadIndices(ticker);

            var testData = data.FirstOrDefault(d => d.ExCh == "tse_t00.tw");
            Assert.Equal("發行量加權股價指數", testData?.Name ?? "");
        }
    }
}
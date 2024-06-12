using System.Net.Http;
using TwStockNET.Interface;

namespace TwStockNET.Scraper
{
    public class Scraper
    {
        protected readonly HttpClient httpService;

        public Scraper(IRateLimitOptions options = null)
        {
            var maxRequests = options.Limit;
            var perMilliseconds = options.Ttl;

            // C# does not have a built-in rate limiter for HttpClient.
            // You may need to implement your own or use a third-party library.
            // Here we just create a new HttpClient instance.
            this.httpService = new HttpClient();
        }
    }

}
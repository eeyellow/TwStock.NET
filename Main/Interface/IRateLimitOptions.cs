namespace TwStockNET.Interface
{
    public interface IRateLimitOptions
    {
        int Ttl { get; set; }
        int Limit { get; set; }
    }

    public class RateLimitOptions : IRateLimitOptions
    {
        public int Ttl { get; set; }
        public int Limit { get; set; }
    }
}

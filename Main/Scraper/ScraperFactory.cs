using System;
using System.Collections.Generic;
using TwStockNET.Enums;
using TwStockNET.Interface;

namespace TwStockNET.Scraper
{
    public class ScraperFactory
    {
        private readonly Dictionary<string, Scraper> scrapers = new Dictionary<string, Scraper>();
        private readonly IRateLimitOptions options;

        public ScraperFactory(IRateLimitOptions options)
        {
            this.options = options;
        }

        public Scraper Get(ScraperEnum type)
        {
            if (!scrapers.ContainsKey(type.ToString()))
            {
                Scraper scraper = type switch
                {
                    ScraperEnum.Twse => new TwseScraper(options),
                    ScraperEnum.Tpex => new TpexScraper(options),
                    ScraperEnum.Mops => new MopsScraper(options),
                    ScraperEnum.MisTaifex => new MisTaifexScraper(options),
                    ScraperEnum.MisTwse => new MisTwseScraper(options),
                    ScraperEnum.Tdcc => new TdccScraper(options),
                    ScraperEnum.Isin => new IsinScraper(options),
                    ScraperEnum.Taifex => new TaifexScraper(options),

                    _ => throw new ArgumentException($"Invalid scraper type: {type}")
                };
                scrapers[type.ToString()] = scraper;
            }

            return scrapers[type.ToString()];
        }

        public TwseScraper GetTwseScraper()
        {
            return (TwseScraper)Get(ScraperEnum.Twse);
        }

        public TpexScraper GetTpexScraper()
        {
            return (TpexScraper)Get(ScraperEnum.Tpex);
        }

        public MopsScraper GetMopsScraper()
        {
            return (MopsScraper)Get(ScraperEnum.Mops);
        }

        public MisTaifexScraper GetMisTaifexScraper()
        {
            return (MisTaifexScraper)Get(ScraperEnum.MisTaifex);
        }

        public MisTwseScraper GetMisTwseScraper()
        {
            return (MisTwseScraper)Get(ScraperEnum.MisTwse);
        }

        public TdccScraper GetTdccScraper()
        {
            return (TdccScraper)Get(ScraperEnum.Tdcc);
        }

        public IsinScraper GetIsinScraper()
        {
            return (IsinScraper)Get(ScraperEnum.Isin);
        }

        public TaifexScraper GetTaifexScraper()
        {
            return (TaifexScraper)Get(ScraperEnum.Taifex);
        }
    }
}
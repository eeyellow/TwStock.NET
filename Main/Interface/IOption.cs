using System;
using TwStockNET.Enums;

namespace TwStockNET.Interface
{
    public interface IOption
    {
        string Symbol { get; set; }
        string Name { get; set; }
        ExchangeEnum Exchange { get; set; }
        MarketEnum Market { get; set; }
        string Type { get; set; }
        IndustryEnum? Industry { get; set; }
        string ListedDate { get; set; }
        string ExCh { get; set; }
        bool Odd { get; set; }
        DateTime Date { get; set; }
    }

    public class Option : IOption
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public ExchangeEnum Exchange { get; set; }
        public MarketEnum Market { get; set; }
        public string Type { get; set; }
        public IndustryEnum? Industry { get; set; }
        public string ListedDate { get; set; }
        public string ExCh { get; set; }
        public bool Odd { get; set; }
        public DateTime Date { get; set; }
    }
}

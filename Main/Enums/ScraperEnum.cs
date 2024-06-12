using System.ComponentModel;

namespace TwStockNET.Enums
{
    public enum ScraperEnum
    {
        [Description("twse")]
        Twse = 0,
        [Description("tpex")]
        Tpex = 1,
        [Description("taifex")]
        Taifex = 2,
        [Description("tdcc")]
        Tdcc = 3,
        [Description("mis-twse")]
        MisTwse = 4,
        [Description("mis-taifex")]
        MisTaifex = 5,
        [Description("mops")]
        Mops = 6,
        [Description("isin")]
        Isin = 7
    }
}

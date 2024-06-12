using System.ComponentModel;

namespace TwStockNET.Enums
{
    public enum MarketEnum
    {
        /// <summary>上市</summary>
        [Description("上市")]
        TSE = 0,

        /// <summary>上櫃</summary>
        [Description("上櫃")]
        OTC = 1,

        /// <summary>期貨及選擇權</summary>
        [Description("期貨及選擇權")]
        FUTOPT = 2
    }
}

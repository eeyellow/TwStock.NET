using System.ComponentModel;

namespace TwStockNET.Enums
{
    public enum ExchangeEnum
    {
        [Description("臺灣證券交易所")]
        TWSE = 0,      // 臺灣證券交易所
        [Description("證券櫃檯買賣中心")]
        TPEx = 1,      // 證券櫃檯買賣中心
        [Description("臺灣期貨交易所")]
        TAIFEX = 2     // 臺灣期貨交易所
    }
}

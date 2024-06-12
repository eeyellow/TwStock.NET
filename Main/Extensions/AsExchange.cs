using System;
using System.Collections.Generic;
using TwStockNET.Enums;

namespace TwStockNET.Extensions
{
    public static partial class Extensions
    {
        private static readonly Dictionary<string, ExchangeEnum> Exchanges = new Dictionary<string, ExchangeEnum>
        {
            { MarketEnum.TSE.GetDescription(), ExchangeEnum.TWSE },
            { "TWSE", ExchangeEnum.TWSE },
            { MarketEnum.OTC.GetDescription(), ExchangeEnum.TPEx },
            { "TPEx", ExchangeEnum.TPEx },
            { MarketEnum.FUTOPT.GetDescription(), ExchangeEnum.TAIFEX },
            { "TAIFEX", ExchangeEnum.TPEx },
        };

        /// <summary>
        /// 將市場字串轉換為交易所列舉
        /// </summary>
        /// <param name="market"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static ExchangeEnum AsExchange(this string market)
        {
            if (Exchanges.TryGetValue(market, out ExchangeEnum exchange))
            {
                return exchange;
            }
            else
            {
                throw new ArgumentException($"Invalid exchange value: {market}");
            }
        }
    }
}

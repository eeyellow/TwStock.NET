using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TwStockNET.Extensions
{
    public static partial class Extensions
    {
        private static readonly List<Regex> rules = new List<Regex>
            {
                new Regex(@"^0[3-8][0-9][0-9][0-9][0-9]$"), // 上市國內標的認購權證
                new Regex(@"^0[3-8][0-9][0-9][0-9]P$"),     // 上市國內標的認售權證
                new Regex(@"^0[3-8][0-9][0-9][0-9]F$"),     // 上市外國標的認購權證
                new Regex(@"^0[3-8][0-9][0-9][0-9]Q$"),     // 上市外國標的認售權證
                new Regex(@"^0[3-8][0-9][0-9][0-9]C$"),     // 上市國內標的下限型認購權證
                new Regex(@"^0[3-8][0-9][0-9][0-9]B$"),     // 上市國內標的上限型認售權證
                new Regex(@"^0[3-8][0-9][0-9][0-9]X$"),     // 上市國內標的可展延下限型認購權證
                new Regex(@"^0[3-8][0-9][0-9][0-9]Y$"),     // 上市國內標的可展延上限型認售權證
                new Regex(@"^7[0-3][0-9][0-9][0-9][0-9]$"), // 上櫃國內標的認購權證
                new Regex(@"^7[0-3][0-9][0-9][0-9]P$"),     // 上櫃國內標的認售權證
                new Regex(@"^7[0-3][0-9][0-9][0-9]F$"),     // 上櫃外國標的認購權證
                new Regex(@"^7[0-3][0-9][0-9][0-9]Q$"),     // 上櫃外國標的認售權證
                new Regex(@"^7[0-3][0-9][0-9][0-9]C$"),     // 上櫃國內標的下限型認購權證
                new Regex(@"^7[0-3][0-9][0-9][0-9]B$"),     // 上櫃國內標的上限型認售權證
                new Regex(@"^7[0-3][0-9][0-9][0-9]X$"),     // 上櫃國內標的可展延下限型認購權證
                new Regex(@"^7[0-3][0-9][0-9][0-9]Y$"),     // 上櫃國內標的可展延上限型認售權證
            };

        /// <summary>
        /// 檢查指定的字串是否為權證代碼
        /// </summary>
        /// <param name="symbol">要檢查的字串</param>
        /// <returns>如果是權證代碼，則為 true；否則為 false。</returns>
        public static bool IsWarrant(this string symbol)
        {
            return rules.Any(regex => regex.IsMatch(symbol));
        }
    }
}

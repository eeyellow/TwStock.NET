using System;
using System.ComponentModel;
using System.Reflection;

namespace TwStockNET.Extensions
{
    public static partial class Extensions
    {
        /// <summary>
        /// 取得Enum的Description
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum source)
        {
            FieldInfo fi = source.GetType().GetField(source.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(
             typeof(DescriptionAttribute), false);
            if (attributes.Length > 0) return attributes[0].Description;
            else return source.ToString();
        }
    }
}

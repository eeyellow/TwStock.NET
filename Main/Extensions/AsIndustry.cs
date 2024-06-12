using System;
using System.Collections.Generic;
using TwStockNET.Enums;

namespace TwStockNET.Extensions
{
    public static partial class Extensions
    {
        private static readonly Dictionary<string, IndustryEnum> Industries = new Dictionary<string, IndustryEnum>
        {
            {"水泥工業", IndustryEnum.Cement},
            {"食品工業", IndustryEnum.Food},
            {"塑膠工業", IndustryEnum.Plastic},
            {"紡織纖維", IndustryEnum.Textiles},
            {"電機機械", IndustryEnum.ElectricMachinery},
            {"電器電纜", IndustryEnum.ElectricalAndCable},
            {"玻璃陶瓷", IndustryEnum.GlassAndCeramic},
            {"造紙工業", IndustryEnum.PaperAndPulp},
            {"鋼鐵工業", IndustryEnum.IronAndSteel},
            {"橡膠工業", IndustryEnum.Rubber},
            {"汽車工業", IndustryEnum.Automobile},
            {"建材營造業", IndustryEnum.BuildingMaterialsAndConstruction},
            {"航運業", IndustryEnum.ShippingAndTransportation},
            {"觀光餐旅", IndustryEnum.TourismAndHospitality},
            {"金融保險業", IndustryEnum.FinancialAndInsurance},
            {"貿易百貨業", IndustryEnum.TradingAndConsumerGoods},
            {"綜合", IndustryEnum.Miscellaneous},
            {"其他業", IndustryEnum.Other},
            {"化學工業", IndustryEnum.Chemical},
            {"生技醫療業", IndustryEnum.BiotechnologyAndMedicalCare},
            {"油電燃氣業", IndustryEnum.OilGasAndElectricity},
            {"半導體業", IndustryEnum.Semiconductors},
            {"電腦及週邊設備業", IndustryEnum.ComputerAndPeripheralEquipment},
            {"光電業", IndustryEnum.Optoelectronics},
            {"通信網路業", IndustryEnum.CommunicationsTechnologyAndInternet},
            {"電子零組件業", IndustryEnum.ElectronicPartsComponents},
            {"電子通路業", IndustryEnum.ElectronicProductsDistirbution},
            {"資訊服務業", IndustryEnum.InformationService},
            {"其他電子業", IndustryEnum.OtherElectronics},
            {"文化創意業", IndustryEnum.CulturalAndCreative},
            {"農業科技業", IndustryEnum.AgriculturalTechnology},
            {"電子商務", IndustryEnum.Ecommerce},
            {"綠能環保", IndustryEnum.GreenEnergyAndEnvironmentalServices},
            {"數位雲端", IndustryEnum.DigitalAndCloudServices},
            {"運動休閒", IndustryEnum.SportsAndLeisure},
            {"居家生活", IndustryEnum.Household},
            {"管理股票", IndustryEnum.ManagedStock},
            {"無所屬產業", IndustryEnum.None},
        };

        /// <summary>
        /// 將行業名稱字串轉換為對應的行業列舉值
        /// </summary>
        /// <param name="industry"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IndustryEnum AsIndustry(this string industry)
        {
            if (Industries.TryGetValue(industry, out IndustryEnum industryenum))
            {
                return industryenum;
            }
            else
            {
                return IndustryEnum.None;
            }
        }
    }
}

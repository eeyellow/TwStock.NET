using System;
using System.Collections.Generic;
using TwStockNET.Enums;

namespace TwStockNET.Extensions
{
    public static partial class Extensions
    {
        private static readonly Dictionary<string, IndexEnum> Index = new Dictionary<string, IndexEnum>
        {
            { "發行量加權股價指數", IndexEnum.TAIEX },
            { "未含金融指數", IndexEnum.NonFinance },
            { "未含電子指數", IndexEnum.NonElectronics },
            { "未含金融電子指數", IndexEnum.NonFinanceNonElectronics },
            { "水泥類指數", IndexEnum.Cement },
            { "食品類指數", IndexEnum.Food },
            { "塑膠類指數", IndexEnum.Plastic },
            { "水泥窯製類指數", IndexEnum.CementAndCeramic },
            { "塑膠化工類指數", IndexEnum.PlasticAndChemical },
            { "機電類指數", IndexEnum.Electrical },
            { "紡織纖維類指數", IndexEnum.Textiles },
            { "電機機械類指數", IndexEnum.ElectricMachinery },
            { "電器電纜類指數", IndexEnum.ElectricalAndCable },
            { "化學生技醫療類指數", IndexEnum.ChemicalBiotechnologyAndMedicalCare },
            { "化學類指數", IndexEnum.Chemical },
            { "生技醫療類指數", IndexEnum.BiotechnologyAndMedicalCare },
            { "玻璃陶瓷類指數", IndexEnum.GlassAndCeramic },
            { "造紙類指數", IndexEnum.PaperAndPulp },
            { "鋼鐵類指數", IndexEnum.IronAndSteel },
            { "橡膠類指數", IndexEnum.Rubber },
            { "汽車類指數", IndexEnum.Automobile },
            { "電子工業類指數", IndexEnum.Electronics },
            { "半導體類指數", IndexEnum.Semiconductors },
            { "電腦及週邊設備類指數", IndexEnum.ComputerAndPeripheralEquipment },
            { "光電類指數", IndexEnum.Optoelectronics },
            { "通信網路類指數", IndexEnum.CommunicationsTechnologyAndInternet },
            { "電子零組件類指數", IndexEnum.ElectronicPartsComponents },
            { "電子通路類指數", IndexEnum.ElectronicProductsDistirbution },
            { "資訊服務類指數", IndexEnum.InformationService },
            { "其他電子類指數", IndexEnum.OtherElectronics },
            { "建材營造類指數", IndexEnum.BuildingMaterialsAndConstruction },
            { "航運類指數", IndexEnum.ShippingAndTransportation },
            { "觀光餐旅類指數", IndexEnum.TourismAndHospitality },
            { "金融保險類指數", IndexEnum.FinancialAndInsurance },
            { "貿易百貨類指數", IndexEnum.TradingAndConsumerGoods },
            { "油電燃氣類指數", IndexEnum.OilGasAndElectricity },
            { "其他類指數", IndexEnum.Other },
            { "綠能環保類指數", IndexEnum.GreenEnergyAndEnvironmentalServices },
            { "數位雲端類指數", IndexEnum.DigitalAndCloudServices },
            { "運動休閒類指數", IndexEnum.SportsAndLeisure },
            { "居家生活類指數", IndexEnum.Household },
            { "櫃買指數", IndexEnum.TPEX },
            { "櫃買紡纖類指數", IndexEnum.TPExTextiles },
            { "櫃買機械類指數", IndexEnum.TPExElectricMachinery },
            { "櫃買鋼鐵類指數", IndexEnum.TPExIronAndSteel },
            { "櫃買電子類指數", IndexEnum.TPExElectronic },
            { "櫃買營建類指數", IndexEnum.TPExBuildingMaterialsAndConstruction },
            { "櫃買航運類指數", IndexEnum.TPExShippingAndTransportation },
            { "櫃買觀光餐旅類指數", IndexEnum.TPExTourismAndHospitality },
            { "櫃買化工類指數", IndexEnum.TPExChemical },
            { "櫃買生技醫療類指數", IndexEnum.TPExBiotechnologyAndMedicalCare },
            { "櫃買半導體類指數", IndexEnum.TPExSemiconductors },
            { "櫃買電腦及週邊類指數", IndexEnum.TPExComputerAndPeripheralEquipment },
            { "櫃買光電業類指數", IndexEnum.TPExOptoelectronic },
            { "櫃買通信網路類指數", IndexEnum.TPExCommunicationsAndInternet },
            { "櫃買電子零組件類指數", IndexEnum.TPExElectronicPartsComponents },
            { "櫃買電子通路類指數", IndexEnum.TPExElectronicProductsDistribution },
            { "櫃買資訊服務類指數", IndexEnum.TPExInformationService },
            { "櫃買文化創意業類指數", IndexEnum.TPExCulturalAndCreative },
            { "櫃買其他電子類指數", IndexEnum.TPExOtherElectronic },
            { "櫃買其他類指數", IndexEnum.TPExOther },
            { "櫃買綠能環保類指數", IndexEnum.TPExGreenEnergyAndEnvironmentalServices },
            { "櫃買數位雲端類指數", IndexEnum.TPExDigitalAndCloudServices },
            { "櫃買居家生活類指數", IndexEnum.TPExHousehold },

            { "未含金融保險股指數", IndexEnum.NonFinance },
            { "未含電子股指數", IndexEnum.NonElectronics },
            { "未含金融電子股指數", IndexEnum.NonFinanceNonElectronics },
            { "電子類指數", IndexEnum.Electronics },
            { "觀光類指數", IndexEnum.TourismAndHospitality },
            { "航運業類指數", IndexEnum.ShippingAndTransportation },
            { "觀光事業類指數", IndexEnum.TourismAndHospitality },
            { "櫃買觀光類指數", IndexEnum.TPExTourismAndHospitality },
            { "櫃買生技類指數", IndexEnum.TPExBiotechnologyAndMedicalCare },
            { "櫃買電腦週邊類指數", IndexEnum.TPExComputerAndPeripheralEquipment },
            { "櫃買光電類指數", IndexEnum.TPExOptoelectronic },
            { "櫃買電子零件類指數", IndexEnum.TPExElectronicPartsComponents },
            { "櫃買文化創意類指數", IndexEnum.TPExCulturalAndCreative },
            { "櫃買紡織纖維類指數", IndexEnum.TPExTextiles },
            { "櫃買電機機械類指數", IndexEnum.TPExElectricMachinery },
            { "櫃買鋼鐵工業類指數", IndexEnum.TPExIronAndSteel },
            { "櫃買建材營造類指數", IndexEnum.TPExBuildingMaterialsAndConstruction },
            { "櫃買航運業類指數", IndexEnum.TPExShippingAndTransportation },
            { "櫃買化學工業類指數", IndexEnum.TPExChemical },
            { "櫃買半導體業類指數", IndexEnum.TPExSemiconductors },
            { "櫃買電腦週邊業類指數", IndexEnum.TPExComputerAndPeripheralEquipment },
            { "櫃買通信網路業類指數", IndexEnum.TPExCommunicationsAndInternet },
            { "櫃買電子零組件業類指數", IndexEnum.TPExElectronicPartsComponents },
            { "櫃買電子通路業類指數", IndexEnum.TPExElectronicProductsDistribution },
            { "櫃買資訊服務業類指數", IndexEnum.TPExInformationService },
            { "櫃買其他電子業類指數", IndexEnum.TPExOtherElectronic },
            { "櫃買電子工業類指數", IndexEnum.TPExElectronic },
            { "櫃買電腦及週邊設備業類指數", IndexEnum.TPExComputerAndPeripheralEquipment },
            { "櫃買觀光事業類指數", IndexEnum.TPExTourismAndHospitality },
        };

        /// <summary>
        /// 將索引字串轉換為索引列舉
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IndexEnum AsIndex(this string index)
        {
            if (Index.TryGetValue(index, out IndexEnum indexenum))
            {
                return indexenum;
            }
            else
            {
                throw new ArgumentException($"Invalid index value: {index}");
            }
        }
    }
}

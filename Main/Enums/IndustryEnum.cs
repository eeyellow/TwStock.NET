using System.ComponentModel;

namespace TwStockNET.Enums
{
    public enum IndustryEnum
    {
        [Description("水泥工業")]
        Cement = 1,                                 // 水泥工業
        [Description("食品工業")]
        Food = 2,                                   // 食品工業
        [Description("塑膠工業")]
        Plastic = 3,                                // 塑膠工業
        [Description("紡織纖維")]
        Textiles = 4,                               // 紡織纖維
        [Description("電機機械")]
        ElectricMachinery = 5,                      // 電機機械
        [Description("電器電纜")]
        ElectricalAndCable = 6,                     // 電器電纜
        [Description("玻璃陶瓷")]
        GlassAndCeramic = 8,                        // 玻璃陶瓷
        [Description("造紙工業")]
        PaperAndPulp = 9,                           // 造紙工業
        [Description("鋼鐵工業")]
        IronAndSteel = 10,                          // 鋼鐵工業
        [Description("橡膠工業")]
        Rubber = 11,                                // 橡膠工業
        [Description("汽車工業")]
        Automobile = 12,                            // 汽車工業
        [Description("建材營造業")]
        BuildingMaterialsAndConstruction = 14,      // 建材營造業
        [Description("航運業")]
        ShippingAndTransportation = 15,             // 航運業
        [Description("觀光餐旅")]
        TourismAndHospitality = 16,                 // 觀光餐旅
        [Description("金融保險業")]
        FinancialAndInsurance = 17,                 // 金融保險業
        [Description("貿易百貨業")]
        TradingAndConsumerGoods = 18,               // 貿易百貨業
        [Description("綜合")]
        Miscellaneous = 19,                         // 綜合
        [Description("其他業")]
        Other = 20,                                 // 其他業
        [Description("化學工業")]
        Chemical = 21,                              // 化學工業
        [Description("生技醫療業")]
        BiotechnologyAndMedicalCare = 22,           // 生技醫療業
        [Description("油電燃氣業")]
        OilGasAndElectricity = 23,                  // 油電燃氣業
        [Description("半導體業")]
        Semiconductors = 24,                        // 半導體業
        [Description("電腦及週邊設備業")]
        ComputerAndPeripheralEquipment = 25,        // 電腦及週邊設備業
        [Description("光電業")]
        Optoelectronics = 26,                       // 光電業
        [Description("通信網路業")]
        CommunicationsTechnologyAndInternet = 27,   // 通信網路業
        [Description("電子零組件業")]
        ElectronicPartsComponents = 28,             // 電子零組件業
        [Description("電子通路業")]
        ElectronicProductsDistirbution = 29,        // 電子通路業
        [Description("資訊服務業")]
        InformationService = 30,                    // 資訊服務業
        [Description("其他電子業")]
        OtherElectronics = 31,                      // 其他電子業
        [Description("文化創意業")]
        CulturalAndCreative = 32,                   // 文化創意業
        [Description("農業科技業")]
        AgriculturalTechnology = 33,                // 農業科技業
        [Description("電子商務")]
        Ecommerce = 34,                             // 電子商務
        [Description("綠能環保")]
        GreenEnergyAndEnvironmentalServices = 35,   // 綠能環保
        [Description("數位雲端")]
        DigitalAndCloudServices = 36,               // 數位雲端
        [Description("運動休閒")]
        SportsAndLeisure = 37,                      // 運動休閒
        [Description("居家生活")]
        Household = 38,                             // 居家生活
        [Description("管理股票")]
        ManagedStock = 80,                          // 管理股票
        [Description("無所屬產業")]
        None = 0                                    // 無所屬產業
    }
}

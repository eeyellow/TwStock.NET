using System.ComponentModel;

namespace TwStockNET.Enums
{
    public enum IndexEnum
    {
        /// <summary>發行量加權股價指數</summary>
        [Description("發行量加權股價指數")]
        TAIEX = 0,
        /// <summary>未含金融指數</summary>
        [Description("未含金融指數")]
        NonFinance = 1,
        /// <summary>未含電子指數</summary>
        [Description("未含電子指數")]
        NonElectronics = 2,
        /// <summary>未含金融電子指數</summary>
        [Description("未含金融電子指數")]
        NonFinanceNonElectronics = 3,
        /// <summary>水泥類指數</summary>
        [Description("水泥類指數")]
        Cement = 4,
        /// <summary>食品類指數</summary>
        [Description("食品類指數")]
        Food = 5,
        /// <summary>塑膠類指數</summary>
        [Description("塑膠類指數")]
        Plastic = 6,
        /// <summary>水泥窯製類指數</summary>
        [Description("水泥窯製類指數")]
        CementAndCeramic = 7,
        /// <summary>塑膠化工類指數</summary>
        [Description("塑膠化工類指數")]
        PlasticAndChemical = 8,
        /// <summary>機電類指數</summary>
        [Description("機電類指數")]
        Electrical = 9,
        /// <summary>紡織纖維類指數</summary>
        [Description("紡織纖維類指數")]
        Textiles = 10,
        /// <summary>電機機械類指數</summary>
        [Description("電機機械類指數")]
        ElectricMachinery = 11,
        /// <summary>電器電纜類指數</summary>
        [Description("電器電纜類指數")]
        ElectricalAndCable = 12,
        /// <summary>化學生技醫療類指數</summary>
        [Description("化學生技醫療類指數")]
        ChemicalBiotechnologyAndMedicalCare = 13,
        /// <summary>化學類指數</summary>
        [Description("化學類指數")]
        Chemical = 14,
        /// <summary>生技醫療類指數</summary>
        [Description("生技醫療類指數")]
        BiotechnologyAndMedicalCare = 15,
        /// <summary>玻璃陶瓷類指數</summary>
        [Description("玻璃陶瓷類指數")]
        GlassAndCeramic = 16,
        /// <summary>造紙類指數</summary>
        [Description("造紙類指數")]
        PaperAndPulp = 17,
        /// <summary>鋼鐵類指數</summary>
        [Description("鋼鐵類指數")]
        IronAndSteel = 18,
        /// <summary>橡膠類指數</summary>
        [Description("橡膠類指數")]
        Rubber = 19,
        /// <summary>汽車類指數</summary>
        [Description("汽車類指數")]
        Automobile = 20,
        /// <summary>電子工業類指數</summary>
        [Description("電子工業類指數")]
        Electronics = 21,                            
        /// <summary>半導體類指數</summary>
        [Description("半導體類指數")]
        Semiconductors = 22,                         
        /// <summary>電腦及週邊設備類指數</summary>
        [Description("電腦及週邊設備類指數")]
        ComputerAndPeripheralEquipment = 23,         
        /// <summary>光電類指數</summary>
        [Description("光電類指數")]
        Optoelectronics = 24,                        
        /// <summary>通信網路類指數</summary>
        [Description("通信網路類指數")]
        CommunicationsTechnologyAndInternet = 25,    
        /// <summary>電子零組件類指數</summary>
        [Description("電子零組件類指數")]
        ElectronicPartsComponents = 26,              
        /// <summary>電子通路類指數</summary>
        [Description("電子通路類指數")]
        ElectronicProductsDistirbution = 27,         
        /// <summary>資訊服務類指數</summary>
        [Description("資訊服務類指數")]
        InformationService = 28,                     
        /// <summary>其他電子類指數</summary>
        [Description("其他電子類指數")]
        OtherElectronics = 29,                       
        /// <summary>建材營造類指數</summary>
        [Description("建材營造類指數")]
        BuildingMaterialsAndConstruction = 30,       
        /// <summary>航運類指數</summary>
        [Description("航運類指數")]
        ShippingAndTransportation = 31,              
        /// <summary>觀光餐旅類指數</summary>
        [Description("觀光餐旅類指數")]
        TourismAndHospitality = 32,                  
        /// <summary>金融保險類指數</summary>
        [Description("金融保險類指數")]
        FinancialAndInsurance = 33,                  
        /// <summary>貿易百貨類指數</summary>
        [Description("貿易百貨類指數")]
        TradingAndConsumerGoods = 34,                
        /// <summary>油電燃氣類指數</summary>
        [Description("油電燃氣類指數")]
        OilGasAndElectricity = 35,                   
        /// <summary>其他類指數</summary>
        [Description("其他類指數")]
        Other = 36,                                  
        /// <summary>綠能環保類指數</summary>
        [Description("綠能環保類指數")]
        GreenEnergyAndEnvironmentalServices = 37,    
        /// <summary>數位雲端類指數</summary>
        [Description("數位雲端類指數")]
        DigitalAndCloudServices = 38,                
        /// <summary>運動休閒類指數</summary>
        [Description("運動休閒類指數")]
        SportsAndLeisure = 39,                       
        /// <summary>居家生活類指數</summary>
        [Description("居家生活類指數")]
        Household = 40,
        /// <summary>櫃檯指數</summary>
        [Description("櫃檯指數")]
        TPEX = 41,                                   
        /// <summary>櫃檯紡纖類指數</summary>
        [Description("櫃檯紡纖類指數")]
        TPExTextiles = 42,                           
        /// <summary>櫃檯機械類指數</summary>
        [Description("櫃檯機械類指數")]
        TPExElectricMachinery = 43,                  
        /// <summary>櫃檯鋼鐵類指數</summary>
        [Description("櫃檯鋼鐵類指數")]
        TPExIronAndSteel = 44,                       
        /// <summary>櫃檯電子類指數</summary>
        [Description("櫃檯電子類指數")]
        TPExElectronic = 45,                         
        /// <summary>櫃檯營建類指數</summary>
        [Description("櫃檯營建類指數")]
        TPExBuildingMaterialsAndConstruction = 46,   
        /// <summary>櫃檯航運類指數</summary>
        [Description("櫃檯航運類指數")]
        TPExShippingAndTransportation = 47,          
        /// <summary>櫃檯觀光餐旅類指數</summary>
        [Description("櫃檯觀光餐旅類指數")]
        TPExTourismAndHospitality = 48,              
        /// <summary>櫃檯化工類指數</summary>
        [Description("櫃檯化工類指數")]
        TPExChemical = 49,                           
        /// <summary>櫃檯生技醫療類指數</summary>
        [Description("櫃檯生技醫療類指數")]
        TPExBiotechnologyAndMedicalCare = 50,        
        /// <summary>櫃檯半導體類指數</summary>
        [Description("櫃檯半導體類指數")]
        TPExSemiconductors = 51,                     
        /// <summary>櫃檯電腦及週邊類指數</summary>
        [Description("櫃檯電腦及週邊類指數")]
        TPExComputerAndPeripheralEquipment = 52,     
        /// <summary>櫃檯光電業類指數</summary>
        [Description("櫃檯光電業類指數")]
        TPExOptoelectronic = 53,                     
        /// <summary>櫃檯通信網路類指數</summary>
        [Description("櫃檯通信網路類指數")]
        TPExCommunicationsAndInternet = 54,          
        /// <summary>櫃檯電子零組件類指數</summary>
        [Description("櫃檯電子零組件類指數")]
        TPExElectronicPartsComponents = 55,          
        /// <summary>櫃檯電子通路類指數</summary>
        [Description("櫃檯電子通路類指數")]
        TPExElectronicProductsDistribution = 56,     
        /// <summary>櫃檯資訊服務類指數</summary>
        [Description("櫃檯資訊服務類指數")]
        TPExInformationService = 57,                 
        /// <summary>櫃檯文化創意業類指數</summary>
        [Description("櫃檯文化創意業類指數")]
        TPExCulturalAndCreative = 58,                
        /// <summary>櫃檯其他電子類指數</summary>
        [Description("櫃檯其他電子類指數")]
        TPExOtherElectronic = 59,                    
        /// <summary>櫃檯其他類指數</summary>
        [Description("櫃檯其他類指數")]
        TPExOther = 60,                              
        /// <summary>櫃檯綠能環保類指數</summary>
        [Description("櫃檯綠能環保類指數")]
        TPExGreenEnergyAndEnvironmentalServices = 61,
        /// <summary>櫃檯數位雲端類指數</summary>
        [Description("櫃檯數位雲端類指數")]
        TPExDigitalAndCloudServices = 62,            
        /// <summary>櫃檯居家生活類指數</summary>
        [Description("櫃檯居家生活類指數")]
        TPExHousehold = 63                           
    }
}

namespace TopModel
{
    public class SuplierInfo
    {
        public decimal profitMax { get; set; }
        public decimal profitMin { get; set; }
        public int autoDelivery { get; set; }
        public int mode { get; set; }
        public int profitMode { get; set; }
        /// <summary>
        /// 当前使用的供应商及利润设置
        /// </summary>
        public SuplierDetail[] profitData { get; set; }
        /// <summary>
        /// 供应商列表，包括当前使用的
        /// </summary>
        public SuplierDetail[] data { get; set; }
        public string status { get; set; }//200
    }
    public class SuplierDetail
    {
        public string name { get; set; }
        public string id { get; set; }
        /// <summary>
        /// 进价
        /// </summary>
        public decimal price { get; set; }
        public string tag { get; set; }
        /// <summary>
        /// up or down
        /// </summary>
        public string status { get; set; }
        public bool select { get; set; }
    }
}
using LiteDB;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace TopModel
{
    public class ProductItem
    {
        /// <summary>
        /// 如522571681053
        /// </summary>
        [Display(Name = "ID")]
        public long Id { get; set; }

        /// <summary>
        /// 是否监控
        /// </summary>
        [Display(Name = "是否监控")]
        [LocalProperty]
        public bool Monitor { get; set; }

        [Display(Name = "自动上架")]
        public bool AutoUpshelf { get; set; }
        /// <summary>
        /// 如137883
        /// </summary>
        [Display(Name = "SpuId")]
        public string SpuId { get; set; }
        /// <summary>
        /// 如 QB便宜直充不客气（5个）
        /// </summary>
        [Display(Name = "名称")]
        public string ItemName { get; set; }
        /// <summary>
        /// 如 类型：QQ直充
        /// </summary>
        [Display(Name = "下标名")]
        [BsonIndex]
        public string ItemSubName { get; set; }

        /// <summary>
        /// 如  卖家代充 
        /// </summary>
        [Display(Name = "类型")]
        public string Type { get; set; }

        /// <summary>
        /// 面值如  5元 
        /// </summary>
        public string 面值 { get; set; }

        /// <summary>
        /// 进价 如  4.77元 
        /// </summary>
        [Display(Name = "进价")]
        public decimal 进价 { get; set; }

        /// <summary>
        /// 淘宝一口价 如 4.80元
        /// </summary>
        [Display(Name = "一口价")]
        public decimal 一口价 { get; set; }

        [Display(Name = "利润")]
        public decimal 利润 { get; set; }

        [Display(Name = "供应商")]
        public string SupplierId { get; set; }

        [Display(Name = "位置")]
        public string Where { get; set; }

        [Display(Name = "库存")]
        [LocalProperty]
        public int? StockQty { get; set; }

        [Display(Name = "更新时间")]
        public DateTime UpdateAt { get; set; }

        /// <summary>
        /// 同步供应商操作已提交
        /// </summary>
        public bool SyncSuplierSubmited { get; set; }

        /// <summary>
        /// 改价操作已提交
        /// </summary>
        public bool ModifyProfitSubmitted { get; set; }

        public void OnDownshelf(LiteCollection<ProductItem> productItems)
        {
            Where = "仓库中";
            productItems.Update(this);
        }

        public void OnUpshelf(LiteCollection<ProductItem> productItems)
        {
            Where = "出售中";
            productItems.Update(this);
        }

        /// <summary>
        /// 获取供应商
        /// </summary>
        /// <returns></returns>
        public SuplierInfo GetSuplierInfo()
        {
            return new SuplierInfo
            {
                profitMax = this.利润,
                profitMin = this.利润,
                profitData = new SuplierDetail[] {
                        new SuplierDetail {
                            id =this.SupplierId,
                            price = this.进价
                        }
                }
            };
        }

        /// <summary>
        /// 更新供应商信息
        /// </summary>
        /// <param name="productItems"></param>
        /// <param name="supplier"></param>
        public void OnSupplierInfoUpdate(LiteCollection<ProductItem> productItems, SuplierInfo supplier)
        {
            this.利润 = supplier.profitMax;
            this.进价 = supplier.profitData[0].price;
            this.一口价 = supplier.profitData[0].price + supplier.profitMax;
            this.SupplierId = supplier.profitData[0].id;
            this.UpdateAt = DateTime.Now;
            productItems.Update(this);
        }
    }
}

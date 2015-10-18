using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using System.ComponentModel.DataAnnotations;
using Top.Api.Domain;
namespace TopModel.Models
{
    public class TopTrade : IEqualityComparer<TopTrade>
    {
        [BsonId]
        [Display(Name = "订单号")]
        public long Tid { get; set; }//淘宝订单号（唯一标识）

        [Display(Name = "买家名称")]
        public string BuyerNick { get; set; }//买家名称

        [Display(Name = "下单时间")]
        public DateTime Created { get; set; }

        [Display(Name = "付款时间")]
        public DateTime? PayTime { get; set; }

        [Display(Name = "商品ID")]
        public long NumIid { get; set; }

        [Display(Name = "实付款")]
        public string Payment { get; set; }

        [BsonIndex]
        [Display(Name = "收货地址")]
        public string ReceiverAddress { get; set; }

        /// <summary>
        ///  交易状态。可选值:      * TRADE_NO_CREATE_PAY(没有创建支付宝交易)      * WAIT_BUYER_PAY(等待买家付款)      * SELLER_CONSIGNED_PART(卖家部分发货)      * WAIT_SELLER_SEND_GOODS(等待卖家发货,即:买家已付款)      * WAIT_BUYER_CONFIRM_GOODS(等待买家确认收货,即:卖家已发货)      * TRADE_BUYER_SIGNED(买家已签收,货到付款专用)      * TRADE_FINISHED(交易成功)      * TRADE_CLOSED(付款以后用户退款成功，交易自动关闭)      * TRADE_CLOSED_BY_TAOBAO(付款以前，卖家或买家主动关闭交易)
        /// </summary>
        [Display(Name = "交易状态")]
        public string Status { get; set; }

        [Display(Name = "是否卖家可评")]
        public bool SellerCanRate { get; set; }

        [Display(Name = "是否卖家已评")]
        public bool SellerRate { get; set; }

        [Display(Name = "成交时间")]
        public DateTime? EndTime { get; set; }

        [Display(Name = "购买数量")]
        public long Num { get; set; }

        [Display(Name = "最后更新")]
        public DateTime UpdateAt { get; set; }

        [Display(Name = "是否拦截")]
        public bool Intercept { get; set; }

        public bool Equals(TopTrade x, TopTrade y)    //比较x和y对象是否相同，按照订单号比较
        {
            return x.Tid == y.Tid;
        }

        public int GetHashCode(TopTrade obj)
        {
            return obj.ToString().GetHashCode();
        }

        public static TopTrade FromTrade(Trade trade)
        {
            TopTrade order = new TopTrade();
            order.Num = trade.Num;
            order.Tid = trade.Tid;
            order.BuyerNick = trade.BuyerNick;
            order.Created = trade.Created.AsDateTime();
            order.NumIid = trade.NumIid;
            order.Payment = trade.Payment;
            order.PayTime = trade.PayTime.AsNullableDateTime();
            order.Status = trade.Status;
            order.ReceiverAddress = trade.ReceiverAddress;
            order.SellerCanRate = trade.SellerCanRate;
            order.SellerRate = trade.SellerRate;
            order.EndTime = trade.EndTime.AsNullableDateTime();
            return order;
        }
    }
}

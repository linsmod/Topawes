using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using TopModel.Models;

namespace WinFormsClient.MessageHubClients
{
    public class TradeHubProxy : HubProxyInvoker
    {
        /// <summary>
        /// 关闭交易消息
        /// </summary>
        /// <remarks>
        /// 在买家未付款之前，卖家或买家关闭这笔交易。 当通过api关闭交易，关闭订单或子订单时，会产生此消息。 当通过页面关闭交易时，会产生此消息。
        /// </remarks>
        public event Action<object> TradeClose;

        /// <summary>
        /// 买家付完款，或万人团买家付完尾款
        /// </summary>
        /// <remarks> 买家在页面付完款，会收到此消息 </remarks>
        public event Action<object> TradeBuyerPay;

        /// <summary>
        ///  创建淘宝交易消息 
        /// </summary>
        /// <remarks>买家购买商品产生此消息。
        ///当买家在页面购买商品生成订单发送此消息。
        ///当创建交易，成功创建交易发送此消息。
        ///在创建交易中，会创建支付宝订单。
        ///所以在创建交易中除了发此消息，
        ///还会发创建支付宝订单消息”taobao_trade_TradeAlipayCreate”
        ///</remarks>
        public event Action<object> TradeCreate;
        public TradeHubProxy(HubConnection connection) : base(connection, "TradeMessageHub")
        {
            HubProxy.On<object>("TradeClose", x => InvokeEvent(TradeClose, x));
            HubProxy.On<object>("TradeBuyerPay", x => InvokeEvent(TradeBuyerPay, x));
            HubProxy.On<object>("TradeCreate", x => InvokeEvent(TradeCreate, x));
        }

        /// <summary>
        /// 卖家关闭一笔交易
        /// </summary>
        /// <param name="tid"></param>
        /// <param name="closeReason"></param>
        /// <returns></returns>
        /// <remarks>关闭一笔订单，可以是主订单或子订单。当订单从创建到关闭时间小于10s的时候，会报“CLOSE_TRADE_TOO_FAST”错误。</remarks>
        public async Task<ApiResult<TopTrade>> CloseTrade(long tid, string closeReason)
        {
            return await ProxyInvoke<ApiResult<TopTrade>>("CloseTrade", tid, closeReason);
        }
    }
}

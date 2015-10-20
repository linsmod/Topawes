using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using TopModel.Models;
using Top.Tmc;

namespace TopModel.MessageHubClients
{
    /// <summary>
    /// 交易消息及逻辑
    /// </summary>
    public class TradeHubProxy : HubProxyInvoker
    {
        /// <summary>
        /// 关闭交易消息
        /// </summary>
        /// <remarks>
        /// 在买家未付款之前，卖家或买家关闭这笔交易。 当通过api关闭交易，关闭订单或子订单时，会产生此消息。 当通过页面关闭交易时，会产生此消息。
        /// </remarks>
        public event Action<Message> TradeClose;

        /// <summary>
        /// 买家付完款，或万人团买家付完尾款
        /// </summary>
        /// <remarks> 买家在页面付完款，会收到此消息 </remarks>
        public event Action<Message> TradeBuyerPay;

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
        public event Action<Message> TradeCreate;

        /// <summary>
        /// 交易消息及逻辑
        /// </summary>
        /// <param name="connection"></param>
        public TradeHubProxy(HubConnection connection) : base(connection, "TradeMessageHub")
        {
            HubProxy.On<Message>("TradeClose", x => InvokeEvent(TradeClose, x));
            HubProxy.On<Message>("TradeBuyerPay", x => InvokeEvent(TradeBuyerPay, x));
            HubProxy.On<Message>("TradeCreate", x => InvokeEvent(TradeCreate, x));
        }

        /// <summary>
        /// 卖家关闭一笔交易
        /// </summary>
        /// <param name="tid"></param>
        /// <param name="closeReason">1.未及时付款 2.买家联系不上 3.谢绝还价 4.商品瑕疵 5.协商不一致 6.买家不想买 7.与买家协商一致</param>
        /// <returns></returns>
        /// <remarks>关闭一笔订单，可以是主订单或子订单。当订单从创建到关闭时间小于10s的时候，会报“CLOSE_TRADE_TOO_FAST”错误。</remarks>
        public async Task<ApiResult<long>> CloseTrade(long tid, string closeReason)
        {
            return await ProxyInvoke<ApiResult<long>>("CloseTrade", tid, closeReason);
        }

        public async Task<ApiResult<string>> GetTradeStatus(long tid)
        {
            return await ProxyInvoke<ApiResult<string>>("GetTradeStatus", tid);
        }

        /// <summary>
        /// 获取交易信息
        /// </summary>
        /// <param name="tid"></param>
        /// <returns></returns>
        public async Task<ApiResult<TopTrade>> GetTrade(long tid)
        {
            return await ProxyInvoke<ApiResult<TopTrade>>("GetTrade", tid);
        }
    }
}

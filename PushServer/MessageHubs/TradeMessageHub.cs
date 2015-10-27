using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;
using Top.Tmc;
using TopModel.Models;
using TopModel;
using Microsoft.AspNet.SignalR;

namespace PushServer.MessageHubs
{
    public interface ITradeMessageClient
    {
        /// <summary>
        /// 关闭交易消息
        /// </summary>
        /// <remarks>
        /// 在买家未付款之前，卖家或买家关闭这笔交易。 当通过api关闭交易，关闭订单或子订单时，会产生此消息。 当通过页面关闭交易时，会产生此消息。
        /// </remarks>
        void TradeClose(Message msg);

        /// <summary>
        /// 买家付完款，或万人团买家付完尾款
        /// </summary>
        /// <remarks> 买家在页面付完款，会收到此消息 </remarks>
        void TradeBuyerPay(Message msg);

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
        void TradeCreate(Message msg);
    }

    [Authorize]
    public class TradeMessageHub : TopawesHub<ITradeMessageClient>
    {
        /// <summary>
        /// 卖家关闭一笔交易
        /// </summary>
        /// <param name="tid"></param>
        /// <param name="closeReason"></param>
        /// <returns></returns>
        /// <remarks>关闭一笔订单，可以是主订单或子订单。当订单从创建到关闭时间小于10s的时候，会报“CLOSE_TRADE_TOO_FAST”错误。</remarks>
        public ApiResult<long> CloseTrade(long tid, string closeReason)
        {
            ITopClient client = GetTopClient();
            TradeCloseRequest req = new TradeCloseRequest();
            req.Tid = tid;
            req.CloseReason = closeReason;
            TradeCloseResponse rsp = client.Execute(req, AccessToken);
            return rsp.AsApiResult(()=>rsp.Trade.Tid);
        }

        public ApiPagedResult<List<TopTrade>> GetSoldTrade(string status, int page, DateTime start)
        {
            ITopClient client = GetTopClient();
            TradesSoldGetRequest request = new TradesSoldGetRequest
            {
                Fields = "tid,buyer_nick,num,num_iid,created,pay_time,payment,receiver_address,status,end_time,seller_rate,seller_can_rate"
            };
            request.StartCreated = start;
            request.EndCreated = DateTime.Now;
            request.Status = status;
            request.Type = "guarantee_trade";
            request.PageNo = page;
            request.PageSize = 50;
            request.UseHasNext = true;

            TradesSoldGetResponse response = client.Execute<TradesSoldGetResponse>(request, AccessToken);
            List<TopTrade> list = new List<TopTrade>();

            if (!response.IsError)
            {
                list = response.Trades.Select(x => TopTrade.FromTrade(x)).ToList();
            }
            return response.AsApiPagedResult(list, response.HasNext);
        }

        /// <summary>
        /// 获取订单状态
        /// </summary>
        /// <param name="tid"></param>
        /// <returns></returns>
        public ApiResult<string> GetTradeStatus(long tid)
        {
            ITopClient client = GetTopClient();
            TradeGetRequest request = new TradeGetRequest
            {
                Fields = "status",
                Tid = tid
            };
            TradeGetResponse response = client.Execute<TradeGetResponse>(request, AccessToken);
            return response.AsApiResult(()=>response.Trade.Status);
        }

        /// <summary>
        /// 获取交易信息
        /// </summary>
        /// <param name="tid"></param>
        /// <returns></returns>
        public ApiResult<TopTrade> GetTrade(long tid)
        {
            ITopClient client = GetTopClient();
            TradeFullinfoGetRequest request = new TradeFullinfoGetRequest
            {
                Tid = tid,
                Fields = "tid,buyer_nick,num,num_iid,created,pay_time,payment,receiver_address,status,end_time,seller_rate,seller_can_rate"
            };
            var rsp = client.Execute(request, AccessToken);
            return rsp.AsApiResult<TopTrade>(()=>TopTrade.FromTrade(rsp.Trade));
        }
    }
}

using PushServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Top.Api;
using Top.Api.Domain;
using Top.Api.Request;
using Top.Api.Response;
using Top.Tmc;
using TopModel.Models;
using TopModel;
namespace PushServer
{
    public class TopManager
    {
        public static ITopClient GetTopClient()
        {
            return TopOperation.GetTopClient();
        }
        public static TopOpr TopOperation;
        static bool _initialized;
        public static bool Initialized
        {
            get
            {
                return _initialized;
            }
            private set
            {
                SignalRServer.Instance.Clients.All.OnTopManagerState(value);
                _initialized = value;
            }
        }
        static ApiServiceAccount apiServiceAccount;
        public static void Initialize(ApiServiceAccount account)
        {
            apiServiceAccount = account;
            Initialized = true;
            TopOperation = new TopOpr(account);
            TopOperation.TmcStartListen();
        }

        public static ApiResult TmcUserPermitThenTmcGroupAdd(string nick, string accessToken)
        {
            if (!Initialized)
            {
                throw new Exception("使用TopManager前必须Initialize");
            }
            var x = TopOperation.PermitMSG(accessToken);
            if (!x.Success)
                return x;
            return TopOperation.AddGroup(nick);
        }


        public static void TmcUserCancel(string nick)
        {
            if (!Initialized)
            {
                throw new Exception("使用TopManager前必须Initialize");
            }
            TopOperation.CancelMSG(nick);
        }

        public static bool IsListening(string nick)
        {
            if (!Initialized)
            {
                throw new Exception("使用TopManager前必须Initialize");
            }
            return TopOperation.TmcListening;
        }
        public class ApiServiceAccount
        {
            public ApiServiceAccount(string appKey, string appSecret)
            {
                this.AppKey = appKey;
                this.AppSecret = appSecret;
            }
            public string UserNick { get; set; }
            public string AppKey { get; set; }
            public string AppSecret { get; set; }
        }

        public class TopOpr
        {

            ApiServiceAccount account;
            const string url_api = "http://gw.api.taobao.com/router/rest?";//gw.api.taobao.com=140.205.76.86
            const string url_TMCserveraddress = "ws://mc.api.taobao.com/";
            TmcClient tmcClient;
            string AppKey { get { return account.AppKey; } }
            string AppSecret { get { return account.AppSecret; } }
            public bool TmcListening { get; private set; }
            public TopOpr(ApiServiceAccount account)
            {
                this.account = account;
            }

            public ITopClient GetTopClient()
            {
                DefaultTopClient client = new DefaultTopClient(url_api, this.AppKey, this.AppSecret);
                client.SetDisableTrace(true);
                return client;
            }

            /// <summary>
            /// 获取当前应用的卖家昵称
            /// </summary>
            /// <returns></returns>
            public string GetUserSeller(Models.UserTaoOAuth taoOAuth)
            {
                ITopClient client = new DefaultTopClient(url_api, AppKey, AppSecret);
                UserSellerGetRequest request = new UserSellerGetRequest
                {
                    Fields = "nick",
                };
                return GetTopResponseBody(client.Execute<UserSellerGetResponse>(request, taoOAuth.access_token));
            }

            private string GetTopResponseBody(TopResponse x)
            {
                if (!x.IsError)
                {
                    return x.Body;
                }
                return x.ErrMsg;
            }

            public ApiResult AddGroup(string nick)
            {
                ITopClient client = new DefaultTopClient(url_api, AppKey, AppSecret);
                TmcGroupAddRequest req = new TmcGroupAddRequest();
                req.GroupName = "sunshine";
                req.Nicks = nick;
                TmcGroupAddResponse response = client.Execute(req);
                return response.AsApiResult();
            }
            /// <summary>
            /// 连接消息服务
            /// </summary>
            /// <param name="handlerOnMessage"></param>
            public void TmcStartListen()
            {
                this.tmcClient = new TmcClient(AppKey, AppSecret, "sunshine");
                this.tmcClient.OnMessage += (sender, e) =>
                {
                    SignalRServer.Instance.PushMessage(e.Message);
                };
                this.tmcClient.Connect(url_TMCserveraddress);
                this.TmcListening = true;
            }

            public void SendTmcMessage(string accessToken, string topic, string content)
            {
                tmcClient.Send(topic, content, accessToken);
            }

            /// <summary>
            /// 取消用户的消息服务
            /// </summary>
            /// <param name="nick"></param>
            public bool CancelMSG(string nick)
            {
                try
                {
                    ITopClient client = new DefaultTopClient(url_api, this.AppKey, this.AppSecret);
                    TmcUserCancelRequest request = new TmcUserCancelRequest
                    {
                        Nick = nick
                    };
                    return client.Execute<TmcUserCancelResponse>(request).IsSuccess;
                }
                catch (Exception e)
                {
                    throw new Exception("TmcUserCancelRequest Failure.", e);
                }
            }
            /// <summary>
            /// 关闭消息服务客户端
            /// </summary>
            public void TmcStopListen()
            {
                if (tmcClient != null)
                {
                    tmcClient.Close();
                }
                this.TmcListening = false;
            }

            /// <summary>
            /// 获得消息许可
            /// </summary>
            /// <returns></returns>
            public ApiResult PermitMSG(string accessToken)
            {
                var topics = new string[] {
                    "taobao_trade_TradeCreate",
                    "taobao_trade_TradeBuyerPay",
                    "taobao_trade_TradeClose",

                    "taobao_refund_RefundCreated",
                    "taobao_refund_RefundSellerAgreeAgreement",
                    "taobao_refund_RefundSellerRefuseAgreement",
                    "taobao_refund_RefundBuyerModifyAgreement",
                    "taobao_refund_RefundBuyerReturnGoods",
                    "taobao_refund_RefundCreateMessage",
                    "taobao_refund_RefundBlockMessage",
                    "taobao_refund_RefundTimeoutRemind",
                    "taobao_refund_RefundClosed",
                    "taobao_refund_RefundSuccess",

                    "taobao_item_ItemAdd",
                    "taobao_item_ItemUpshelf",
                    "taobao_item_ItemDownshelf",
                    "taobao_item_ItemDelete",
                    "taobao_item_ItemUpdate",
                    "taobao_item_ItemZeroStock",
                    "taobao_item_ItemStockChanged",
                    "taobao_item_ItemRecommendDelete"
                };
                ITopClient client = new DefaultTopClient(url_api, AppKey, AppSecret);
                TmcUserPermitRequest request = new TmcUserPermitRequest
                {
                    Topics = string.Join(",", topics)
                };
                //var request = new TmcUserPermitRequest();
                var resp = client.Execute<TmcUserPermitResponse>(request, accessToken);
                return resp.AsApiResult();
            }

            /// <summary>
            /// 获取已开通消息
            /// </summary>
            /// <param name="nick">店铺昵称</param>
            /// <returns></returns>
            public ApiResult<TmcUser> TmcUserGet(Models.UserTaoOAuth taoUserOAuth)
            {
                ITopClient client = new DefaultTopClient(url_api, AppKey, AppSecret);
                TmcUserGetRequest request = new TmcUserGetRequest();
                request.Nick = taoUserOAuth.taobao_user_nick;
                request.Fields = "user_nick,topics,user_id,is_valid,created,modified";
                var resp = client.Execute<TmcUserGetResponse>(request, taoUserOAuth.access_token);
                return resp.AsApiResult(()=>resp.TmcUser);
            }

            /// <summary>
            /// 根据订单号获取商品ID
            /// </summary>
            /// <param name="tid"></param>
            /// <returns></returns>
            public string GetItemID(long tid, Models.UserTaoOAuth taoUserOAuth)
            {
                try
                {
                    ITopClient client = new DefaultTopClient(url_api, this.AppKey, this.AppSecret);
                    TradeGetRequest request = new TradeGetRequest
                    {
                        Fields = "num_iid",
                        Tid = tid
                    };
                    var resp = client.Execute<TradeGetResponse>(request, taoUserOAuth.access_token);
                    if (resp.IsError)
                    {
                        SignalRServer.Instance.Clients.User(taoUserOAuth.taobao_user_nick).OnMessage("宝贝ID查询失败：" + resp.Body);
                    }
                    return resp.Trade.NumIid.ToString();
                }
                catch (Exception e)
                {
                    throw new Exception("TradeGetRequest Failure.", e);
                }
            }
            /// <summary>
            /// 根据订单号获取商品ID
            /// </summary>
            /// <param name="tid"></param>
            /// <returns></returns>
            public Trade GetTradeByTid(long tid, Models.UserTaoOAuth taoUserOAuth)
            {
                try
                {
                    ITopClient client = new DefaultTopClient(url_api, this.AppKey, this.AppSecret);

                    TradeFullinfoGetRequest request = new TradeFullinfoGetRequest
                    {
                        Fields = "tid,buyer_nick,num_iid,num,created,payment,pay_time,price,receiver_address",
                        Tid = tid
                    };
                    var resp = client.Execute<TradeFullinfoGetResponse>(request, taoUserOAuth.access_token);
                    if (resp.IsError)
                    {
                        SignalRServer.Instance.Clients.User(taoUserOAuth.taobao_user_nick).OnMessage("订单查询失败：" + resp.Body);
                    }
                    return resp.Trade;
                }
                catch (Exception e)
                {
                    throw new Exception("TradeFullinfoGetRequest Failure.", e);
                }
            }

            /// <summary>
            /// 根据订单号获取商品ID
            /// </summary>
            /// <param name="tid"></param>
            /// <returns></returns>
            public Trade GetTradeDetailByTid(long tid, Models.UserTaoOAuth taoUserOAuth)
            {
                try
                {
                    ITopClient client = new DefaultTopClient(url_api, this.AppKey, this.AppSecret);
                    TradeGetRequest request = new TradeGetRequest
                    {
                        Fields = "tid,seller_nick,buyer_nick,num_iid,status,num,created,payment,pay_time,price",
                        Tid = tid
                    };
                    TradeGetResponse tgr = client.Execute<TradeGetResponse>(request, taoUserOAuth.access_token);
                    if (tgr.Trade == null)
                    {
                        SignalRServer.Instance.Clients.User(taoUserOAuth.taobao_user_nick).OnMessage("订单查询失败：" + tgr.Body);
                    }
                    return tgr.Trade;
                }
                catch (Exception e)
                {
                    throw new Exception("TradeGetRequest Failure.", e);
                }
            }
            /// <summary>
            /// 调用Api关闭订单
            /// </summary>
            /// <param name="tid">订单号</param>
            /// <param name="reason">关单理由</param>
            public ApiResult CloseOrderByApi(long tid, string reason, Models.UserTaoOAuth taoUserOAuth)
            {
                try
                {
                    DefaultTopClient client = new DefaultTopClient(url_api, this.AppKey, this.AppSecret);
                    client.SetDisableTrace(true);
                    TradeCloseRequest request = new TradeCloseRequest
                    {
                        Tid = tid,
                        CloseReason = reason
                    };
                    TradeCloseResponse tcr = client.Execute<TradeCloseResponse>(request, taoUserOAuth.access_token);
                    if (tcr.IsError)
                    {
                        return new ApiResult(false, tcr.ErrMsg);
                    }
                    return new ApiResult(true, tcr.Trade.Status);
                }
                catch (Exception e)
                {
                    throw new Exception("TradeCloseRequest Failure.", e);
                }
            }
            /// <summary>
            /// 设置库存
            /// </summary>
            /// <param name="ItemID"></param>
            /// <param name="Quantity"></param>
            public ApiResult UpdateItemQty(long ItemID, long Quantity, Models.UserTaoOAuth taoUserOAuth)
            {
                try
                {
                    ITopClient client = new DefaultTopClient(url_api, this.AppKey, this.AppSecret);
                    ItemQuantityUpdateRequest request = new ItemQuantityUpdateRequest
                    {
                        NumIid = ItemID,
                        Quantity = Quantity
                    };
                    ItemQuantityUpdateResponse rsp = client.Execute<ItemQuantityUpdateResponse>(request, taoUserOAuth.access_token);
                    return new ApiResult(!rsp.IsError, rsp.ErrMsg + " " + rsp.SubErrMsg);
                }
                catch (Exception e)
                {
                    throw new Exception("ItemQuantityUpdateRequest Failure.", e);
                }
            }
            /// <summary>
            /// 获取订单状态
            /// </summary>
            /// <param name="Tid"></param>
            /// <returns></returns>
            public string GetStatus(long Tid, Models.UserTaoOAuth taoUserOAuth)
            {
                try
                {
                    string result = "拦截失败";
                    ITopClient client = new DefaultTopClient(url_api, this.AppKey, this.AppSecret);
                    TradeGetRequest request = new TradeGetRequest
                    {
                        Fields = "status",
                        Tid = Tid
                    };
                    TradeGetResponse response = client.Execute<TradeGetResponse>(request, taoUserOAuth.access_token);
                    if (response.Trade.Status == "TRADE_CLOSED")
                    {
                        result = "拦截成功[退款关单]";
                    }
                    else if (response.Trade.Status == "TRADE_CLOSED_BY_TAOBAO")
                    {
                        result = "拦截成功[直接关单]";
                    }
                    else
                    {
                        result = "拦截失败[" + response.Trade.Status + "]";
                    }
                    return result;
                }
                catch (Exception e)
                {
                    throw new Exception("TradeGetRequest Failure.", e);
                }
            }

            public ApiResult CanBeClose(long tid, Models.UserTaoOAuth taoUserOAuth)
            {
                bool result = false;
                try
                {
                    ITopClient client = new DefaultTopClient(url_api, this.AppKey, this.AppSecret);
                    TradeGetRequest request = new TradeGetRequest
                    {
                        Fields = "status",
                        Tid = tid
                    };
                    TradeGetResponse response = client.Execute<TradeGetResponse>(request, taoUserOAuth.access_token);
                    if (response.Trade.Status == "TRADE_NO_CREATE_PAY")//没有创建支付宝交易
                    {
                        result = true;
                    }
                    else if (response.Trade.Status == "WAIT_BUYER_PAY")//等待买家付款
                    {
                        result = true;
                    }
                    return new ApiResult(result, response.Trade.Status);
                }
                catch (Exception e)
                {
                    throw new Exception("TradeGetRequest Failure.", e);
                }
            }
            /// <summary>
            /// 同步已卖出的交易数据
            /// </summary>
            /// <param name="orders"></param>
            /// <param name="orderOpr"></param>
            /// <param name="status"></param>
            /// <param name="pageno"></param>
            /// <param name="daysago"></param>
            /// <returns></returns>
            public ApiPagedResult<List<TopTrade>> SyncTrade(string status, long pageno, DateTime start, Models.UserTaoOAuth taoUserOAuth)
            {
                var orderList = new List<TbOrder>();
                ITopClient client = new DefaultTopClient(url_api, this.AppKey, this.AppSecret);
                TradesSoldGetRequest request = new TradesSoldGetRequest
                {
                    Fields = "tid,buyer_nick,num_iid,created,pay_time,payment,receiver_address,status,end_time,seller_rate,seller_can_rate"
                };
                request.StartCreated = start;
                request.EndCreated = DateTime.Now;
                request.Status = status;
                request.Type = "guarantee_trade";
                request.PageNo = pageno;
                request.PageSize = 50;
                request.UseHasNext = true;

                TradesSoldGetResponse response = client.Execute<TradesSoldGetResponse>(request, taoUserOAuth.access_token);
                var result = new ApiPagedResult<List<TopTrade>>(!response.IsError, response.ErrMsg + " " + response.SubErrMsg);
                if (!response.IsError)
                {
                    result.Data = response.Trades.Select(x => TopTrade.FromTrade(x)).ToList();
                    result.HasMore = response.HasNext;
                }
                return result;
                //XmlDocument doc = new XmlDocument();
                //response.Trades.First().ser
                //doc.LoadXml(response.Body);
                ////string json = JsonConvert.SerializeXmlNode(doc);
                //XmlNodeList xnltrades = doc.SelectNodes("//trade");
                //if (xnltrades != null && xnltrades.Count > 0)
                //{
                //    foreach (XmlNode xntrade in xnltrades)
                //    {
                //        string jsontrade = JsonConvert.SerializeXmlNode(xntrade);
                //        var definition = new { trade = new { buyer_nick = "", created = new DateTime(), num_iid = "", tid = 0L, pay_time = (DateTime?)null, payment = 0.00, receiver_address = "", status = "" } };
                //        //{"trade":{"buyer_nick":"恋上你回眸","created":"2014-09-18 01:46:15","num_iid":"35071709812","tid":"807930976731116"}}
                //        var trade = JsonConvert.DeserializeAnonymousType(jsontrade, definition);

                //        TbOrder order = new TbOrder()
                //        {
                //            Tid = trade.trade.tid,
                //            Name = trade.trade.buyer_nick,
                //            BuyTime = trade.trade.created,
                //            Itemid = trade.trade.num_iid,
                //            Payment = trade.trade.payment,
                //            PayTime = trade.trade.pay_time,
                //            Status = trade.trade.status,
                //            ReceiverAddress = trade.trade.receiver_address,
                //            trade.seller_rate
                //        };
                //        orderList.Add(order);
                //    }
                //}
                //return new ApiPagedResult<List<TbOrder>>(true, "")
                //{
                //    HasMore = response.HasNext,
                //    Data = orderList
                //};
            }

            internal ApiResult Traderate(long tid, UserTaoOAuth taoOAuth)
            {
                ITopClient client = new DefaultTopClient(url_api, AppKey, AppSecret);
                TraderateAddRequest req = new TraderateAddRequest();
                req.Tid = tid;
                //req.Oid = 1234L;
                req.Result = "good";
                req.Role = "seller";
                //req.Content = "好评！";
                req.Anony = false;
                TraderateAddResponse rsp = client.Execute(req, taoOAuth.access_token);
                return new ApiResult(!rsp.IsError, rsp.ErrMsg + " " + rsp.SubErrMsg);
            }

            public class TbOrder : IEqualityComparer<TbOrder>
            {
                public long Tid { get; set; }//淘宝订单号（唯一标识）
                public string Name { get; set; }//买家名称
                public DateTime BuyTime { get; set; }
                public DateTime? PayTime { get; set; }
                public string Itemid { get; set; }
                public double Payment { get; set; }
                public string ReceiverAddress { get; set; }
                public string Status { get; internal set; }

                public bool Equals(TbOrder x, TbOrder y)    //比较x和y对象是否相同，按照订单号比较
                {
                    return x.Tid == y.Tid;
                }

                public int GetHashCode(TbOrder obj)
                {
                    return obj.ToString().GetHashCode();
                }
            }

        }
    }
}

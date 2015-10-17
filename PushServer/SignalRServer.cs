using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using PushServer.MessageHubs;
using PushServer.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PushServer
{
    public class SignalRServer : IEnumerable
    {
        static SignalRServer()
        {
            _inst = new Lazy<SignalRServer>(
               () => new SignalRServer());
            Instance = _inst.Value;
        }
        public readonly static SignalRServer Instance;
        private readonly static Lazy<SignalRServer> _inst;

        public IHubContext<IMessageClient> MessageClient;
        public IHubContext<ItemMessageClient> TaoItemMessageClient;
        public IHubContext<ITaoRefundMessageClient> TaoRefundMessageClient;
        public IHubContext<ITradeMessageClient> TaoTradeMessageClient;
        public IHubContext TaoTradeRateMessageClient;

        private SignalRServer()
        {
            MessageClient = GlobalHost.ConnectionManager.GetHubContext<MessageHub, IMessageClient>();
            TaoItemMessageClient = GlobalHost.ConnectionManager.GetHubContext<ItemMessageHub, ItemMessageClient>();
            TaoTradeMessageClient = GlobalHost.ConnectionManager.GetHubContext<TradeMessageHub, ITradeMessageClient>();
            TaoRefundMessageClient = GlobalHost.ConnectionManager.GetHubContext<RefundMessageHub, ITaoRefundMessageClient>();
            TaoTradeRateMessageClient = GlobalHost.ConnectionManager.GetHubContext<TradeRateMessageHub>();
        }

        public void PushMessage(Top.Tmc.Message msg)
        {
            try
            {
                //按照卖家名称将消息分发给客户端，如果该卖家不在系统中，则忽略该消息。
                using (var db = new ApplicationDbContext())
                {
                    if (db.UserTaoOAuths.Any(x => x.taobao_user_nick == msg.UserNick))
                    {
                        var connections = db.Connections.Where(x => x.Connected && x.User.UserName == msg.UserNick);
                        var connIds = connections.Select(x => x.ConnectionID).ToList();
                        if (connIds.Any())
                        {
                            var mInfoName = "";
                            var mInfoNames = new List<string>();
                            try
                            {
                                foreach (dynamic item in this)
                                {
                                    var proxyInstance = (object)item.Clients(connIds);
                                    var type = proxyInstance.GetType();
                                    var methodInfos = type.GetMethods();
                                    mInfoNames.AddRange(methodInfos.Select(x => x.Name));
                                    var methodName = new string(msg.Topic.Skip(msg.Topic.LastIndexOf("_") + 1).ToArray());
                                    var method = methodInfos.FirstOrDefault(x => x.Name == methodName);
                                    if (method != null)
                                    {
                                        mInfoName = method.Name;
                                        method.Invoke(proxyInstance, BindingFlags.Default, null, new object[] { msg }, Thread.CurrentThread.CurrentCulture);
                                        return;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Clients.Clients(connIds).OnMessage("动态调用方法" + mInfoName + "时发生错误，错误信息：" + ex.Message);
                            }
                            //Clients.Clients(connIds).OnMessage("消息" + msg.Topic + "未处理。找过的处理方法有：" + string.Join(",", mInfoNames));
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        public IEnumerator GetEnumerator()
        {
            yield return MessageClient.Clients;
            yield return TaoItemMessageClient.Clients;
            yield return TaoTradeMessageClient.Clients;
            yield return TaoRefundMessageClient.Clients;
            //yield return TaoTradeRateMessageClient.Clients;
        }

        public IHubConnectionContext<IMessageClient> Clients
        {
            get
            {
                return this.MessageClient.Clients;
            }
        }
    }
}

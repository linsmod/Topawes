using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Top.Api.Request;
using TopModel.Models;
using WinFormsClient.Models;

namespace WinFormsClient
{
    public class MessageHubClient
    {
        public event Action<Exception> HubProxyInvokeException;
        public TopModel.MessageHubClients.ItemHubProxy ItemHub { get; private set; }
        public TopModel.MessageHubClients.TaoRefundHubProxy RefundHub { get; private set; }
        public TopModel.MessageHubClients.TradeHubProxy TradeHub { get; private set; }
        public TopModel.MessageHubClients.TradeRateHubProxy TradeRateHub { get; private set; }
        public event Action<string> onMessage;
        public event Action<bool> onTopManagerState;
        public event Action<string> onTmcState;
        public event Action<string[]> onKeyValues;

        public IHubProxy HubProxy { get; private set; }
        public MessageHubClient(HubConnection conn)
        {
            ItemHub = new TopModel.MessageHubClients.ItemHubProxy(conn);
            RefundHub = new TopModel.MessageHubClients.TaoRefundHubProxy(conn);
            TradeHub = new TopModel.MessageHubClients.TradeHubProxy(conn);
            TradeRateHub = new TopModel.MessageHubClients.TradeRateHubProxy(conn);
            HubProxy = conn.CreateHubProxy("MessageHub");
            HubProxy.On("onTopManagerState", (x) => { if (onTopManagerState != null) onTopManagerState(x); });
            HubProxy.On("onTmcState", (x) => { if (onTmcState != null) onTmcState(x); });
            HubProxy.On<string>("onMessage", (x) => { if (onMessage != null) onMessage(x); });
            HubProxy.On<string[]>("onKeyValues", (x) => { if (onKeyValues != null) onKeyValues(x); });
            HubProxy.On<string, string>("onSoftwareLicenseNotify", (softwareId, message) =>
            {
                MessageBox.Show(message, "软件授权提示（" + softwareId + "）");
                Application.Exit();
            });
        }

        public async Task<ApiResult> Traderate(long tid)
        {
            return await ProxyInvoke<ApiResult>("traderate", tid);
        }

        public async Task<ApiPagedResult<List<TopTrade>>> SyncTrade(string status, long pageno, DateTime start)
        {
            return await ProxyInvoke<ApiPagedResult<List<TopTrade>>>("syncTrade", status, pageno, start);
        }

        /// <summary>
        /// 如果交易状态为TRADE_NO_CREATE_PAY（没有创建支付宝交易）或者WAIT_BUYER_PAY（等待买家付款）就关闭交易
        /// </summary>
        /// <param name="tradeId"></param>
        /// <returns></returns>
        public async Task<ApiResult> TradeCloseIfTradeGetSuccess(long tradeId)
        {
            return await ProxyInvoke<ApiResult>("tradeCloseIfTradeGetSuccess", tradeId);
        }

        public async Task<ApiResult<Top.Api.Domain.Trade>> GetTradeById(long tradeId)
        {
            return await ProxyInvoke<ApiResult<Top.Api.Domain.Trade>>("getTradeById", tradeId);
        }

        /// <summary>
        /// 检查授权情况，匿名权限可用
        /// </summary>
        public async Task<bool> Authorize()
        {
            return await ProxyInvoke<bool>("authorize");
        }

        /// <summary>
        /// 获取已开通的tmc消息topic
        /// </summary>
        public async Task TmcUserGet()
        {
            await ProxyInvoke("tmcUserGet");
        }

        /// <summary>
        /// 发送新消息
        /// </summary>
        /// <param name="message"></param>
        public async Task<string> NewMessage(string message)
        {
            return await ProxyInvoke<string>("newMessage", message);
        }

        /// <summary>
        /// 广播一条消息，管理员权限可用
        /// </summary>
        /// <param name="message"></param>
        public async Task Broadcast(string message)
        {
            await ProxyInvoke("broadcast", message);
        }

        /// <summary>
        /// 取消监听
        /// </summary>
        public async Task TmcUserCancel()
        {
            await ProxyInvoke("tmcUserCancel");
        }

        /// <summary>
        /// 监听消息
        /// </summary>
        public async Task<ApiResult> TmcGroupAddThenTmcUserPermit()
        {
            return await ProxyInvoke<ApiResult>("tmcGroupAddThenTmcUserPermit");
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        public Task<dynamic> UserInfo()
        {
            return ProxyInvoke<dynamic>("userInfo");
        }


        protected async Task<T> ProxyInvoke<T>(string name, params object[] args)
        {
            return await HubProxy.Invoke<T>(name, args);
        }
        protected async Task ProxyInvoke(string name, params object[] args)
        {
            await HubProxy.Invoke(name, args);
        }

        protected void InvokeEvent<T>(Action<T> proxyEvent, T value)
        {
            if (proxyEvent != null)
            {
                proxyEvent.Invoke(value);
            }
        }
    }


}

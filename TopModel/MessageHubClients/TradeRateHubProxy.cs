using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNet.SignalR.Client;
using TopModel.Models;
using System.Threading.Tasks;

namespace TopModel.MessageHubClients
{
    public class TradeRateHubProxy : HubProxyInvoker
    {
        public TradeRateHubProxy(HubConnection connection) : base(connection, "TradeRateMessageHub")
        {
        }

        public async Task<ApiResult> TradeRate(long itemId)
        {
            return await ProxyInvoke<ApiResult>("TradeRate", itemId);
        }
    }
}

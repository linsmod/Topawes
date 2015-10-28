using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TopModel.MessageHubClients
{
    public abstract class HubProxyInvoker
    {
        public IHubProxy HubProxy { get; private set; }
        public HubConnection HubConnection { get; private set; }
        public HubProxyInvoker(HubConnection connection, string hubName)
        {
            HubConnection = connection;
            HubProxy = connection.CreateHubProxy(hubName);
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
                try
                {
                    var cts = new CancellationTokenSource();
                    cts.CancelAfter(TimeSpan.FromSeconds(5));
                    var t = Task.Factory.StartNew((x) => proxyEvent((T)x), value);
                    t.Wait(cts.Token);
                }
                catch (Exception ex)
                {

                }
                //proxyEvent.Invoke(value);
            }
        }
    }
}

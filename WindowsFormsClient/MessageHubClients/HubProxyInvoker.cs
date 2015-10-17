using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsClient.MessageHubClients
{
    public abstract class HubProxyInvoker
    {
        public event Action<Exception> HubProxyInvokeException;
        public IHubProxy HubProxy { get; private set; }
        public HubConnection HubConnection { get; private set; }
        public HubProxyInvoker(HubConnection connection, string hubName)
        {
            HubConnection = connection;
            HubProxy = connection.CreateHubProxy(hubName);
        }
        protected async Task<T> ProxyInvoke<T>(string name, params object[] args)
        {
            try
            {
                return await HubProxy.Invoke<T>(name, args);
            }
            catch (Exception ex)
            {
                OnHubProxyInvokeException(ex);
                return default(T);
            }
        }

        protected async Task ProxyInvoke(string name, params object[] args)
        {
            try
            {
                await HubProxy.Invoke(name, args);
            }
            catch (Exception ex)
            {
                OnHubProxyInvokeException(ex);
            }
        }

        private void OnHubProxyInvokeException(Exception ex)
        {
            var h = this.HubProxyInvokeException;
            if (h != null)
            {
                h(ex); return;
            }
            throw ex;
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

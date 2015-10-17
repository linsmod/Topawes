using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using PushServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Top.Api;
using Top.Tmc;
namespace PushServer.MessageHubs
{
    public abstract class TopawesHub<T> : Hub<T> where T : class
    {
        public string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public Dictionary<string, MethodInfo> methodInfos = new Dictionary<string, MethodInfo>();
        public ApplicationUserManager UserManager = new ApplicationUserManager(new UserStore<ApplicationUser>(new ApplicationDbContext()));
        public TopawesHub()
        {
            var type = typeof(T);
            var methods = type.GetMethods(System.Reflection.BindingFlags.Public);
            foreach (var item in methods)
            {
                methodInfos[item.Name] = item;
            }
        }

        public void HandleMessage(Message msg)
        {
            //按照卖家名称将消息分发给客户端，如果该卖家不在系统中，则忽略该消息。
            using (var db = new ApplicationDbContext())
            {
                if (db.UserTaoOAuths.Any(x => x.taobao_user_nick == msg.UserNick))
                {
                    var connections = db.Connections.Where(x => x.Connected && x.User.UserName == msg.UserNick);
                    var connIds = connections.Select(x => x.ConnectionID).ToList();
                    var proxyInstance = Clients.Clients(connIds);
                    var methodName = new string(msg.Topic.Skip(msg.Topic.LastIndexOf("_")).ToArray());
                    if (methodInfos.Keys.Any(x => x == methodName))
                    {
                        methodInfos[methodName].Invoke(proxyInstance, BindingFlags.Default, null, new object[] { msg }, Thread.CurrentThread.CurrentCulture);
                    }
                }
            }
        }

        protected string AccessToken
        {
            get
            {
                var user = UserManager.FindByName(Context.User.Identity.Name);
                if (user != null)
                {
                    return user.TaoOAuth.access_token;
                }
                return string.Empty;
            }
        }

        protected ITopClient GetTopClient()
        {
            return TopManager.GetTopClient();
        }

        public override Task OnConnected()
        {
            var userId = Context.User.Identity.GetUserId();
            if (userId != null)
            {
                using (var db = new ApplicationDbContext())
                {
                    var connId = this.Context.ConnectionId;
                    var conn = db.Connections.Find(connId);
                    if (conn == null)
                    {
                        conn = new SignalRConnection
                        {
                            ConnectionID = connId,
                            Connected = true,
                            LastConnectDate = DateTime.Now,
                            UserId = userId,
                            UserAgent = Context.Request.Headers["User-Agent"],
                        };
                        db.Connections.Add(conn);
                    }
                    else
                    {
                        if (!conn.Connected)
                            conn.Connected = true;
                    }
                    db.SaveChanges();
                }
            }
            // Add your own code here.
            // For example: in a chat application, record the association between
            // the current connection ID and user name, and mark the user as online.
            // After the code in this method completes, the client is informed that
            // the connection is established; for example, in a JavaScript client,
            // the start().done callback is executed.
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            using (var db = new ApplicationDbContext())
            {
                var connection = db.Connections.Find(Context.ConnectionId);
                if (connection != null)
                {
                    connection.Connected = false;
                    db.SaveChanges();
                }
            }
            // Add your own code here.
            // For example: in a chat application, mark the user as offline, 
            // delete the association between the current connection id and user name.
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            using (var db = new ApplicationDbContext())
            {
                var connection = db.Connections.Find(Context.ConnectionId);
                if (connection != null)
                {
                    connection.Connected = true;
                    connection.LastConnectDate = DateTime.Now;
                    db.SaveChanges();
                }
            }
            // Add your own code here.
            // For example: in a chat application, you might have marked the
            // user as offline after a period of inactivity; in that case 
            // mark the user as online again.
            return base.OnReconnected();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UserManager.Dispose();
            }
            base.Dispose(disposing);
        }
    }
    public abstract class TopawesHub : Hub
    {
        public ApplicationUserManager UserManager = new ApplicationUserManager(new UserStore<ApplicationUser>(new ApplicationDbContext()));
        protected string AccessToken
        {
            get
            {
                var user = UserManager.FindByName(Context.User.Identity.Name);
                if (user != null)
                {
                    return user.TaoOAuth.access_token;
                }
                return string.Empty;
            }
        }

        protected ITopClient GetTopClient()
        {
            return TopManager.GetTopClient();
        }
        public override Task OnConnected()
        {
            var userId = Context.User.Identity.GetUserId();
            if (userId != null)
            {
                using (var db = new ApplicationDbContext())
                {
                    var connId = this.Context.ConnectionId;
                    var conn = db.Connections.Find(connId);
                    if (conn == null)
                    {
                        conn = new SignalRConnection
                        {
                            ConnectionID = connId,
                            Connected = true,
                            LastConnectDate = DateTime.Now,
                            UserId = userId,
                            UserAgent = Context.Request.Headers["User-Agent"],
                        };
                        db.Connections.Add(conn);
                    }
                    else
                    {
                        if (!conn.Connected)
                            conn.Connected = true;
                    }
                    db.SaveChanges();
                }
            }
            // Add your own code here.
            // For example: in a chat application, record the association between
            // the current connection ID and user name, and mark the user as online.
            // After the code in this method completes, the client is informed that
            // the connection is established; for example, in a JavaScript client,
            // the start().done callback is executed.
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            using (var db = new ApplicationDbContext())
            {
                var connection = db.Connections.Find(Context.ConnectionId);
                connection.Connected = false;
                db.SaveChanges();
            }
            // Add your own code here.
            // For example: in a chat application, mark the user as offline, 
            // delete the association between the current connection id and user name.
            return base.OnDisconnected(stopCalled);

        }

        public override Task OnReconnected()
        {
            using (var db = new ApplicationDbContext())
            {
                var connection = db.Connections.Find(Context.ConnectionId);
                if (connection != null)
                {
                    connection.Connected = true;
                    connection.LastConnectDate = DateTime.Now;
                    db.SaveChanges();
                }
            }
            // Add your own code here.
            // For example: in a chat application, you might have marked the
            // user as offline after a period of inactivity; in that case 
            // mark the user as online again.
            return base.OnReconnected();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UserManager.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

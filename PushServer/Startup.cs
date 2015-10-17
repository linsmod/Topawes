using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;
using PushServer.Models;
using System;
using System.Configuration;
using System.Threading;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Microsoft.AspNet.Identity.EntityFramework;

[assembly: OwinStartup(typeof(PushServer.Startup))]
[assembly: OwinStartupAttribute(typeof(PushServer.Startup))]
namespace PushServer
{
    public partial class Startup
    {
        public static string AppKey = "23140690";
        public static string AppSecret = "a84b819688969ee00b5ae44a19b3f1f0";
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.UseCors(CorsOptions.AllowAll);

            InitializeAppEnvironment();
            InitializeSignalRConnState();

            //GlobalHost.DependencyResolver.UseSqlServer(ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString());
            var hubConfiguration = new HubConfiguration();
            hubConfiguration.EnableDetailedErrors = true;
            hubConfiguration.EnableJavaScriptProxies = false;
            app.MapSignalR("/signalr", hubConfiguration);
            //var hub = GlobalHost.ConnectionManager.GetHubContext<MessageHub>();
            TopManager.Initialize(new TopManager.ApiServiceAccount(AppKey, AppSecret));
        }

        private void InitializeAppEnvironment()
        {
            //app environment configuration
            using (var db = new ApplicationDbContext())
            {
                var roleStore = new RoleStore<IdentityRole>(db);
                var role = roleStore.FindByNameAsync("Admin").Result;
                if (role == null)
                {
                    roleStore.CreateAsync(new IdentityRole("Admin")).Wait();
                }

                var userStore = new UserStore<ApplicationUser>(db);
                var manager = new ApplicationUserManager(userStore);

                var admin = manager.FindByName("admin");
                if (admin == null)
                {
                    admin = new ApplicationUser
                    {
                        UserName = "admin",
                        Email = "admin@sandsea.info",
                        EmailConfirmed = true,
                        CreateDate = DateTime.Now
                    };
                    var r = manager.CreateAsync(admin, "~Pwd123456").Result;
                }
                if (!manager.IsInRole(admin.Id, role.Name))
                {
                    manager.AddToRole(admin.Id, role.Name);
                }
            }
        }

        private void InitializeSignalRConnState()
        {
            using (var db = new ApplicationDbContext())
            {
                //connnection state reset
                var conns = db.Connections.ToList();
                foreach (var conn in conns)
                {
                    conn.Connected = false;
                }
                db.SaveChanges();
            }
        }
    }
}

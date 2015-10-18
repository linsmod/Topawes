using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PushServer.Models
{
    // 可以通过向 ApplicationUser 类添加更多属性来为用户添加配置文件数据。若要了解详细信息，请访问 http://go.microsoft.com/fwlink/?LinkID=317594。
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // 请注意，authenticationType 必须与 CookieAuthenticationOptions.AuthenticationType 中定义的相应项匹配
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // 在此处添加自定义用户声明
            return userIdentity;
        }

        public virtual DateTime CreateDate { get; set; }

        public virtual ICollection<SignalRConnection> Connections { get; set; }

        public virtual ICollection<SoftwareLicense> SoftwareLicenses { get; set; }

        public virtual UserTaoOAuth TaoOAuth { get; set; }
    }



    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationUser>().HasOptional(x => x.TaoOAuth).WithRequired(x => x.User).Map(x => x.MapKey("UserId"));

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(x => x.SoftwareLicenses)
                .WithRequired(x => x.User)
                .HasForeignKey(x => x.UserId);
            base.OnModelCreating(modelBuilder);
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public virtual DbSet<SignalRConnection> Connections { get; set; }

        public virtual DbSet<UserTaoOAuth> UserTaoOAuths { get; set; }

        public virtual DbSet<Software> Softwares { get; set; }

        public virtual DbSet<SoftwareLicense> SoftwareLicenses { get; set; }
    }

    /// <summary>
    /// signalR connection
    /// </summary>
    public class SignalRConnection
    {
        [Key]
        public string ConnectionID { get; set; }
        public DateTime LastConnectDate { get; set; }
        public string UserAgent { get; set; }
        public bool Connected { get; set; }
        public string AppName { get; set; }
        public string AppVersion { get; set; }
        public string AppCreationTime { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
    }

    /// <summary>
    /// 淘宝OAuthToken
    /// </summary>
    public class UserTaoOAuth
    {
        [Key]
        public string taobao_user_nick { get; set; }

        public string token_type { get; set; }

        public string refresh_token { get; set; }
        public int re_expires_in { get; set; }

        public string access_token { get; set; }
        public int expires_in { get; set; }

        public int r1_expires_in { get; set; }
        public int r2_expires_in { get; set; }

        public int w1_expires_in { get; set; }
        public int w2_expires_in { get; set; }

        public DateTime UpdateAt { get; set; }

        public virtual ApplicationUser User { get; set; }
    }

    /// <summary>
    /// 软件许可证
    /// </summary>
    public class SoftwareLicense
    {
        public string Id { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime? Expires { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public string SoftwareId { get; set; }
        [ForeignKey("SoftwareId")]
        public virtual Software Software { get; set; }
    }

    /// <summary>
    /// 软件
    /// </summary>
    public class Software
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
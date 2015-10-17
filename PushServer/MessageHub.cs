using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using PushServer.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using Top.Api.Request;
using Top.Api;
using TopModel.Models;
using PushServer.MessageHubs;

namespace PushServer
{
    public interface IMessageClient
    {
        //服务信息
        void OnMessage(string message);
        //Tmc消息
        void OnTmcMessage(Top.Tmc.Message msg);
        //TopManager状态
        void OnTopManagerState(bool initialized);
        //Tmc状态变更
        void OnTmcState(string state);
        //返回存储结果
        void OnKeyValues(string[] keyValues);

        void OnSoftwareLicenseNotify(string softwareId, string message);
    }

    public class MessageHub : TopawesHub<IMessageClient>
    {
        private const string SoftwareId = "23140690";

        [Authorize]
        public ApiResult Traderate(long tid)
        {

            if (!NotifyIfLicensExpired(Context.User.Identity.Name, SoftwareId))
            {
                var user = UserManager.FindByName(Context.User.Identity.Name);
                return TopManager.TopOperation.Traderate(tid, user.TaoOAuth);
            }
            return new ApiPagedResult<List<Top.Api.Domain.Trade>>(false, "操作无法继续，请检查软件授权情况");
        }

        [Authorize]
        public ApiPagedResult<List<TopTrade>> SyncTrade(string status, int pageno, DateTime start)
        {
            if (!NotifyIfLicensExpired(Context.User.Identity.Name, SoftwareId))
            {
                var userId = Context.User.Identity.GetUserId();
                var user = UserManager.FindById(userId);
                return TopManager.TopOperation.SyncTrade(status, pageno, start, user.TaoOAuth);
            }
            return new ApiPagedResult<List<TopTrade>>(false, "操作无法继续，请检查软件授权情况");
        }


        /// <summary>
        /// 关闭订单
        /// </summary>
        /// <param name="tradeId"></param>
        /// <returns></returns>
        [Authorize]
        public ApiResult TradeCloseIfTradeGetSuccess(long tradeId)
        {
            if (!NotifyIfLicensExpired(Context.User.Identity.Name, SoftwareId))
            {
                var user = UserManager.FindByName(Context.User.Identity.Name);
                var resp = TopManager.TopOperation.CanBeClose(tradeId, user.TaoOAuth);
                if (!resp.Success)
                {
                    return new ApiResult(false, "订单状态不正确，状态=" + resp.Message);
                }
                return TopManager.TopOperation.CloseOrderByApi(tradeId, "其他原因", user.TaoOAuth);
            }
            return new ApiResult(false, "操作无法继续，请检查软件授权情况");
        }

        [Authorize]
        public object UserInfo()
        {
            var userId = Context.User.Identity.GetUserId();
            var user = UserManager.FindById(userId);
            var license = user.SoftwareLicenses.FirstOrDefault(x => x.SoftwareId == SoftwareId);
            DateTime? licenseExpires = license == null ? (DateTime?)null : license.Expires;
            return new { UserName = Context.User.Identity.Name, LicenseExpires = licenseExpires, TopManagerInitialized = TopManager.Initialized };
        }

        public bool Authorize()
        {
            return Context.User.Identity.IsAuthenticated;
        }

        [Authorize]
        public void SaveKeyValues(List<string> keyValues)
        {
            if (!NotifyIfLicensExpired(Context.User.Identity.Name, SoftwareId))
            {
                var userId = Context.User.Identity.GetUserId();
                System.IO.File.WriteAllLines(System.IO.Path.Combine(BaseDirectory, userId + ".data"), keyValues);
                this.NewMessage("配置上载完成！");
            }
        }

        [Authorize]
        public string[] LoadKeyValues()
        {
            if (!NotifyIfLicensExpired(Context.User.Identity.Name, SoftwareId))
            {
                var userId = Context.User.Identity.GetUserId();
                var userDataPath = System.IO.Path.Combine(BaseDirectory, userId + ".data");
                if (System.IO.File.Exists(userDataPath))
                {
                    var lines = System.IO.File.ReadAllLines(userDataPath);
                    return lines;
                }
            }
            return new string[0];
        }

        [Authorize(RequireOutgoing = false)]
        public string NewMessage(string message)
        {
            Clients.Caller.OnMessage(this.Context.User.Identity.Name + ":" + message);
            return "N.";
        }

        [Authorize(Roles = "Admin")]
        public void Broadcast(string message)
        {
            Clients.All.OnMessage(message);
        }

        [Authorize]
        public ApiResult TmcGroupAddThenTmcUserPermit()
        {
            var user = UserManager.FindByName(Context.User.Identity.Name);
            return TopManager.TmcGroupAddThenTmcUserPermit(user.TaoOAuth.taobao_user_nick, user.TaoOAuth.access_token);
        }

        //no stop,only cancel msg
        [Authorize]
        public void TmcUserCancel()
        {
            var user = UserManager.FindByName(Context.User.Identity.Name);
            TopManager.TmcUserCancel(user.TaoOAuth.taobao_user_nick);
        }

        [Authorize]
        public void TmcUserGet()
        {
            if (!NotifyIfLicensExpired(Context.User.Identity.Name, SoftwareId))
            {
                var user = UserManager.FindByName(Context.User.Identity.Name);
                string msg = TopManager.TopOperation.TmcUserGet(user.TaoOAuth);
                Clients.Caller.OnMessage("已开通以下消息:" + msg);
            }
        }

        [Authorize]
        public ApiResult SetItemQty(ItemQuantityUpdateRequest req)
        {
            if (!NotifyIfLicensExpired(Context.User.Identity.Name, SoftwareId))
            {
                var user = UserManager.FindByName(Context.User.Identity.Name);

                return TopManager.TopOperation.UpdateItemQty(req.NumIid.Value, req.Quantity.Value, user.TaoOAuth);
            }
            return new ApiResult(false, "操作无法继续，请检查软件授权情况");
        }
        [Authorize]
        public ApiResult<Top.Api.Domain.Trade> GetTradeById(long tradeId)
        {
            if (!NotifyIfLicensExpired(Context.User.Identity.Name, SoftwareId))
            {
                var db = new ApplicationDbContext();
                var user = UserManager.FindByName(Context.User.Identity.Name);
                var trade = TopManager.TopOperation.GetTradeByTid(tradeId, user.TaoOAuth);
                return new ApiResult<Top.Api.Domain.Trade>(true, "") { Data = trade };
            }
            return new ApiResult<Top.Api.Domain.Trade>(false, "操作无法继续，请检查软件授权情况");
        }

        /// <summary>
        /// 过期了就返回true
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="softwareId"></param>
        /// <returns></returns>
        private bool NotifyIfLicensExpired(string userName, string softwareId)
        {
            var user = UserManager.FindByName(userName);
            var sli = user.SoftwareLicenses.FirstOrDefault(x => x.SoftwareId == softwareId);
            if (sli == null)
            {
                Clients.User(userName).OnSoftwareLicenseNotify(softwareId, "软件未授权");
            }
            else if (sli.Expires < DateTime.Now)
            {
                Clients.User(userName).OnSoftwareLicenseNotify(softwareId, "软件授权过期");
            }
            else
            {
                return false;
            }
            return true;
        }
    }

}
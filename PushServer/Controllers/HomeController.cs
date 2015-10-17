using Codeplex.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Top.Api.Util;
using Microsoft.AspNet.Identity.Owin;
using PushServer.Models;
using System.Net;

namespace PushServer.Controllers
{
    public class HomeController : Controller
    {
        public static string AppKey = "23140690";
        public static string AppSecret = "a84b819688969ee00b5ae44a19b3f1f0";

        public ApplicationUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
        }


        [Authorize]
        public ActionResult Index(string redirectUrl)   
        {
            return View();
        }

        public async Task<ActionResult> AuthorizeCallback(string code, string state)
        {
            if (code != null)
            {
                string url = "https://oauth.taobao.com/token";
                Dictionary<string, string> props = new Dictionary<string, string>();
                props.Add("grant_type", "authorization_code");
                props.Add("code", code);
                props.Add("client_id", AppKey);
                props.Add("client_secret", AppSecret);
                props.Add("redirect_uri", "http://123.56.122.122:8080/home/AuthorizeCallback");
                props.Add("view", "web");
                string s = "";
                try
                {
                    WebUtils webUtils = new WebUtils();
                    s = webUtils.DoPost(url, props);
                    dynamic x = DynamicJson.Parse(s);
                    var taobao_user_nick = HttpUtility.UrlDecode(x.taobao_user_nick);
                    //UpdateToken(taobao_user_nick, x.access_token, x.refresh_token);
                    return Redirect(string.Format("http://container.open.taobao.com/container?appkey={0}", 23140690));
                }
                catch (IOException e)
                {
                    return Content(e.Message);
                }
            }
            else if (Request["top_appkey"] != null)
            {
                var appKey = Request["top_appkey"];
                var db = new ApplicationDbContext();
                try
                {
                    var software = db.Softwares.Find(appKey);
                    if (software == null)
                    {
                        software = new Software { Id = appKey, Name = appKey };
                        db.Softwares.Add(software);
                        db.SaveChanges();
                    }

                    var paramList = Encoding.GetEncoding("gb2312").GetString(Convert.FromBase64String(Request["top_parameters"]));
                    var list = paramList.Split('&').Select(x => x.Split('=')).ToList();
                    var dict = new Dictionary<string, string>();
                    dict["access_token"] = Request["top_session"];
                    foreach (var item in list)
                    {
                        if (item.Length == 2) dict.Add(item[0], item[1]); else dict.Add(item[0], "");
                    }
                    var userNick = dict["visitor_nick"];
                    var user = await UserManager.FindByNameAsync(userNick);
                    if (user == null)
                    {
                        user = new ApplicationUser
                        {
                            UserName = userNick,
                            Email = userNick + "@sandsea.info",
                            EmailConfirmed = true,
                            CreateDate = DateTime.Now
                        };
                        var result = await UserManager.CreateAsync(user, "~Pwd123456");
                        if (result.Succeeded)
                        {
                            if (AppSettings.AutoAddSoftwareLicenseWhenAddUser)
                            {
                                //首次添加的用户自动授权1个月
                                db.SoftwareLicenses.Add(new SoftwareLicense
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    UserId = user.Id,
                                    SoftwareId = software.Id,
                                    CreateDate = DateTime.Now,
                                    Expires = DateTime.Now.AddDays(AppSettings.AutoLicenseDays),
                                });
                                await db.SaveChangesAsync();
                            }

                            //保存OAuth会话
                            var userOAuth = GetUserTaoOAuth(dict);
                            user.TaoOAuth = userOAuth;
                            await UserManager.UpdateAsync(user);

                            var siginStatus = await SignInManager.PasswordSignInAsync(userNick, "~Pwd123456", false, false);
                            ViewBag.Information = "siginStatus=" + siginStatus;
                        }
                        else
                        {
                            ViewBag.Information = "identityResult=" + string.Join(";", result.Errors);
                        }
                    }
                    else
                    {
                        var license = user.SoftwareLicenses.FirstOrDefault(x => x.SoftwareId == software.Id);
                        if (license == null)
                        {
                            ViewBag.Title = "尚未授权，请联系客服。";
                            return View();
                        }
                        else if (license.Expires < DateTime.Now)
                        {
                            ViewBag.Title = "授权过期，请联系客服。";
                            return View();
                        }

                        //更新OAuth会话
                        UpdateUserTaoOAuth(user.TaoOAuth, dict);
                        await UserManager.UpdateAsync(user);
                        //isPersistent must be true,otherwise cannot get cookies for client webbrowser control
                        var siginStatus = await SignInManager.PasswordSignInAsync(userNick, "~Pwd123456", false, false);
                        return RedirectToAction("Index", new { redirectUrl = "http://121.41.161.45:30000/Login.aspx" + Request.Url.Query });
                    }
                }
                catch (Exception ex)
                {
                    var errors = db.GetValidationErrors();
                    return Content(ex.Message + ex.StackTrace);
                }
                return RedirectToAction("Index", new { redirectUrl = "http://121.41.161.45:30000/Login.aspx" + Request.Url.Query });
                //string user_id = TaoBaoAPI.GetParameters(Request["top_parameters"], "visitor_id");
                //string user_nick = TaoBaoAPI.GetParameters(Request["top_parameters"], "visitor_nick");
                //string sub_user_nick = TaoBaoAPI.GetParameters(Request["top_parameters"], "sub_taobao_user_nick");
            }
            else
            {
                return null;
            }
        }

        public dynamic RefreshAccessToken(string refreshToken)
        {
            WebUtils webUtils = new WebUtils();
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("grant_type", "refresh_token");
            param.Add("refresh_token", refreshToken);
            param.Add("client_id", AppKey);
            param.Add("client_secret", AppSecret);
            param.Add("view", "web");
            param.Add("state", new Random().Next(1000).ToString());
            try
            {
                String responseJson = webUtils.DoPost("https://oauth.taobao.com/token ", param);
                dynamic d = DynamicJson.Parse(responseJson);
                return d;
                //DebugInfo = responseJson;
            }
            catch (WebException ex)
            {
                var streamReader = new StreamReader(ex.Response.GetResponseStream());
                throw new Exception(streamReader.ReadToEnd());
            }

        }

        private UserTaoOAuth GetUserTaoOAuth(Dictionary<string, string> dict)
        {
            var taoOAuth = new UserTaoOAuth();
            UpdateUserTaoOAuth(taoOAuth, dict);
            return taoOAuth;
        }

        private void UpdateUserTaoOAuth(UserTaoOAuth taoOAuth, Dictionary<string, string> dict)
        {
            if (taoOAuth.taobao_user_nick == null)
            {
                taoOAuth.taobao_user_nick = dict["visitor_nick"];
            }
            taoOAuth.access_token = dict["access_token"];
            taoOAuth.expires_in = int.Parse(dict["expires_in"]);
            taoOAuth.r1_expires_in = int.Parse(dict["r1_expires_in"]);
            taoOAuth.r2_expires_in = int.Parse(dict["r2_expires_in"]);
            taoOAuth.refresh_token = dict["refresh_token"];
            taoOAuth.re_expires_in = int.Parse(dict["re_expires_in"]);
            taoOAuth.token_type = "Bearer";
            taoOAuth.w1_expires_in = int.Parse(dict["w1_expires_in"]);
            taoOAuth.w2_expires_in = int.Parse(dict["w2_expires_in"]);
            taoOAuth.UpdateAt = DateTime.Now;
        }
    }
}
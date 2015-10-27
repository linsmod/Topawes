using Codeplex.Data;
using LiteDB;
using Microsoft.AspNet.SignalR.Client;
using Moonlight;
using Moonlight.Helpers;
using Moonlight.WindowsForms.Controls;
using MSHTML;
using Newtonsoft.Json;
using Nx.EasyHtml.Html;
using Nx.EasyHtml.Html.Parser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TopModel;
using TopModel.Models;
using WinFormsClient.Extensions;
using WinFormsClient.Helpers;
using WinFormsClient.Models;
using WinFormsClient.Properties;
using WinFormsClient.WBMode;

namespace WinFormsClient
{

    public partial class WinFormsClient : BaseForm
    {
        private static X509Certificate2 signingCertificate;
        private Dictionary<string, string> TaoAuthorizedCookieDictionary = new Dictionary<string, string>();
        public JumonyParser HtmlParser = new JumonyParser();
        public AppQueue<InterceptInfo> InteceptQueue;
        public AppQueue<ProductItem, SimpleResult> SyncSupplierQueue;
        const string IndexUrl = "http://chongzhi.taobao.com/index.do?spm=0.0.0.0.OR0khk&method=index";
        public TimeSpan DateDifference = TimeSpan.Zero;
        public static CancellationTokenSource cts = new CancellationTokenSource();
        public bool IsAutoRateRunning = false;
        public string tbcpCrumbs = "";
        private MessageHubClient client { get; set; }
        //public const string Server = "http://localhost:62585/";
        //public const string Server = "http://localhost:8090/";
        public const string Server = "http://123.56.122.122:8080/";
        private HubConnection Connection { get; set; }
        public bool IsLogin { get; private set; }
        public static WBTaoLoginState wbLoginMode = new WBTaoLoginState();
        WBTaoDurexValidationState wbTaoDurexValidateMode = new WBTaoDurexValidationState();
        WBTaoChongZhiBrowserState wbTaoChongZhiMode = new WBTaoChongZhiBrowserState();
        ExtendedWinFormsWebBrowser wb = new ExtendedWinFormsWebBrowser();
        public string title = "淘充值防牛工具";
        public static WinFormsClient Instance;
        internal WinFormsClient()
        {
            Instance = this;
            this.Text = title;
            SyncSupplierQueue = new AppQueue<ProductItem, SimpleResult>(cts.Token);
            InteceptQueue = new AppQueue<InterceptInfo>(cts.Token);
            InstallCert();
            wbLoginMode.UserCancelLogin += () => { this.Close(); };
            wbTaoChongZhiMode.AskLogin += () => { this.InvokeAction(this.ShowLoginWindow, wb); };
            var files = Directory.EnumerateFiles(Application.StartupPath, "*.bin");
            if (files.Any())
            {
                var selectAccountForm = new SelectAccountForm();
                foreach (var fName in files)
                {
                    var uName = Path.GetFileNameWithoutExtension(fName);
                    var button = new Button()
                    {
                        Size = new Size(140, 30),
                        Text = uName,
                    };
                    button.Click += OnSelectAccountButtonClick;
                    selectAccountForm.AccountButtons.Add(button);
                }
                selectAccountForm.ShowDialog(this);
            }
            InitializeComponent();
            wbLoginMode.Enter(wb);
            var x = new TaoLoginForm(wbLoginMode).ShowDialog(this);
            if (x != DialogResult.OK)
            {
                Environment.Exit(0);
            }
            LoadMenuItemSetting();
            this.OnLoginSuccess();
            tabPage2.Controls.Add(wb);
            WBHelper.InitWBHelper(wbArrayWrapper, "http://chongzhi.taobao.com/index.do?spm=0.0.0.0.OR0khk&method=index");
        }

        private void InstallCert()
        {
            signingCertificate = CertificateHelper.GetCertificate("O=Alipay.com Corporation, OU=SecurityCenter, CN=Alipay Trust NetWork", StoreName.Root, StoreLocation.CurrentUser);
            if (signingCertificate == null)
            {
                bool? installResult = null;
                new Thread(delegate ()
                {
                    while (!installResult.HasValue)
                    {
                        SendKeys.SendWait("%Y");
                    }
                }).Start();
                if (!(installResult = CertificateHelper.InstallCertificate(Resources.Alipay_Trust_Network, StoreName.Root, StoreLocation.CurrentUser)).Value)
                {
                    MessageBox.Show("用户取消了证书的安装。");
                    Environment.Exit(0);
                }
            }
        }

        private void OnSelectAccountButtonClick(object sender, EventArgs e)
        {
            var userName = (sender as Button).Text;
            AppSetting.InitializeUserSetting(userName, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, userName + ".bin"));
            ((sender as Button).Parent.Parent as Form).Close();
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            AppendException(e.Exception);
            foreach (var item in e.Exception.InnerExceptions)
            {
                AppendException(item);
            }
        }
        private void ResetTaskProgress()
        {
            this.InvokeAction(() =>
            {
                this.progressBar1.Value = 0;
                this.labelTaskName.Text = "就绪";
            });
        }
        private void ReportTaskProgress(string name, int index, int total)
        {
            this.InvokeAction(() =>
            {
                this.labelTaskName.Text = name;
                this.progressBar1.Value = index + 1;
                this.progressBar1.Maximum = total;
            });
        }
        private async Task OnLoginSuccess()
        {
            await TaskEx.Delay(500);
            this.InvokeAction(async () =>
            {
                using (var wbHelper = new WBHelper(false))
                {
                    await this.InvokeTask(wbHelper.SynchronousLoadDocument, IndexUrl);
                    TaoAuthorizedCookieDictionary = CookieHelper.GetAllCookie(wbHelper.WB);
                    WBHelper.Cookie = CookieHelper.ExpandCookieDictionary(TaoAuthorizedCookieDictionary);
                    AppSetting.UserSetting.Set("淘充值饼干", WBHelper.Cookie, true);
                    AppSetting.UserSetting.Set("淘充值饼干时间", DateTime.Now);

                    var apicookie = IECookieHelper.GetGlobalCookie(Server, ".AspNet.ApplicationCookie");
                    AppSetting.UserSetting.Set("API饼干", apicookie, true);
                    AppSetting.UserSetting.Set("API饼干时间", DateTime.Now);

                    IsLogin = true;
                }
                wbLoginMode.TransitionToNext(wbTaoChongZhiMode, wb);
                AppendText("服务登录完成！");
                /*使用cookie连接*/
                ConnectAsync();
            });
        }

        private async Task InterceptTrade(InterceptInfo info)
        {
            SuplierInfo supplier = info.supplier;
            string spu = info.spuId;
            TopTrade trade = info.trade;
            Statistic statistic = info.statistic;
            AppendText("将拦截订单{0}，买家：{1}", trade.Tid, trade.BuyerNick);
            trade.Intercept = true;
            AppDatabase.db.TopTrades.Update(trade);
            var product = AppDatabase.db.ProductItems.FindById(trade.NumIid);

            //更新价格
            using (var helper = new WBHelper(false))
            {
                try
                {
                    if (!await this.SetProductProfit(product, (x) => 0, forceModify: true))
                    {
                        statistic.InterceptFailed++;
                        AppDatabase.db.Statistics.Upsert(statistic, statistic.Id);
                        OnStatisticUpdate(statistic);
                    }
                    else
                    {
                        AppendText("商品{0}改价已提交", trade.NumIid);

                        //1分钟后关单
                        await TaskEx.Delay(1000 * 60 - 500);

                        AppendText("{0}关闭交易...", trade.Tid);
                        await CloseTradeIfPossible(trade.Tid);
                    }
                }
                catch (Exception ex)
                {
                    AppendException(ex);
                }
            }
        }


        /// <summary>
        /// 查询供应商信息(AJAX)
        /// </summary>
        /// <param name="wb"></param>
        /// <param name="spu"></param>
        /// <returns></returns>
        public static async Task<SuplierInfo> supplierInfo(WBHelper wbHelper, string spu)
        {
            var url = "http://chongzhi.taobao.com/item.do?spu={0}&action=edit&method=supplierInfo&_=" + DateTime.Now.Ticks;
            url = string.Format(url, spu);
            var content = await wbHelper.WB.ExecuteTriggerJSONP(url);
            var doc = (HTMLDocument)wbHelper.WB.Document.DomDocument;
            var cnt = (IHTMLDOMNode)doc.getElementById("jsonpcontent");
            if (cnt != null)
                cnt.removeNode();
            var suplierInfo = JsonConvert.DeserializeObject<SuplierInfo>(content);
            if (suplierInfo.profitData != null && suplierInfo.profitData.Any())
            {
                //查到供应商
                return suplierInfo;
            }
            else
            {
                //无供应商,平台默认会自动关单
                return null;
            }
        }
        /// <summary>
        /// 设置供应商(非AJAX)
        /// </summary>
        /// <param name="wb"></param>
        /// <param name="sup"></param>
        /// <param name="spu"></param>
        /// <param name="profitMin"></param>
        /// <param name="profitMax"></param>
        /// <param name="price">一口价</param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public async Task<TaoJsonpResult> supplierSave(WBHelper helper, string sup, string spu, string profit, string price, long itemId, string tbcpCrumbs)
        {
            //profitMode=0 保证我赚钱
            //profitMode=2 自定义
            var url = "http://chongzhi.taobao.com/item.do?method=supplierSave&sup={0}&mode=2&spu={1}&itemId={2}&profitMode=2&profitMin={3}&profitMax={4}&price={5}&tbcpCrumbs={6}";
            url = string.Format(url, sup, spu, itemId, profit, profit, price, tbcpCrumbs);
            var content = await helper.SynchronousLoadString(url);
            if (content.LoginRequired)
            {
                this.InvokeAction(ShowLoginWindow, helper.WB);
                return await supplierSave(helper, sup, spu, profit, price, itemId, tbcpCrumbs);
            }
            return JsonConvert.DeserializeObject<TaoJsonpResult>(content.Data);
        }

        private async Task CloseTradeIfPossible(long tid)
        {
            try
            {
                var topTrade = AppDatabase.db.TopTrades.FindById(tid);
                if (topTrade == null || !topTrade.Intercept)
                    return;
                var statistic = AppDatabase.db.Statistics.FindById(AppSetting.UserSetting.UserName);

                if (topTrade.Status == "TRADE_NO_CREATE_PAY" || topTrade.Status == "WAIT_BUYER_PAY" || string.IsNullOrEmpty(topTrade.Status))
                {
                    //等待付款
                    try
                    {
                        var result = await client.TradeHub.CloseTrade(topTrade.Tid, AppSetting.UserSetting.Get<string>("关单原因"));
                    }
                    catch (Exception ex)
                    {
                        AppendException(ex);
                    }
                    return;
                }
                if (topTrade.Status == "TRADE_CLOSED" || topTrade.Status == "TRADE_CLOSED_BY_TAOBAO")
                {
                    //交易关闭

                    statistic.InterceptSuccess++;
                    AppDatabase.db.Statistics.Update(statistic);
                    OnStatisticUpdate(statistic);
                    AppendText("{0}关闭交易成功，买家：{1}", topTrade.Tid, topTrade.BuyerNick);
                }
                else
                {
                    //交易成功

                    statistic.InterceptFailed++;
                    AppDatabase.db.Statistics.Update(statistic);
                    OnStatisticUpdate(statistic);
                    AppendText("{0}关闭交易失败，买家：{1}，订单状态：{2}", topTrade.Tid, topTrade.BuyerNick, topTrade.Status);
                }

                //恢复价格
                var product = AppDatabase.db.ProductItems.FindById(topTrade.NumIid);
                if (!product.NeedResotreProfit)
                {
                    //尝试还原价格时原始价格已经更新的话就不用恢复了
                    return;
                }

                using (var wbHelper = new WBHelper(false))
                {
                    var taoResult = await this.InvokeTask(supplierSave, wbHelper, product.SupplierId, product.SpuId, product.原利润.ToString("f2"), (product.进价 + product.原利润).ToString("f2"), topTrade.NumIid, tbcpCrumbs);
                    if (taoResult == null || taoResult.status != 200)
                    {
                        AppendText("商品{0}恢复价格失败，请注意！" + taoResult != null ? taoResult.msg : "", topTrade.NumIid);
                        return;
                    }
                    else
                    {
                        AppendText("商品{0}恢复价格已提交", topTrade.NumIid);
                    }
                }
            }
            catch (Exception ex)
            {
                AppendException(ex);
            }
        }
        private async void ButtonSend_Click(object sender, EventArgs e)
        {
            if (TextBoxMessage.Text == "")
            {
                MessageBox.Show("请输入内容！");
                return;
            }
            var h = await client.NewMessage(TextBoxMessage.Text);
            TextBoxMessage.Text = String.Empty;
            TextBoxMessage.Focus();
        }
        private void ButtonConnect_Click(object sender, EventArgs e)
        {
            ButtonSend.Enabled = false;
            ConnectAsync();
        }

        private async Task<ApiPagedResult<List<TopTrade>>> DownloadTbOrderList(string state, DateTime start)
        {
            ApiPagedResult<List<TopTrade>> tradeList = new ApiPagedResult<List<TopTrade>>(true, "") { HasMore = true };
            int num = 1;
            List<TopTrade> list = new List<TopTrade>();
            while (tradeList.HasMore)
            {
                tradeList = await client.SyncTrade(state, num, start);
                if (!tradeList.Success)
                {
                    return new ApiPagedResult<List<TopTrade>>(false, tradeList.Message);
                }
                list.AddRange(tradeList.Data);
                num++;
            }
            AppendText("{0} SUCCESS.({1}个)", state, list.Count);
            return new ApiPagedResult<List<TopTrade>>() { Data = list };
        }

        /// <summary>
        /// Creates and connects the hub connection and hub proxy. This method
        /// is called asynchronously from SignInButton_Click.
        /// </summary>
        private async void ConnectAsync()
        {
            if (Connection != null)
            {
                Connection = null;
            }
            Connection = new HubConnection(Server);
            var asm = this.GetType().Assembly.GetName();
            Connection.Headers["x-name"] = asm.Name;
            Connection.Headers["x-version"] = asm.Version.ToString();
            Connection.Headers["x-creation-time"] = new System.IO.FileInfo(Application.ExecutablePath).CreationTime.ToString();
            Connection.Headers["x-os-version"] = Environment.OSVersion.VersionString + "(" + Environment.OSVersion.Platform.ToString() + ")";
            Connection.Headers["x-ie-version"] = wb.Version.ToString();
            Connection.Headers["x-runtime-version"] = Environment.Version.ToString();
            Connection.Error += Connection_Error;
            Connection.Closed += Connection_Closed;
            Connection.Reconnecting += Connection_Reconnecting;
            Connection.Reconnected += Connection_Reconnected;
            Connection.StateChanged += Connection_StateChanged;
            Connection.CookieContainer = new System.Net.CookieContainer();
            var cookie = IECookieHelper.GetGlobalCookie(Server, ".AspNet.ApplicationCookie");
            Connection.CookieContainer.SetCookies(new Uri(Server), ".AspNet.ApplicationCookie=" + cookie);
            client = new MessageHubClient(Connection);

            //常规事件处理
            client.ItemHub.ItemAdd += ItemHub_ItemAdd;
            client.ItemHub.ItemDelete += ItemHub_ItemDelete;
            client.ItemHub.ItemDownshelf += ItemHub_ItemDownshelf;
            client.ItemHub.ItemStockChanged += ItemHub_ItemStockChanged;
            client.ItemHub.ItemUpdate += ItemHub_ItemUpdate;
            client.ItemHub.ItemUpshelf += ItemHub_ItemUpshelf;
            client.ItemHub.ItemZeroStock += ItemHub_ItemZeroStock;

            client.RefundHub.RefundBlockMessage += RefundHub_RefundBlockMessage;
            client.RefundHub.RefundBuyerModifyAgreement += RefundHub_RefundBuyerModifyAgreement;
            client.RefundHub.RefundBuyerReturnGoods += RefundHub_RefundBuyerReturnGoods;
            client.RefundHub.RefundClosed += RefundHub_RefundClosed;
            client.RefundHub.RefundCreated += RefundHub_RefundCreated;
            client.RefundHub.RefundCreateMessage += RefundHub_RefundCreateMessage;
            client.RefundHub.RefundSellerAgreeAgreement += RefundHub_RefundSellerAgreeAgreement;
            client.RefundHub.RefundSellerRefuseAgreement += RefundHub_RefundSellerRefuseAgreement;
            client.RefundHub.RefundSuccess += RefundHub_RefundSuccess;
            client.RefundHub.RefundTimeoutRemind += RefundHub_RefundTimeoutRemind;

            client.TradeHub.TradeBuyerPay += TradeHub_TradeBuyerPay;
            client.TradeHub.TradeClose += TradeHub_TradeClose;
            client.TradeHub.TradeCreate += TradeHub_TradeCreate;

            client.onMessage += (message) => { AppendText(String.Format("{0}: {1}" + Environment.NewLine, "[服务端信息]", message)); };
            client.onTmcState += (state) => { AppendText(String.Format("{0}: {1}" + Environment.NewLine, "交易监控", state)); };
            try
            {
                if (!GetServerTime())
                {
                    MessageBox.Show("网络错误！");
                    ExitConfirmed = true;
                    this.Close();
                    return;
                }
                this.InvokeAction(() =>
                {
                    //登录完成后导航到这个页面，方便后面AJAX直接使用这个浏览器取数据
                    //wbMain.Navigate("http://chongzhi.taobao.com/index.do?spm=0.0.0.0.OR0khk&method=index");
                    wb.Navigate("http://chongzhi.taobao.com/index.do?spm=0.0.0.0.OR0khk&method=index");
                });
                try
                {
                    await Connection.Start();
                }
                catch
                {
                    AppendText("尝试连接失败，10s后重试...");
                    await TaskEx.Delay(1000 * 10);
                    ConnectAsync();
                    return;
                }
                this.tssl_ConnState.Text = "连接状态：" + ConnectionState.Connected.AsZhConnectionState();
                IsLogin = true;
                var userInfo = await client.UserInfo();
                var userName = (string)userInfo.UserName;
                AppDatabase.Initialize(userName);
                AppSetting.InitializeUserSetting(userName, AppDatabase.db.Database);
                LoadUserSetting();
                SetupTaskbarIcon();
                AppDatabase.db.Statistics.Delete(userName);
                AppDatabase.db.Statistics.Insert(new Statistic { Id = userName });
                var statistic = AppDatabase.db.Statistics.FindById(userName);
                OnStatisticUpdate(statistic);
                this.InvokeAction(() =>
                {
                    this.Text = title + " [" + asm.Version + "] 授权给：" + userName +
                    " 过期时间：" + userInfo.LicenseExpires;
                });
                if (userInfo.LicenseExpires == null)
                {
                    MessageBox.Show("软件授权无效，疑问、咨询或购买请联系客服");
                    this.Close();
                    return;
                }
                if (DateTime.Now > (DateTime)userInfo.LicenseExpires)
                {
                    MessageBox.Show("软件授权过期，疑问或续期请联系客服");
                    this.Close();
                    return;
                }
                //this.InvokeAction(() =>
                //{
                //    wbMain.ScriptErrorsSuppressed = true;
                //    WBHelper.wbQueue.Enqueue(wbMain);
                //});
                var taoInfo = await this.InvokeTask(GetTaoInfo);
                if (taoInfo.status == 200)
                {
                    AppSetting.UserSetting.Set("TaoInfo", taoInfo);
                    ResetProductStates();
                    BindDGViewProduct();
                    BindDGViewTBOrder();
                    var permit = await client.TmcGroupAddThenTmcUserPermit();
                    var permitSuccess = permit.Success;
                    this.AppendText("消息授权{0}", permitSuccess ? "成功" : "失败，错误消息：" + permit.Message);
                    if (permitSuccess)
                    {
                        this.InvokeAction(() =>
                        {
                            tabControl1.Enabled = true;
                        });
                    }
                    else
                    {
                        MessageBox.Show("消息授权失败，请联系客服。");
                        this.ExitConfirmed = true;
                        this.Close();
                        return;
                    }
                    SyncBackground();
                }
                else
                {
                    AppendText("网络错误！");
                }
            }
            catch (Exception ex)
            {
                AppendException(ex);
            }
        }
        private void ResetProductStates()
        {
            foreach (var item in AppDatabase.db.ProductItems.FindAll())
            {
                item.ModifyProfitSubmitted = false;
                item.SyncProfitSubmited = false;
                AppDatabase.db.ProductItems.Update(item);
            }
        }

        private async Task SyncBackground()
        {
            await SyncTradeListIncrease(true);
            var success = await SyncAllProductList();
            if (success)
            {
                ThreadLoopMonitorDelay();
            }
        }

        private void SetupTaskbarIcon()
        {
            if (this.notifyIcon1.Icon == null)
            {
                this.notifyIcon1.Icon = (Icon)this.Icon.Clone();
                this.notifyIcon1.Text = AppSetting.UserSetting.UserName;
                this.notifyIcon1.BalloonTipText = AppSetting.UserSetting.UserName;
                this.notifyIcon1.DoubleClick += (s, e) =>
                {
                    this.ShowInTaskbar = true;
                    this.Show();
                    this.WindowState = FormWindowState.Normal;
                };
            }
        }

        private async Task<SimpleResult> SyncSupplierInfo(ProductItem product)
        {
            using (var helper = new WBHelper(true))
            {
                var success = false;
                //查找供应商
                var url = string.Format("http://chongzhi.taobao.com/item.do?spu={0}&action=edit&method=supplierInfo&_=" + DateTime.Now.Ticks, product.SpuId);
                var x = await this.InvokeTask(helper.IsLoginRequired);
                while (x)
                {
                    this.InvokeAction(ShowLoginWindow, helper.WB);
                    x = await this.InvokeTask(helper.IsLoginRequired);
                }
                var content = await this.InvokeTask(helper.WB.ExecuteTriggerJSONP, url);
                while (content == "{\"code\":999}")
                {
                    this.InvokeAction(ShowLoginWindow, helper.WB);
                    content = await this.InvokeTask(helper.WB.ExecuteTriggerJSONP, url);
                }
                if (!string.IsNullOrEmpty(content))
                {
                    var supplier = JsonConvert.DeserializeObject<SuplierInfo>(content);
                    if (supplier.profitData.Any())
                    {
                        product.OnSupplierInfoUpdate(AppDatabase.db.ProductItems, supplier);
                        success = true;
                    }
                }
                return new SimpleResult { Success = success, ProductId = product.Id };
            }
        }

        /// <summary>
        /// 同步供应商
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private async Task SyncSupplierInfo(IEnumerable<ProductItem> args)
        {
            var list = args.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                var product = list[i];
                await SyncSupplierInfo(product);
                ReportTaskProgress("正在处理后台任务...", i, list.Count);
            }
            ResetTaskProgress();
            BindDGViewProduct();
        }

        private async void TradeHub_TradeCreate(Top.Tmc.Message msg)
        {
            //AppendText(msg.Topic);
            //AppendText(msg.Content);

            //解析内容
            var definition = new { buyer_nick = "", payment = "", oid = 0L, tid = 0L, type = "guarantee_trade", seller_nick = "" };
            //var content = @"{'buyer_nick':'包花呗','payment':'9.56','oid':1316790655492656,'tid':1316790655492656,'type':'guarantee_trade','seller_nick':'红指甲与高跟鞋'}";
            var d = JsonConvert.DeserializeAnonymousType(msg.Content, definition);
            var trade = (await client.TradeHub.GetTrade(d.tid)).Data;
            var product = AppDatabase.db.ProductItems.FindById(trade.NumIid);
            if (product == null)
            {
                AppendText("忽略非平台商品订单{0}", trade.Tid);
                return;
            }


            //更新订单列表
            AppDatabase.db.TopTrades.Upsert(trade, trade.Tid);
            AppendText("[{0}]交易创建：买家：{1}", d.tid, d.buyer_nick);

            //更新统计信息
            var statistic = AppDatabase.db.Statistics.FindById(d.seller_nick) ?? new Statistic { Id = d.seller_nick };
            statistic.BuyCount++;
            AppDatabase.db.Statistics.Upsert(statistic, statistic.Id);
            OnStatisticUpdate(statistic);

            SuplierInfo supplier = null;
            if (!string.IsNullOrEmpty(product.SupplierId))
            {
                supplier = product.GetSuplierInfo();
            }
            else
            {
                using (var wbHelper = new WBHelper(true))
                {
                    try
                    {
                        var x = await this.InvokeTask(wbHelper.IsLoginRequired);
                        while (x)
                        {
                            this.InvokeAction(ShowLoginWindow, wbHelper.WB);
                            x = await this.InvokeTask(wbHelper.IsLoginRequired);
                        }
                        //查找供应商
                        supplier = await this.InvokeTask(supplierInfo, wbHelper, product.SpuId);
                        if (supplier != null)
                        {
                            product.OnSupplierInfoUpdate(AppDatabase.db.ProductItems, supplier);
                        }
                    }
                    catch (Exception ex)
                    {
                        AppendException(ex);
                    }
                }
            }
            if (supplier != null)
            {
                if (ServerNow.HasValue)
                {
                    //5分钟之前的消息即过期的下单信息不处理
                    if ((ServerNow.Value - trade.Created).TotalMinutes > 5)
                    {
                        AppendText("[{0}/{1}]不拦截过期交易。", trade.Tid, trade.NumIid);
                        return;
                    }
                }

                var interceptType = AppSetting.UserSetting.Get<string>("拦截模式");
                if (interceptType == InterceptMode.无条件拦截模式)
                {
                    InteceptQueue.CreateTaskItem(new InterceptInfo(supplier, product.SpuId, trade, statistic), InterceptTrade);
                    return;
                }
                else if (interceptType == InterceptMode.仅拦截亏本交易)
                {
                    if (supplier.profitMin < 0)
                    {
                        InteceptQueue.CreateTaskItem(new InterceptInfo(supplier, product.SpuId, trade, statistic), InterceptTrade);
                    }
                    else
                        AppendText("[{0}/{1}]不拦截-{2}。", trade.Tid, trade.NumIid, interceptType);
                    return;
                }
                else if (interceptType == InterceptMode.智能拦截模式)
                {
                    //若14天内同充值号码
                    var number = new Regex(@"(?<=号码:|用户名:)(\d{5,12})\b").Match(trade.ReceiverAddress ?? "").Value;
                    if (!string.IsNullOrEmpty(number))
                    {
                        var countNumUsed = AppDatabase.db.TopTrades.Count(x => x.ReceiverAddress.Contains(number) && x.NumIid == trade.NumIid);
                        if (countNumUsed > 1)
                        {
                            //AppendText("14天内同充值号码拦截，订单ID={0}", trade.Tid);
                            InteceptQueue.CreateTaskItem(new InterceptInfo(supplier, product.SpuId, trade, statistic), InterceptTrade);
                            return;
                        }
                    }

                    //14天内同宝贝付款订单大于1
                    var orderCount = AppDatabase.db.TopTrades.Count(x => x.BuyerNick == trade.BuyerNick && x.NumIid == trade.NumIid);
                    if (orderCount > 1)
                    {
                        //AppendText("14天内同宝贝付款订单大于1拦截，订单ID={0}", trade.Tid);
                        InteceptQueue.CreateTaskItem(new InterceptInfo(supplier, product.SpuId, trade, statistic), InterceptTrade);
                        return;
                    }

                    //买家是白名单内买家,不拦截
                    if (AppSetting.UserSetting.Get("买家白名单", new string[0]).Any(x => x == trade.BuyerNick))
                    {
                        AppendText("[{0}/{1}]不拦截-{2}。", trade.Tid, trade.NumIid, interceptType);
                        return;
                    }
                    //黑名单买家一律拦截
                    if (AppSetting.UserSetting.Get<string[]>("买家黑名单", new string[0]).Any(x => x == trade.BuyerNick))
                    {
                        //AppendText("黑名单买家拦截，订单ID={0}", trade.Tid);
                        InteceptQueue.CreateTaskItem(new InterceptInfo(supplier, product.SpuId, trade, statistic), InterceptTrade);
                        return;
                    }

                    //购买数量超过1件的拦截
                    if (trade.Num > 1)
                    {
                        //AppendText("购买数量超过1件拦截，订单ID={0}", trade.Tid);
                        InteceptQueue.CreateTaskItem(new InterceptInfo(supplier, product.SpuId, trade, statistic), InterceptTrade);
                        return;
                    }
                    AppendText("[{0}/{1}]不拦截-{2}。", trade.Tid, trade.NumIid, interceptType);
                }
            }
            else
            {
                AppendText("[{0}/{1}]交易商品供应商信息未查询到，不拦截。", trade.Tid, trade.NumIid);
            }
        }

        private async void TradeHub_TradeClose(Top.Tmc.Message msg)
        {
            //AppendText(msg.Topic);
            //AppendText(msg.Content);

            var definition = new { buyer_nick = "", payment = "", oid = 0L, tid = 0L, type = "guarantee_trade", seller_nick = "" };
            //var content = @"{'buyer_nick':'包花呗','payment':'9.56','oid':1316790655492656,'tid':1316790655492656,'type':'guarantee_trade','seller_nick':'红指甲与高跟鞋'}";
            var d = JsonConvert.DeserializeAnonymousType(msg.Content, definition);
            var result = await client.TradeHub.GetTradeStatus(d.tid).ConfigureAwait(false);
            if (result.Success)
            {
                var trade = AppDatabase.db.TopTrades.FindById(d.tid);
                if (trade != null)
                {
                    trade.Status = result.Data;
                    AppDatabase.db.TopTrades.Update(trade);
                    CloseTradeIfPossible(trade.Tid);
                }
            }
        }

        private async void TradeHub_TradeBuyerPay(Top.Tmc.Message msg)
        {
            //AppendText(msg.Topic);
            //AppendText(msg.Content);

            //解析内容
            var definition = new { buyer_nick = "", payment = "", oid = 0L, tid = 0L, type = "guarantee_trade", seller_nick = "" };
            //var content = @"{'buyer_nick':'包花呗','payment':'9.56','oid':1316790655492656,'tid':1316790655492656,'type':'guarantee_trade','seller_nick':'红指甲与高跟鞋'}";
            var d = JsonConvert.DeserializeAnonymousType(msg.Content, definition);
            var trade = (await client.TradeHub.GetTrade(d.tid)).Data;
            AppDatabase.UpsertTopTrade(trade);
            var product = AppDatabase.db.ProductItems.FindById(trade.NumIid);
            if (product == null)
            {
                AppendText("忽略非平台商品订单{0}", trade.Tid);
                return;
            }
            var statistic = AppDatabase.db.Statistics.FindById(d.seller_nick) ?? new Statistic { Id = d.seller_nick };
            statistic.PayCount++;
            AppDatabase.db.Statistics.Upsert(statistic, statistic.Id);
            OnStatisticUpdate(statistic);
            AppendText("[{0}]交易付款，买家：{1} 创建于{2}，付款时间{3}", trade.Tid, trade.BuyerNick, trade.Created.ToString("M月d日 H时m分s秒"), trade.PayTime.Value.ToString("M月d日 H时m分s秒"));
            CloseTradeIfPossible(trade.Tid);
        }

        private void RefundHub_RefundTimeoutRemind(Top.Tmc.Message msg)
        {
            AppendText(msg.Topic);
            AppendText(msg.Content);
        }

        private void RefundHub_RefundSuccess(Top.Tmc.Message msg)
        {
            AppendText(msg.Topic);
            AppendText(msg.Content);
        }

        private void RefundHub_RefundSellerRefuseAgreement(Top.Tmc.Message msg)
        {
            AppendText(msg.Topic);
            AppendText(msg.Content);
        }

        private void RefundHub_RefundSellerAgreeAgreement(Top.Tmc.Message msg)
        {
            AppendText(msg.Topic);
            AppendText(msg.Content);
        }

        private void RefundHub_RefundCreateMessage(Top.Tmc.Message msg)
        {
            AppendText(msg.Topic);
            AppendText(msg.Content);
        }

        private void RefundHub_RefundCreated(Top.Tmc.Message msg)
        {
            AppendText(msg.Topic);
            AppendText(msg.Content);
        }

        private void RefundHub_RefundClosed(Top.Tmc.Message msg)
        {
            AppendText(msg.Topic);
            AppendText(msg.Content);
        }

        private void RefundHub_RefundBuyerReturnGoods(Top.Tmc.Message msg)
        {
            AppendText(msg.Topic);
            AppendText(msg.Content);
        }

        private void RefundHub_RefundBuyerModifyAgreement(Top.Tmc.Message msg)
        {
            AppendText(msg.Topic);
            AppendText(msg.Content);
        }

        private void RefundHub_RefundBlockMessage(Top.Tmc.Message msg)
        {
            AppendText(msg.Topic);
            AppendText(msg.Content);
        }

        private void ItemHub_ItemZeroStock(Top.Tmc.Message msg)
        {
            AppendText(msg.Topic);
            AppendText(msg.Content);
        }

        private async void ItemHub_ItemUpshelf(Top.Tmc.Message msg)
        {
            //AppendText(msg.Topic);
            //AppendText(msg.Content);
            var def = new { num_iid = 522569629924 };
            var d = JsonConvert.DeserializeAnonymousType(msg.Content, def);
            var product = await FindProductById(d.num_iid).ConfigureAwait(false);
            if (product != null)
            {
                AppendText("上架了商品【{0}】", product.ItemName);
                product.OnUpshelf(AppDatabase.db.ProductItems);
            }
            BindDGViewProduct();
        }

        private async void ItemHub_ItemUpdate(Top.Tmc.Message msg)
        {
            //AppendText(msg.Topic + " " + msg.Content);
            //taobao_item_ItemUpdate
            //{ "nick":"cendart","changed_fields":"","num_iid":522571681053} 
            //被删除时会先触发taobao_item_ItemUpdate消息，如果商品在出售中，还会触发下架消息
            var d = DynamicJson.Parse(msg.Content);
            var product = await FindProductById((long)d.num_iid).ConfigureAwait(false);
            if (product != null)
            {
                //提交了改价的才处理
                if (product.ModifyProfitSubmitted)
                {
                    product.ModifyProfitSubmitted = false;
                    AppDatabase.db.ProductItems.Update(product);
                    BindDGViewProduct();
                    SyncSupplierQueue.CreateTaskItem(product, SyncSupplierInfo, x =>
                    {
                        BindDGViewProduct();
                    });
                }

                //恢复价格的,已经在关单的时候提交了请求，这里只需要更新供应商利润信息即可
                if (product.NeedResotreProfit)
                {
                    SyncSupplierQueue.CreateTaskItem(product, SyncSupplierInfo, x =>
                    {
                        BindDGViewProduct();
                    });
                }
            }
        }

        private async void ItemHub_ItemStockChanged(Top.Tmc.Message msg)
        {
            //AppendText(msg.Topic);
            //AppendText(msg.Content);
            //页面新增商品后会触发此消息
            var def = new { increment = 999999, num = 999999, num_iid = 523021091186 };
            var d = JsonConvert.DeserializeAnonymousType(msg.Content, def);
            var product = await FindProductById(d.num_iid);
            if (product != null && product.StockQty != d.num)
            {
                //更新库存
                product.StockQty = d.num;
                AppDatabase.db.ProductItems.Update(product);

                //await SetItemQty(product);
            }
            BindDGViewProduct();
        }

        private async void ItemHub_ItemDownshelf(Top.Tmc.Message msg)
        {
            //AppendText(msg.Topic);
            //AppendText(msg.Content);
            //var def = new { num_iid = 522569629924 };
            var d = DynamicJson.Parse(msg.Content);
            var product = await FindProductById((long)d.num_iid);
            if (product != null)
            {
                product.OnDownshelf(AppDatabase.db.ProductItems);
                AppendText("下架了商品【{0}】", product.ItemName);

                if (product.AutoUpshelf)
                {
                    await UpProduct(product);
                }
            }
            BindDGViewProduct();
        }

        private async void ItemHub_ItemDelete(Top.Tmc.Message msg)
        {
            //貌似不触发这个事件
            //var def = new { num_iid = 522569629924 };
            var d = DynamicJson.Parse(msg.Content);
            var product = await FindProductById((long)d.num_iid);
            AppDatabase.db.ProductItems.Delete(d.num_iid);
            AppendText("删除了商品【{0}】", product.ItemName);
            BindDGViewProduct();
        }

        private void ItemHub_ItemAdd(Top.Tmc.Message msg)
        {
            //AppendText(msg.Topic);
            //AppendText(msg.Content);
            //var def = new { num = 999999, title = "QQ币1个直充", price = "0.97", nick = "cendart", num_iid = 523042737153 };
            //var d = DynamicJson.Parse(msg.Content);
            //AppendText("新增了商品【{0}】", d.title);
            //var product = await FindProductById((long)d.num_iid);
            //if (product == null)
            //{
            //    product = new ProductItem { Id = d.num_iid, ItemName = d.title, 一口价 = d.price, StockQty = d.num };
            //    AppDatabase.db.ProductItems.Upsert((ProductItem)product, (string)d.num_iid);
            //}
            //BindDGViewProduct();
        }

        private Task<ProductItem> FindProductById(long itemId)
        {
            var product = AppDatabase.db.ProductItems.FindById(itemId);
            if (product == null)
            {
                //await SyncAllProductList().ConfigureAwait(false);
            }
            return TaskEx.FromResult(product);
        }
        public class InterceptMode
        {
            public static string 智能拦截模式 = "智能拦截模式";
            public static string 仅拦截亏本交易 = "仅拦截亏本交易";
            public static string 无条件拦截模式 = "无条件拦截模式";
        }
        private void LoadUserSetting()
        {
            this.InvokeAction(() =>
            {
                var lockTypes = new string[] { "锁定所有商品", "锁定监控商品" };
                var intercepts = new string[] { InterceptMode.智能拦截模式, InterceptMode.仅拦截亏本交易, InterceptMode.无条件拦截模式 };
                QQ直充库存锁定数量.SetupAfterUserSettingInitialized("QQ直充库存锁定数量");
                对QQ直充启用库存锁定.SetupAfterUserSettingInitialized("对QQ直充启用库存锁定");
                QQ直充库存锁定类型.SetupAfterUserSettingInitialized("QQ直充库存锁定类型", lockTypes, lockTypes[0]);

                点卡直充库存锁定数量.SetupAfterUserSettingInitialized("点卡直充库存锁定数量");
                对点卡直充启用库存锁定.SetupAfterUserSettingInitialized("对点卡直充启用库存锁定");
                点卡直充库存锁定类型.SetupAfterUserSettingInitialized("点卡直充库存锁定类型", lockTypes, lockTypes[0]);

                话费直充库存锁定数量.SetupAfterUserSettingInitialized("话费直充库存锁定数量");
                对话费直充启用库存锁定.SetupAfterUserSettingInitialized("对话费直充启用库存锁定");
                话费直充库存锁定类型.SetupAfterUserSettingInitialized("话费直充库存锁定类型", lockTypes, lockTypes[0]);

                自动好评交易checkBoxAuto.SetupAfterUserSettingInitialized("交易自动好评");

                赔钱利润.SetupAfterUserSettingInitialized("赔钱利润");
                不陪钱利润.SetupAfterUserSettingInitialized("不赔钱利润");

                拦截模式.SetupAfterUserSettingInitialized("拦截模式", intercepts, intercepts[0]);

                var closeReasons = new string[] { CloseReason.与买家协商一致, CloseReason.买家不想买, CloseReason.买家联系不上, CloseReason.协商不一致, CloseReason.商品瑕疵, CloseReason.未及时付款, CloseReason.谢绝还价 };
                关单原因.SetupAfterUserSettingInitialized("关单原因", closeReasons, CloseReason.未及时付款);
            });
        }

        private async Task<TaoInfo> GetTaoInfo()
        {
            using (var wbHelper = new WBHelper(false))
            {
                var cnt = await this.InvokeTask(wbHelper.SynchronousLoadString, "http://chongzhi.taobao.com/index.do?method=info");
                while (cnt.LoginRequired)
                {
                    this.InvokeAction(ShowLoginWindow, wbHelper.WB);
                    cnt = await this.InvokeTask(wbHelper.SynchronousLoadString, "http://chongzhi.taobao.com/index.do?method=info");
                }
                var info = JsonConvert.DeserializeObject<TaoInfo>(cnt.Data);
                if (info != null && info.status == 200)
                {
                    this.InvokeAction(() =>
                    {
                        toolStripStatusLabel余额.Text = "余额：" + info.alipayBalance.ToString("f2");
                        toolStripStatusLabel橱窗.Text = "橱窗：" + info.promotedCount + "/" + info.promotedMax;
                        toolStripStatusLabel出售中.Text = "出售中：" + info.onCount;
                        toolStripStatusLabel仓库中.Text = "仓库中：" + info.offCount;
                        toolStripStatusLabel价格变动.Text = "价格变动：" + info.priceChangedCount;
                    });
                }
                return info;
            }
        }

        public void ShowLoginWindow(ExtendedWinFormsWebBrowser wbx)
        {
            var wbParent = wb.Parent;
            wbTaoChongZhiMode.TransitionToNext(wbLoginMode, wb);
            if (new TaoLoginForm(wbLoginMode).ShowDialog(this) == DialogResult.OK)
            {
                TaoAuthorizedCookieDictionary = CookieHelper.GetAllCookie(wb);
                WBHelper.Cookie = CookieHelper.ExpandCookieDictionary(TaoAuthorizedCookieDictionary);

                wbLoginMode.TransitionToNext(wbTaoChongZhiMode, wb);
                wb.Navigate(IndexUrl);
                wbParent.Controls.Add(wb);
            }
            else
            {
                ExitConfirmed = true;
                Application.Exit();
            }
        }

        private async Task SyncTradeListIncrease(bool startingApp)
        {
            List<TopTrade> list = new List<TopTrade>();
            DateTime timeStart;
            if (AppDatabase.db.TopTrades.Any() && AppSetting.UserSetting.Get<DateTime?>("LastSyncOrderAt").HasValue)
            {
                //增量更新
                AppendText("同步15天内订单数据（增量）...");
                timeStart = AppSetting.UserSetting.Get<DateTime?>("LastSyncOrderAt").Value;
            }
            else
            {
                //全部更新(14天前的)
                AppendText("同步15天内订单数据（全部）...");
                timeStart = ServerDateTime.AddDays(-14);
            }
            ApiPagedResult<List<TopTrade>> x = await DownloadTbOrderList("WAIT_BUYER_PAY", timeStart);
            if (!x.Success)
            {
                AppendText("同步等待付款订单数据失败，错误消息：" + x.Message);
                return;
            }
            list.AddRange(x.Data);

            x = await DownloadTbOrderList("WAIT_SELLER_SEND_GOODS", timeStart);
            if (!x.Success)
            {
                AppendText("同步等待发货订单数据失败，错误消息：" + x.Message);
                return;
            }
            list.AddRange(x.Data);

            x = await DownloadTbOrderList("WAIT_BUYER_CONFIRM_GOODS", timeStart);
            if (!x.Success)
            {
                AppendText("同步已发货订单数据失败，错误消息：" + x.Message);
                return;
            }
            list.AddRange(x.Data);

            x = await DownloadTbOrderList("TRADE_FINISHED", timeStart);
            if (!x.Success)
            {
                AppendText("同步交易成功订单数据失败，错误消息：" + x.Message);
                return;
            }
            list.AddRange(x.Data);

            list = list.Distinct().ToList();
            AppDatabase.UpsertTbOrderList(list);
            AppSetting.UserSetting.Set<DateTime?>("LastSyncOrderAt", DateTime.Now.AddMinutes(-5));//执行上面的代码会造成延迟

            //更新到UI
            this.InvokeAction(() => { BindDGViewTBOrder(); });

            this.AppendText("同步订单完成（{0}个）", list.Count);

            if (!startingApp && AppSetting.UserSetting.Get<bool>(this.自动好评交易checkBoxAuto.Name))
            {
                await AutoTradeRate();
            }
        }
        private TimeSpan ServerDelay = TimeSpan.Zero;
        private DateTime ServerDateTime
        {
            get
            {
                return DateTime.Now.Subtract(DateDifference);
            }
        }
        private async Task AutoTradeRate()
        {
            var finishedList = AppDatabase.db.TopTrades.Find(x => x.Status == "TRADE_FINISHED" && x.SellerCanRate && !x.SellerRate && x.EndTime > ServerDateTime.AddDays(-15)).ToList();
            for (int i = 0; i < finishedList.Count; i++)
            {
                var item = finishedList[i];
                var result = await client.Traderate(item.Tid);
                if (!result.Success)
                {
                    AppendText("评价订单{0}时出现错误，后续操作已取消。错误消息：{1}", item.Tid, result.Message);
                    break;
                }
                else
                {
                    item.SellerRate = true;
                    AppDatabase.db.TopTrades.Update(item);
                    this.ReportTaskProgress("交易自动好评", i, finishedList.Count);
                    AppendText("自动评价订单{0}完成", item.Tid, result.Message);
                }
            }
            ResetTaskProgress();
        }
        bool IsSyncProductListAll;
        private async Task<bool> SyncAllProductList()
        {
            if (IsSyncProductListAll)
            {
                AppendText("请等待上次同步任务完成...");
                return true;
            }
            try
            {
                bool continueYes = true;
                bool success = false;
                //仓库中
                while (!(success = await SyncInStockProductList()) && continueYes)
                {
                    continueYes = MessageBox.Show("同步仓库中商品失败，是否重试？", Text, MessageBoxButtons.YesNo) == DialogResult.Yes;
                }
                if (success)
                {
                    //出售中
                    while (!(success = await SyncOnSaleProductList()) && continueYes)
                    {
                        continueYes = MessageBox.Show("同步出售中商品失败，是否重试？", Text, MessageBoxButtons.YesNo) == DialogResult.Yes;
                    }
                }
                if (success)
                {
                    this.InvokeAction(() =>
                    {
                        BindDGViewProduct();
                        tabControl1.Enabled = true;
                    });
                }
                AppSetting.UserSetting.Set<DateTime?>("LastSyncProductAt", DateTime.Now);
                IsSyncProductListAll = false;
                return success;
            }
            catch (Exception ex)
            {
                AppendText("同步商品异常，你可以稍后重试。错误消息：{0} 调用堆栈：", ex.Message, ex.StackTrace);
                IsSyncProductListAll = false;
                return false;
            }

        }

        private async Task<bool> SyncInStockProductList()
        {
            return await SyncProductList("仓库中", "http://chongzhi.taobao.com/item.do?method=list&type=1&keyword=&category=0&supplier=0&promoted=0&order=0&desc=0&page=1&size=50");
        }
        private async Task<bool> SyncOnSaleProductList()
        {
            return await SyncProductList("出售中", "http://chongzhi.taobao.com/item.do?method=list&type=0&keyword=&category=0&supplier=0&promoted=0&order=0&desc=0&page=1&size=50");
        }

        private void SetTaskName(string value, params object[] args)
        {
            this.InvokeAction(() =>
            {
                labelTaskName.Text = string.Format(value, args);
            });
        }

        private async Task<bool> SyncProductList(string where, string url)
        {
            var productList = new List<ProductItem>();
            int page = 1;
            IHtmlDocument xdoc = null;
            ApiPagedResult<List<ProductItem>> pagedList = new ApiPagedResult<List<ProductItem>>();
            while (pagedList.Success && pagedList.HasMore)
            {

                var nextUrl = UrlHelper.SetValue(url, "page", page.ToString());
                using (var helper = new WBHelper(false))
                {
                    try
                    {
                        var result = await this.InvokeTask(helper.SynchronousLoadDocument, nextUrl);
                        while (result.LoginRequired)
                        {
                            this.InvokeAction(ShowLoginWindow, helper.WB);
                            result = await this.InvokeTask(helper.SynchronousLoadDocument, nextUrl);
                        }
                        xdoc = HtmlParser.Parse(result.Data.Body.InnerHtml);
                        var table = xdoc.GetElementById("main").FindFirst(".stock-table");
                        pagedList = ProductItemHelper.GetProductItemList(table, page);
                        tbcpCrumbs = xdoc.FindSingle("#tbcpCrumbs").Attribute("value").AttributeValue;
                        xdoc = null;
                        result = null;
                        SetTaskName("同步{0}商品第{1}页（{2}个）", where, page, pagedList.Data.Count);
                        productList.AddRange(pagedList.Data);
                        page++;
                    }
                    catch (Exception ex)
                    {
                        pagedList.Success = false;
                        helper.Dispose();
                        //AppendException(ex);
                    }
                }
                if (!pagedList.Success)
                {
                    AppendText("同步{0}商品第{1}页出错，{2}", where, page, pagedList.Message);
                    if (MessageBox.Show(string.Format("同步{0}商品第{1}页出错，是否重试？", where, page), Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        pagedList.Success = true;
                        continue;
                    }
                }

            }
            var dt = DateTime.Now;
            foreach (var item in productList)
            {
                item.UpdateAt = dt;
                item.Where = where;
                item.原利润 = item.利润 = item.一口价 - item.进价;
            }
            AppDatabase.UpsertProductList(productList);
            BindDGViewProduct();
            SetTaskName("就绪");
            AppendText("同步{0}商品完成！（{1}个）", where, productList.Count);
            return true;
        }
        private void Connection_StateChanged(StateChange state)
        {
            if (!!cts.IsCancellationRequested && !this.IsDisposed)
            {
                this.InvokeAction(() =>
                {
                    this.tssl_ConnState.Text = "连接状态：" + state.NewState.AsZhConnectionState();
                });
                if (state.NewState == ConnectionState.Connected)
                {
                    //Activate UI
                    if (!this.InvokeRequired)
                    {
                        ButtonSend.Enabled = true;
                        ButtonSend.Text = "消息测试";
                        ButtonSend.Click -= ButtonSend_Click;
                        ButtonSend.Click += ButtonSend_Click;
                        ButtonSend.Click -= ButtonConnect_Click;
                        TextBoxMessage.Focus();
                    }
                    else
                    {
                        this.Invoke((Action)(() =>
                        {
                            ButtonSend.Enabled = true;
                            ButtonSend.Text = "消息测试";
                            ButtonSend.Click -= ButtonSend_Click;
                            ButtonSend.Click += ButtonSend_Click;
                            ButtonSend.Click -= ButtonConnect_Click;
                            TextBoxMessage.Focus();
                        }));
                    }
                }
            }
        }
        private void OnStatisticUpdate(Statistic statistic)
        {
            this.InvokeAction(() =>
            {
                toolStripStatusLabel拦截失败.Text = "拦截失败：" + statistic.InterceptFailed;
                toolStripStatusLabel拦截成功.Text = "拦截成功：" + statistic.InterceptSuccess;
                toolStripStatusLabel下单.Text = "下单：" + statistic.BuyCount;
                toolStripStatusLabel付款.Text = "付款：" + statistic.PayCount;
            });
        }

        private void AppendText(string text, params object[] args)
        {
            if (!cts.IsCancellationRequested && !this.IsDisposed)
            {
                if (!RichTextBoxConsole.InvokeRequired)
                {
                    try
                    {
                        text = ServerDateTime.ToString("MM-dd HH:mm:ss") + " " + (args.Any() ? string.Format(text, args) : text);
                        RichTextBoxConsole.AppendText(text + Environment.NewLine);
                        RichTextBoxConsole.ScrollToCaret();
                    }
                    catch { }
                }
                else
                    this.Invoke((Action)(() =>
                    {
                        AppendText(text, args);
                    }));
            }
        }

        private void Connection_Reconnected()
        {
            AppendText("重连成功");
        }

        private void Connection_Reconnecting()
        {
            AppendText("断线重连...");
        }

        private void Connection_Error(Exception obj)
        {
            AppendText("连接错误:{0}", obj.Message);
        }

        /// <summary>
        /// If the server is stopped, the connection will time out after 30 seconds (default), and the 
        /// Closed event will fire.
        /// </summary>
        private void Connection_Closed()
        {
            //Deactivate chat UI; show login UI. 
            this.Invoke((Action)(() =>
            {
                ButtonSend.Enabled = true;
                ButtonSend.Text = "连接消息服务";
                ButtonSend.Click -= ButtonSend_Click;
                ButtonSend.Click -= ButtonConnect_Click;
                ButtonSend.Click += ButtonConnect_Click;
                AppendText("连接已断开");
                ConnectAsync();
            }));
        }

        private async void WinFormsClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!ExitConfirmed && IsLogin)
            {
                if (MessageBox.Show("退出将失去监控，注意下架或者改价恢复，你确定要退出吗？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    ExitConfirmed = true;
                    Application.Exit();
                    if (RestartApplication)
                        Process.Start(Application.ExecutablePath);
                }
                else
                {
                    e.Cancel = true;
                }
            }
            else
            {
                if (!cts.IsCancellationRequested)
                    cts.Cancel();
                SyncSupplierQueue.Dispose();
                try
                {
                    if (Connection != null)
                    {
                        if (Connection.State == ConnectionState.Connected)
                        {
                            AppendText("正在停止监控...");
                            await client.TmcUserCancel();
                        }
                        Connection.Error -= Connection_Error;
                        Connection.Closed -= Connection_Closed;
                        Connection.Reconnecting -= Connection_Reconnecting;
                        Connection.Reconnected -= Connection_Reconnected;
                        Connection.StateChanged -= Connection_StateChanged;
                        if (Connection.State == ConnectionState.Connected)
                            Connection.Stop(TimeSpan.FromSeconds(3));
                        Connection.Dispose();
                        Connection = null;
                    }
                }
                catch (Exception ex)
                {
                    AppendException(ex);
                }
            }
            RestartApplication = false;
        }

        private bool GetServerTime()
        {
            Stopwatch sw = new Stopwatch();
            sw.Restart();
            try
            {
                var headers = HttpHelper.ReadResponseHeaders("http://chongzhi.taobao.com/welcome.dox?method=welcome");
                sw.Stop();

                //淘宝时间
                var taoServerTime = DateTime.Parse(headers["Date"]);
                ServerDelay = sw.Elapsed;
                DateDifference = DateTime.Now - taoServerTime;
                this.InvokeAction(() =>
                {
                    延迟.Text = "延迟：" + sw.ElapsedMilliseconds + "ms";
                    服务器时间.Text = "时差：" + DateDifference.ToString(@"hh\:mm\:ss\.fff");
                });
                return true;
            }
            catch (Exception ex)
            {
                this.InvokeAction(() =>
                {
                    延迟.Text = "延迟：...";
                    服务器时间.Text = "时差：...";
                });
            }
            return false;
        }
        public void ThreadLoopMonitorDelay()
        {
            Task.Factory.StartNew(async () =>
            {

                while (!cts.IsCancellationRequested)
                {
                    GetServerTime();
                    await TaskEx.Delay(5000);
                }
            });
        }

        public void AppendException(Exception ex)
        {
            AppendText("");
            AppendText(ex.Message);
            AppendText(ex.StackTrace);
            AppendText("");
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
                AppendException(ex);
            }
        }

        public void ShowException(Exception ex, string caption)
        {
            MessageBox.Show(ex.Message + Environment.NewLine + "StackTrace:" + Environment.NewLine + ex.StackTrace, caption);
        }

        public bool NotifyIfUnconnected()
        {
            if (this.Connection.State != ConnectionState.Connected)
            {
                MessageBox.Show("消息服务未连接，操作无法继续。");
                return true;
            }
            return false;
        }


        private void EmptyEventHandler(object sender, EventArgs e)
        {

        }
        private void SelectAllToolStripMenuItemClick(object sender, EventArgs e)
        {
            var toolStripItem = sender as ToolStripMenuItem;
            var dataGridView = toolStripItem.Tag as DataGridView;
            foreach (DataGridViewRow item in dataGridView.Rows)
            {
                int i = item.Index;
                if ((bool)dataGridView.Rows[i].Cells[0].Value != !toolStripItem.Checked)
                    dataGridView.Rows[i].Cells[0].Value = !toolStripItem.Checked;
            }
            dataGridView.EndEdit();
            toolStripItem.Checked = !toolStripItem.Checked;
        }

        private void BindDGViewProduct()
        {
            var productList = AppDatabase.db.ProductItems.FindAll();
            var dt = productList.AsDataTable();
            this.InvokeAction(() =>
            {
                var selected = GetSelectedProductList();
                dataGridViewItem.DataSource = null;
                dataGridViewItem.DataSource = dt;
                dt = null;
                dataGridViewItem.ClearSelection();
                SelectRows(selected);
            });
        }

        private void BindDGViewTBOrder()
        {
            var orders = AppDatabase.db.TopTrades.FindAll().OrderBy(x => x.Created);
            var dt = orders.AsDataTable();
            dataGridViewTbOrder.DataSource = dt;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            new BlackListForm("黑名单管理").ShowDialog(this);
        }

        private void buttonWlistMgr_Click(object sender, EventArgs e)
        {
            new WhiteListForm("白名单管理").ShowDialog(this);
        }

        private async void UpProduct(object sender, EventArgs e)
        {
            var productIds = GetSelectedProductList();
            var productList = AppDatabase.db.ProductItems.Find(x => x.Where == "仓库中").Where(x => productIds.Contains(x.Id));
            await UpProduct(productList.ToArray());
        }

        public async Task<bool> SetProductProfit(ProductItem product, Func<ProductItem, decimal> Getprofit, bool forceModify = false, bool isUserOperation = false)
        {
            decimal profit = Getprofit(product);
            SuplierInfo supplier = null;
            if (!string.IsNullOrEmpty(product.SupplierId))
            {
                supplier = product.GetSuplierInfo();
            }
            else
            {
                using (var wbHelper = new WBHelper(true))
                {
                    var x = await this.InvokeTask(wbHelper.IsLoginRequired);
                    while (x)
                    {
                        this.InvokeAction(ShowLoginWindow, wbHelper.WB);
                        x = await this.InvokeTask(wbHelper.IsLoginRequired);
                    }
                    supplier = await this.InvokeTask(supplierInfo, wbHelper, product.SpuId);
                    if (supplier == null || !supplier.profitData.Any())
                    {
                        AppendText("【{0}】暂无供应商，改价操作取消。", product.ItemName);
                        return false;
                    }
                    product.OnSupplierInfoUpdate(AppDatabase.db.ProductItems, supplier);
                }
            }
            if (supplier != null)
            {
                if (supplier.profitData[0].price == 0)
                {
                    AppendText("【{0}】暂无供应商，改价操作取消。", product.ItemName);
                    return false;
                }
                if (supplier.profitData[0].price == product.进价 && product.利润 == profit && !forceModify)
                {
                    //价格无变化不处理
                    AppendText("商品【{0}】进价及利润无变化，跳过。", product.ItemName);
                    BindDGViewProduct();
                    return true;
                }
                string profitString = "0.00";
                profitString = profit.ToString("f2");
                var oneprice = (supplier.profitData[0].price + profit).ToString("f2");
                using (var wbHelper = new WBHelper(false))
                {
                    var x = await this.InvokeTask(wbHelper.IsLoginRequired);
                    while (x)
                    {
                        this.InvokeAction(ShowLoginWindow, wbHelper.WB);
                        x = await this.InvokeTask(wbHelper.IsLoginRequired);
                    }
                    var save = await this.InvokeTask(supplierSave, wbHelper, supplier.profitData[0].id, product.SpuId, profitString, oneprice, product.Id, tbcpCrumbs);
                    if (save.status != 200)
                    {
                        AppendText("为商品{0}设置利润时失败。错误消息：{1}", product.Id, save.msg);
                        return false;
                    }
                    else
                    {
                        product.ModifyProfitSubmitted = true;
                        if (isUserOperation)
                        {
                            //备份原始利润
                            product.原利润 = profit;
                        }
                        AppDatabase.db.ProductItems.Update(product);
                        BindDGViewProduct();
                        return true;
                    }
                }
            }
            else
            {
                AppendText("为商品{0}设置利润时失败，因为没有查询到供应商信息。", product.Id);
                return false;
            }
        }

        public async Task SetProfitDirect(decimal profit)
        {
            var selectedList = this.GetSelectedProductList();
            if (!selectedList.Any())
            {
                MessageBox.Show("请选中数据再操作。");
                return;
            }
            foreach (var id in selectedList)
            {
                var product = AppDatabase.db.ProductItems.FindById(id);
                await SetProductProfit(product, (x) => profit, forceModify: false, isUserOperation: true);
            }
        }

        public async Task SetProfitBySubName(List<ProductItem> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                await SetProductProfit(list[i], (product) =>
                {
                    decimal profit = 0;
                    if (product.ItemSubName == "QQ直充" && AppSetting.UserSetting.Get<bool>("处理QQ直充利润"))
                    {
                        profit = AppSetting.UserSetting.Get<decimal>("QQ直充利润");
                    }
                    else if (product.ItemSubName == "点卡直充" && AppSetting.UserSetting.Get<bool>("处理点卡直充利润"))
                    {
                        profit = AppSetting.UserSetting.Get<decimal>("点卡直充利润");
                    }
                    else if (product.ItemSubName == "话费直充" && AppSetting.UserSetting.Get<bool>("处理话费直充利润"))
                    {
                        profit = AppSetting.UserSetting.Get<decimal>("话费直充利润");
                    }
                    return profit;
                }, isUserOperation: true);
            }
        }

        private async Task UpProduct(params ProductItem[] productList)
        {
            if (productList == null || !productList.Any())
            {
                AppendText("没有可以上架的商品");
                return;
            }
            foreach (var product in productList)
            {
                var lockType = "";
                var qty = 0;
                if (product.ItemSubName == "QQ直充" && AppSetting.UserSetting.Get<bool>("对QQ直充启用库存锁定"))
                {
                    lockType = AppSetting.UserSetting.Get<string>("QQ直充库存锁定类型");
                    qty = (int)AppSetting.UserSetting.Get<decimal>("QQ直充库存锁定数量");
                }
                else if (product.ItemSubName == "话费直充" && AppSetting.UserSetting.Get<bool>("对话费直充启用库存锁定"))
                {
                    lockType = AppSetting.UserSetting.Get<string>("话费直充库存锁定类型");
                    qty = (int)AppSetting.UserSetting.Get<decimal>("话费直充库存锁定数量");
                }
                else if (product.ItemSubName == "点卡直充" && AppSetting.UserSetting.Get<bool>("对点卡直充启用库存锁定"))
                {
                    lockType = AppSetting.UserSetting.Get<string>("点卡直充库存锁定类型");
                    qty = (int)AppSetting.UserSetting.Get<decimal>("点卡直充库存锁定数量");
                }
                if (lockType == "锁定所有商品" || (lockType == "锁定监控商品" && product.Monitor))
                {
                    if (qty == 0)
                        qty = 1;
                    var apix = await client.ItemHub.ItemUpdateList(product.Id, qty);
                    if (apix.Success)
                    {
                        //await client.ItemHub.ItemQuantityUpdate(product.Id, qty);
                        //有服务端消息，这个不显示了
                        //AppendText("[商品{0}]上架完成。", product.ItemName);
                    }
                    else
                    {
                        AppendText("[商品{0}]上架失败，错误消息：{1}", product.ItemName, apix.Message);
                    }
                }
                else
                {
                    AppendText("类型为{0}的商品{1}没有设置为锁定库存，忽略。", product.ItemSubName, product.Id);
                }
            }
        }


        private async void UpAllProduct(object sender, EventArgs e)
        {
            var productList = AppDatabase.db.ProductItems.Find(x => x.Where == "仓库中");
            if (productList == null || !productList.Any())
            {
                AppendText("没有可以上架的商品");
                return;
            }
            await UpProduct(productList.ToArray());
        }

        private async void DownProduct(object sender, EventArgs e)
        {
            var ids = GetSelectedProductList();
            var productList = AppDatabase.db.ProductItems.Find(x => x.Where == "出售中").Where(x => ids.Contains(x.Id));
            if (!productList.Any())
            {
                AppendText("没有可以下架的商品");
                return;
            }
            foreach (var item in productList)
            {
                var r = await client.ItemHub.ItemUpdateDelist(item.Id);
                if (r.Success)
                {
                    item.AutoUpshelf = false;
                    AppDatabase.db.ProductItems.Update(item);
                }
            }
        }

        private async void DownAllProduct(object sender, EventArgs e)
        {
            var productList = AppDatabase.db.ProductItems.Find(x => x.Where == "出售中").ToList();
            if (!productList.Any())
            {
                AppendText("没有可以下架的商品");
                return;
            }
            foreach (var item in productList)
            {
                await client.ItemHub.ItemUpdateDelist(item.Id);
                item.AutoUpshelf = false;
                AppDatabase.db.ProductItems.Update(item);
            }
        }

        public async Task ModifyStock(int stockQty, IEnumerable<ProductItem> productList)
        {
            if (!NotifyIfUnconnected())
            {
                foreach (var product in productList)
                {
                    if (NotifyIfUnconnected())
                    {
                        break;
                    }
                    var itemId = product.Id;
                    var ret = await this.client.ItemHub.ItemQuantityUpdate(itemId, stockQty);
                    if (ret.Success)
                    {
                        AppendText("商品{0}库存设置成功,值{1}", itemId, stockQty);
                    }
                    else
                    {
                        AppendText("商品{0}库存设置失败，后续处理已终止。错误消息：{1}", itemId, ret.Message);
                        return;
                    }
                }
                if (productList.Any())
                {
                    AppendText("批量修改库存完成");
                }
            }
        }

        private async void ModifyStockQty(object sender, EventArgs e)
        {
            var productList = AppDatabase.db.ProductItems.FindAll();
            var tag = (sender as ToolStripMenuItem).Tag;
            if (tag == null)
            {
                new ModifyStockQtyForm(productList) { Text = "修改商品库存 【全部修改】" }.ShowDialog(this);
            }
            else
            {
                var stockQty = int.Parse((sender as ToolStripMenuItem).Tag.ToString());
                await ModifyStock(stockQty, productList);
            }
        }

        private void buttonGo_Click(object sender, EventArgs e)
        {
            //if (string.IsNullOrEmpty(this.textBoxUrl.Text))
            //    this.textBoxUrl.Text = wb.Url.AbsoluteUri;
            //this.MainExWB.Navigate(this.textBoxUrl.Text);
        }

        private void button重启软件_Click(object sender, EventArgs e)
        {
            Process.Start(Application.ExecutablePath);
            this.Close();
        }

        private void 修改库存ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var selectedList = GetSelectedProductList();
            var productList = AppDatabase.db.ProductItems.FindAll().Where(x => selectedList.Contains(x.Id));
            new ModifyStockQtyForm(productList) { Text = "修改商品库存 【仅修改表格中选中的】" }.ShowDialog(this);
        }

        private void 设为正利润ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var Ids = GetSelectedProductList();
            if (Ids.Any())
            {
                new SetProfitForm(Ids).ShowDialog(this);
            }
        }

        private List<long> GetSelectedProductList()
        {
            var rows = dataGridViewItem.SelectedRows.AsList<DataGridViewRow>();
            var Ids = new List<long>();
            if (rows.Any())
            {
                foreach (var item in rows)
                {
                    var productId = (long)((System.Data.DataRowView)item.DataBoundItem).Row.ItemArray[0];
                    Ids.Add(productId);
                }
            }
            return Ids;
        }
        bool ExitConfirmed;
        private bool RestartApplication;
        private DateTime? ServerNow;

        private void 切换账号ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //ExitConfirmed = true;
            RestartApplication = true;
            AppSetting.UserSetting.SetNull("AutoSelectLoginedAccount");
            Application.Exit();

        }

        private void 退出应用程序ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //ExitConfirmed = true;
            Application.Exit();
        }

        private void 仓库中ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridViewItem.ClearSlectionIfControlKeyNotPressDown();
            var list = AppDatabase.db.ProductItems.Find(x => x.Where == "仓库中");
            SelectRows(list.Select(x => x.Id));
        }

        private void SelectRows(IEnumerable<long> productIds)
        {
            var rows = dataGridViewItem.Rows.AsList<DataGridViewRow>();
            if (productIds.Any())
            {
                foreach (var item in rows)
                {
                    var productId = (long)((System.Data.DataRowView)item.DataBoundItem).Row.ItemArray[0];
                    if (productIds.Any(x => x == productId))
                    {
                        item.Selected = true;
                    }
                }
            }
        }

        private void 销售中ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridViewItem.ClearSlectionIfControlKeyNotPressDown();
            var list = AppDatabase.db.ProductItems.Find(x => x.Where == "出售中");
            SelectRows(list.Select(x => x.Id));
        }

        private void 正利润ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridViewItem.ClearSlectionIfControlKeyNotPressDown();
            var list = AppDatabase.db.ProductItems.FindAll().Where(x => x.利润 > 0);
            SelectRows(list.Select(x => x.Id));
        }

        private void 零利润ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridViewItem.ClearSlectionIfControlKeyNotPressDown();
            var list = AppDatabase.db.ProductItems.FindAll().Where(x => x.利润 == 0);
            SelectRows(list.Select(x => x.Id));
        }

        private void 负利润ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridViewItem.ClearSlectionIfControlKeyNotPressDown();
            var list = AppDatabase.db.ProductItems.FindAll().Where(x => x.利润 < 0);
            SelectRows(list.Select(x => x.Id));
        }

        private void 全选ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridViewItem.SelectAll();
        }

        private void 反选ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridViewItem.InvertSelection();
        }

        private void qQ直充ToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            dataGridViewItem.ClearSlectionIfControlKeyNotPressDown();
            var list = AppDatabase.db.ProductItems.Find(x => x.ItemSubName == "QQ直充");
            SelectRows(list.Select(x => x.Id));
        }

        private void 话费直充ToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            dataGridViewItem.ClearSlectionIfControlKeyNotPressDown();
            var list = AppDatabase.db.ProductItems.Find(x => x.ItemSubName == "话费直充");
            SelectRows(list.Select(x => x.Id));
        }

        private void 点卡直充ToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            dataGridViewItem.ClearSlectionIfControlKeyNotPressDown();
            var list = AppDatabase.db.ProductItems.Find(x => x.ItemSubName == "点卡直充");
            SelectRows(list.Select(x => x.Id));
        }

        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("生命在于折腾。联系旺旺：上红包啊  QQ：627658514", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void 设为零利润ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await this.SetProfitDirect(0);
        }

        private void 设为负利润ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var Ids = GetSelectedProductList();
            if (Ids.Any())
            {
                new SetProfitForm(Ids).ShowDialog(this);
            }
        }

        public void LoadMenuItemSetting()
        {
            上架ToolStripMenuItem.Showing += 上架ToolStripMenuItem_ShownOnContext;
            下架ToolStripMenuItem.Showing += 下架ToolStripMenuItem_ShownOnContext;
            修改库存ToolStripMenuItem2.ControlOptions.WhenNone.Avaiable = false;
            锁定库存ToolStripMenuItem.Showing += 锁定库存ToolStripMenuItem_ShownOnContext;
            取消锁定库存ToolStripMenuItem.Showing += 取消锁定库存ToolStripMenuItem_ShownOnContext;
            开启自动上架ToolStripMenuItem.Showing += 开启自动上架ToolStripMenuItem_ShownOnContext;
            关闭自动上架ToolStripMenuItem.Showing += 关闭自动上架ToolStripMenuItem_ShownOnContext;
            获取供应商信息ToolStripMenuItem.ControlOptions.WhenNone.Enabled = false;
            反选ToolStripMenuItem1.ControlOptions.WhenNone.Enabled = false;
            进入商品页ToolStripMenuItem.ControlOptions.WhenSingle.Enabled = true;
        }

        private void 关闭自动上架ToolStripMenuItem_ShownOnContext(object sender, Moonlight.WindowsForms.StateControls.StripMenuItemShownOnContextEventArgs e)
        {
            var rows = e.TargetObjects.AsList<DataGridViewRow>();
            foreach (var row in rows)
            {
                //要关闭自动上架，至少有一个是自动上架
                var productId = (long)((System.Data.DataRowView)row.DataBoundItem).Row.ItemArray[0];
                var product = AppDatabase.db.ProductItems.FindById(productId);
                if (product.AutoUpshelf)
                {
                    e.Enabled = true;
                    return;
                }
            }
            e.Enabled = false;
        }

        private void 开启自动上架ToolStripMenuItem_ShownOnContext(object sender, Moonlight.WindowsForms.StateControls.StripMenuItemShownOnContextEventArgs e)
        {
            var rows = e.TargetObjects.AsList<DataGridViewRow>();
            foreach (var row in rows)
            {
                //要开启自动上架，至少有一个不是自动上架
                var productId = (long)((System.Data.DataRowView)row.DataBoundItem).Row.ItemArray[0];
                var product = AppDatabase.db.ProductItems.FindById(productId);
                if (!product.AutoUpshelf)
                {
                    e.Enabled = true;
                    return;
                }
            }
            e.Enabled = false;
        }

        private void 取消锁定库存ToolStripMenuItem_ShownOnContext(object sender, Moonlight.WindowsForms.StateControls.StripMenuItemShownOnContextEventArgs e)
        {
            var rows = e.TargetObjects.AsList<DataGridViewRow>();
            foreach (var row in rows)
            {
                //要取消锁定库存，至少有一个是锁定的
                var productId = (long)((System.Data.DataRowView)row.DataBoundItem).Row.ItemArray[0];
                var product = AppDatabase.db.ProductItems.FindById(productId);
                if (product.Monitor)
                {
                    e.Enabled = true;
                    return;
                }
            }
            e.Enabled = false;
        }

        private void 锁定库存ToolStripMenuItem_ShownOnContext(object sender, Moonlight.WindowsForms.StateControls.StripMenuItemShownOnContextEventArgs e)
        {
            var rows = e.TargetObjects.AsList<DataGridViewRow>();
            foreach (var row in rows)
            {
                //要锁定库存，至少有一个没锁定
                var productId = (long)((System.Data.DataRowView)row.DataBoundItem).Row.ItemArray[0];
                var product = AppDatabase.db.ProductItems.FindById(productId);
                if (!product.Monitor)
                {
                    e.Enabled = true;
                    return;
                }
            }
            e.Enabled = false;
        }

        private void 下架ToolStripMenuItem_ShownOnContext(object sender, Moonlight.WindowsForms.StateControls.StripMenuItemShownOnContextEventArgs e)
        {
            var rows = e.TargetObjects.AsList<DataGridViewRow>();
            foreach (var row in rows)
            {
                //要启用下架菜单项，至少有一个在出售中
                var productId = (long)((System.Data.DataRowView)row.DataBoundItem).Row.ItemArray[0];
                var product = AppDatabase.db.ProductItems.FindById(productId);
                if (product.Where == "出售中")
                {
                    e.Enabled = true;
                    return;
                }
            }
            e.Enabled = false;
        }

        private void 上架ToolStripMenuItem_ShownOnContext(object sender, Moonlight.WindowsForms.StateControls.StripMenuItemShownOnContextEventArgs e)
        {
            var rows = e.TargetObjects.AsList<DataGridViewRow>();
            foreach (var row in rows)
            {
                //要启用上架 至少有一个在仓库中
                var productId = (long)((System.Data.DataRowView)row.DataBoundItem).Row.ItemArray[0];
                var product = AppDatabase.db.ProductItems.FindById(productId);
                if (product != null && product.Where == "仓库中")
                {
                    e.Enabled = true;
                    return;
                }
            }
            e.Enabled = false;
        }

        private void 锁定库存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var Ids = GetSelectedProductList();
            foreach (var item in AppDatabase.db.ProductItems.FindAll().Where(x => Ids.Contains(x.Id)))
            {
                item.Monitor = true;
                AppDatabase.db.ProductItems.Update(item);
            }
            BindDGViewProduct();
        }

        private void 取消锁定库存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var Ids = GetSelectedProductList();
            foreach (var item in AppDatabase.db.ProductItems.FindAll().Where(x => Ids.Contains(x.Id)))
            {
                item.Monitor = false;
                AppDatabase.db.ProductItems.Update(item);
            }
            BindDGViewProduct();
        }

        private async void 同步交易数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            同步交易数据ToolStripMenuItem.Enabled = false;
            await SyncTradeListIncrease(false);
            同步交易数据ToolStripMenuItem.Enabled = true;
        }

        private void 账号设置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AppSettingForm().ShowDialog(this);
        }

        private async void 刷新商品列表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await SyncAllProductList();
        }

        private void 开启自动上架ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ids = GetSelectedProductList();
            foreach (var item in ids)
            {
                var product = AppDatabase.db.ProductItems.FindById(item);
                product.AutoUpshelf = true;
                AppDatabase.db.ProductItems.Update(product);
            }
            BindDGViewProduct();
        }

        private void 关闭自动上架ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ids = GetSelectedProductList();
            foreach (var item in ids)
            {
                var product = AppDatabase.db.ProductItems.FindById(item);
                product.AutoUpshelf = false;
                AppDatabase.db.ProductItems.Update(product);
            }
            BindDGViewProduct();
        }

        private void 修改利润ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var Ids = GetSelectedProductList();
            if (Ids.Any())
            {
                new SetProfitForm(Ids).ShowDialog(this);
            }
        }

        private async void 一键赔钱_Click(object sender, EventArgs e)
        {
            一键赔钱.Enabled = false;
            try
            {
                await SetProfitDirect(AppSetting.UserSetting.Get<decimal>("赔钱利润")).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                AppendException(ex);
            }
            this.InvokeAction(async () => { await TaskEx.Delay(3); 一键赔钱.Enabled = true; 一键不赔钱.Enabled = true; });
        }

        private async void 一键不赔钱_Click(object sender, EventArgs e)
        {
            一键不赔钱.Enabled = false;
            try
            {
                await SetProfitDirect(AppSetting.UserSetting.Get<decimal>("不赔钱利润")).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                AppendException(ex);
            }
            this.InvokeAction(async () => { await TaskEx.Delay(3); 一键赔钱.Enabled = true; 一键不赔钱.Enabled = true; });
        }

        private void 获取供应商信息ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ids = GetSelectedProductList();
            var products = AppDatabase.db.ProductItems.FindAll().Where(x => ids.Contains(x.Id));
            foreach (var item in products)
            {
                if (!item.SyncProfitSubmited)
                {
                    item.SyncProfitSubmited = true;
                    AppDatabase.db.ProductItems.Update(item);
                    SyncSupplierQueue.CreateTaskItem(item, SyncSupplierInfo, x =>
                    {
                        var product = AppDatabase.db.ProductItems.FindById(x.ProductId);
                        if (x.Success && !product.NeedResotreProfit)
                        {
                            //备份原始利润
                            product.原利润 = product.利润;
                        }
                        if (product.SyncProfitSubmited)
                        {
                            product.SyncProfitSubmited = false;
                        }
                        AppDatabase.db.ProductItems.Update(product);
                        BindDGViewProduct();
                    });
                }
            }
            BindDGViewProduct();
        }
        public class SimpleResult
        {
            public bool Success { get; set; }
            public long ProductId { get; set; }
        }

        private void button自动上架_Click(object sender, EventArgs e)
        {
            var products = AppDatabase.db.ProductItems.FindAll();
            foreach (var product in products)
            {
                product.AutoUpshelf = true;
                AppDatabase.db.ProductItems.Update(product);
            }
            BindDGViewProduct();
        }

        private void button开启监控_Click(object sender, EventArgs e)
        {
            var products = AppDatabase.db.ProductItems.FindAll();
            foreach (var product in products)
            {
                product.Monitor = true;
                AppDatabase.db.ProductItems.Update(product);
            }
            BindDGViewProduct();
        }

        private void 刷新ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BindDGViewProduct();
        }

        private void 进入商品页ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var id = GetSelectedProductList().First();
            Process.Start("https://item.taobao.com/item.htm?id=" + id);
        }
    }

    public class CloseReason
    {
        public static string 未及时付款 = "未及时付款";
        public static string 买家联系不上 = "买家联系不上";
        public static string 谢绝还价 = "谢绝还价";
        public static string 商品瑕疵 = "商品瑕疵";
        public static string 协商不一致 = "协商不一致";
        public static string 买家不想买 = "买家不想买";
        public static string 与买家协商一致 = "与买家协商一致";
        //public static string 未及时付款 = "";
        //public static string 买家不想买 = "";
        //public static string 买家信息填写有误_重新拍 = "买家信息填写有误，重新拍";
        //public static string 恶意买家_同行捣乱 = "恶意买家/同行捣乱";
        //public static string 缺货 = "";
        //public static string 卖家拍错了 = "";
        //public static string 同城见面交易 = "";
        //public static string 其他原因 = "";
    }

    public class InterceptInfo
    {
        public string spuId;
        public Statistic statistic;
        public SuplierInfo supplier;
        public TopTrade trade;

        public InterceptInfo(SuplierInfo supplier, string spuId, TopTrade trade, Statistic statistic)
        {
            this.supplier = supplier;
            this.spuId = spuId;
            this.trade = trade;
            this.statistic = statistic;
        }
    }
}

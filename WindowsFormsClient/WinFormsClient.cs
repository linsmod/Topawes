using Microsoft.AspNet.SignalR.Client;
using Microsoft.Phone.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFormsClient.HtmlEntity;
using WinFormsClient.WBMode;
using WinFormsClient.Extensions;
using System.Threading;
using WinFormsClient.Models;
using System.Text.RegularExpressions;
using LiteDB;
using WinFormsClient.Helpers;
using Codeplex.Data;
using TopModel.Models;
using System.Diagnostics;
using System.Drawing;
using Moonlight;
using System.Collections;
using TopModel;
using Moonlight.Treading;
using Moonlight.Helpers;

namespace WinFormsClient
{

    public partial class WinFormsClient : BaseForm
    {
        public TimeSpan DateDifference = TimeSpan.Zero;
        public static CancellationTokenSource cts = new CancellationTokenSource();
        public bool IsAutoRateRunning = false;
        public string tbcpCrumbs = "";
        static DataStorage appOnlineStorage;
        private MessageHubClient client { get; set; }
        //public const string Server = "http://localhost:62585/";
        //public const string Server = "http://localhost:8090/";
        public const string Server = "http://123.56.122.122:8080/";
        private HubConnection Connection { get; set; }
        public bool IsLogin { get; private set; }

        TaoLoginForm taoLoginForm;
        WBTaoLoginMode wbLoginMode = new WBTaoLoginMode("http://123.56.122.122:8080/");
        WBTaoDurexValidationMode wbTaoDurexValidateMode = new WBTaoDurexValidationMode();
        WBTaoChongZhiBrowserMode wbTaoChongZhiMode = new WBTaoChongZhiBrowserMode();
        BlackListForm blist = new BlackListForm("黑名单管理", "blist.data");
        WhiteListForm wlist = new WhiteListForm("白名单管理", "wlist.data");
        ExtendedWinFormsWebBrowser wb;
        public string title = "淘充值防牛工具";

        internal WinFormsClient()
        {
            InitializeComponent();

            wb = new ExtendedWinFormsWebBrowser(wbLoginMode);
            //wb.ProgressChanged2 += (s, e) =>
            //{
            //    this.progressBar1.Invoke((Action)(() =>
            //    {
            //        progressBar1.Value = e.ProgressPercentage;
            //    }));
            //};
            this.Load += WinFormsClient_Load;
            dataGridViewTbOrder.AutoSizeColumnsMode = dataGridViewItem.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewTbOrder.SelectionMode = dataGridViewItem.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewItem.ContextMenuStrip = contextMenuStrip1;
            this.Text = title;
        }

        [DebuggerStepThrough]
        [DebuggerHidden]
        protected override void WndProc(ref Message msg)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_CLOSE = 0xF060;
            const int SC_MINIMIZE = 61472;
            if (msg.Msg == WM_SYSCOMMAND && ((int)msg.WParam == SC_MINIMIZE))
            {
                this.ShowInTaskbar = false;
                this.Hide();
            }
            base.WndProc(ref msg);
        }
        private void ResetTaskProgress()
        {
            this.SmartInvokeAsync(() =>
            {
                this.progressBar1.Value = 0;
                this.labelTaskName.Text = "就绪";
            });
        }
        private void ReportTaskProgress(string name, int index, int total)
        {
            this.SmartInvokeAsync(() =>
            {
                this.labelTaskName.Text = name;
                this.progressBar1.Value = index + 1;
                this.progressBar1.Maximum = total;
            });
        }

        private void WinFormsClient_Load(object sender, EventArgs e)
        {

            LoadMenuItemSetting();
            taoLoginForm = new TaoLoginForm(wbLoginMode);
            wbLoginMode.AskShowUI += () => { taoLoginForm.Show(this); };
            wbLoginMode.RequireLogin += () => { /*AppSetting.UserSetting.Set("Autorized", false);*/ };
            wbLoginMode.LoginSuccess += () => { AppendText("服务登录完成！"); this.SmartInvoke(() => { this.Visible = true; tabPageWB.Controls.Add(wb); });/*使用cookie连接*/ ConnectAsync(); };
            wbLoginMode.UserCancelLogin += () => { taoLoginForm.DialogResult = DialogResult.Cancel; this.Close(); };

            wbTaoChongZhiMode.AskLogin += () => { wb.TransactionToNext(wbLoginMode); taoLoginForm.Show(this); };

            //开始登录
            this.BeginInvoke((Action)(() => { taoLoginForm.Show(this); }));
        }

        delegate Task InterceptTradeDelegate(SuplierInfo supplier, string spu, TopTrade trade, Statistic statistic);

        private async Task InterceptTrade(SuplierInfo supplier, string spu, TopTrade trade, Statistic statistic)
        {
            if (this.InvokeRequired)
            {
                await ((Task)Invoke(new InterceptTradeDelegate(InterceptTrade), supplier, spu, trade, statistic)).ConfigureAwait(false);
                return;
            }
            AppendText("将拦截订单{0}，买家：{1}", trade.Tid, trade.BuyerNick);

            var product = AppDatabase.db.ProductItems.FindById(trade.NumIid);
            //备份用来恢复价格
            var 原始卖价 = product.一口价;
            var 原始利润 = product.利润;

            //更新价格
            var taoResult = await BizHelper.supplierSave(wb, supplier.profitData[0].id, spu, "0.00", "0.00", supplier.profitData[0].price.ToString("f2"), trade.NumIid, tbcpCrumbs);
            //var taoResult = await ((Task<TaoJsonpResult>)this.Invoke(new SupplierSaveDelegate(BizHelper.supplierSave), wb, supplier.profitData[0].id, spu, "0.00", "0.00", supplier.profitData[0].price.ToString("f2"), trade.NumIid, tbcpCrumbs)).ConfigureAwait(false);
            if (taoResult.status != 200)
            {
                statistic.InterceptFailed++;
                AppDatabase.db.Statistics.Upsert(statistic, statistic.Id);
                OnStatisticUpdate(statistic);
                AppendText("商品{0}改价失败，错误消息：{1}", trade.NumIid, taoResult.msg);
            }
            else
            {
                AppendText("商品{0}改价成功", trade.NumIid);

                //更新改价结果
                product.OnProfitInfoUpdate(AppDatabase.db.ProductItems, product.进价, 0);

                //1分钟后关单
                await Task.Delay(1000 * 60 - 500);

                AppendText("{0}关闭交易...", trade.Tid);
                await CloseTradeIfPossible(trade.Tid);

                //恢复价格
                //更新改价结果
                product = AppDatabase.db.ProductItems.FindById(trade.NumIid);
                if (product.利润 != 0)
                {
                    //尝试还原价格时原始价格已经更新的话就不用恢复了
                    return;
                }
                taoResult = await BizHelper.supplierSave(wb, supplier.profitData[0].id, spu, 原始利润.ToString("f2"), 原始利润.ToString("f2"), 原始卖价.ToString("f2"), trade.NumIid, tbcpCrumbs);
                //taoResult = await ((Task<TaoJsonpResult>)this.Invoke(new SupplierSaveDelegate(BizHelper.supplierSave), wb, supplier.profitData[0].id, spu, 原始利润.ToString("f2"), 原始利润.ToString("f2"), 原始卖价.ToString("f2"), product.Id, tbcpCrumbs)).ConfigureAwait(false);
                if (taoResult.status != 200)
                {
                    AppendText("商品{0}恢复价格失败，请注意！错误消息：{1}", trade.NumIid, taoResult.msg);
                    return;
                }

                //更新恢复价格的结果
                product.OnProfitInfoUpdate(AppDatabase.db.ProductItems, product.进价, 原始利润);
            }
        }

        private async Task CloseTradeIfPossible(long tid)
        {
            var topTrade = AppDatabase.db.TopTrades.FindById(tid);
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
        }
        delegate Task<SuplierInfo> supplierInfoDelegate(ExtendedWinFormsWebBrowser wb, string spuId);
        private async void ButtonSend_Click(object sender, EventArgs e)
        {
            if (TextBoxMessage.Text == "")
            {
                AppendText("请输入内容！");
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
            await Task.Delay(200);
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
            //client.onTopManagerState += (x) => { this.AppendText(string.Format("Login As [{0}] - TopManager.Initialized={1}", AppSetttings.TaoUserName, x ? " No" : " Yes")); };
            client.ProxyInvokeException += Client_ProxyInvokeException;

            try
            {
                
                await Connection.Start();
                this.tssl_ConnState.Text = "连接状态：" + ConnectionState.Connected.AsZhConnectionState();
                IsLogin = true;
                this.SmartInvoke(() =>
                {
                    //登录完成后导航到这个页面，方便后面AJAX直接使用这个浏览器取数据
                    wb.Navigate("http://chongzhi.taobao.com/index.do?spm=0.0.0.0.OR0khk&method=index");
                });
                var userInfo = await client.UserInfo();
                var userName = (string)userInfo.UserName;
                AppDatabase.Initialize(userName);
                LoadUserSetting();
                this.AppendText(string.Format("账号 [{0}] - 服务消息代理状态={1}", userName, ((bool)userInfo.TopManagerInitialized) ? " 运行中" : " 未启动，请联系技术支持。"));
                SetupTaskbarIcon();
                AppDatabase.db.Statistics.Delete(userName);
                AppDatabase.db.Statistics.Insert(new Statistic { Id = userName });
                var statistic = AppDatabase.db.Statistics.FindById(userName);
                OnStatisticUpdate(statistic);

                this.SmartInvoke(() =>
                {
                    Text = title + " 授权给：" + userName +
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

                var taoInfo = await GetTaoInfo().ConfigureAwait(false);
                if (taoInfo.status == 200)
                {

                }
                await SyncTradeListIncrease(true).ConfigureAwait(false);
                var success = await SyncAllProductList().ConfigureAwait(false);
                if (success)
                {
                    await SyncSupplierInfo(AppDatabase.db.ProductItems.FindAll().ToArray());
                    ThreadLoopSyncTrade();
                    ThreadLoopMonitorDelay();
                    ThreadLoopCloseTrade();
                    var permit = await client.TmcGroupAddThenTmcUserPermit();
                    var permitSuccess = permit.Success;
                    this.AppendText("消息授权{0}", permitSuccess ? "成功" : "失败，错误消息：" + permit.Message);
                }
                else
                {
                    AppendText("首次同步商品未完成，请联系技术支持。");
                }
            }
            catch (Exception ex)
            {
                AppendException(ex);
            }
        }

        private void SetupTaskbarIcon()
        {
            if (this.notifyIcon1.Icon == null)
            {
                this.notifyIcon1.Icon = this.Icon;
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

        private async Task SyncSupplierInfo(params ProductItem[] args)
        {
            AppendText("同步供应商信息...");
            foreach (var product in args)
            {
                //查找供应商
                var supplier = await ((Task<SuplierInfo>)this.Invoke(new supplierInfoDelegate(BizHelper.supplierInfo), wb, product.SpuId)).ConfigureAwait(false);
                if (supplier != null)
                {
                    product.OnSupplierInfoUpdate(AppDatabase.db.ProductItems, supplier);
                }
                else
                {
                    AppendText("商品【{0}】供应商查询失败，请重试！", product.ItemName);
                }
            }
            BindDGViewProduct();
            AppendText("同步供应商信息完成！");
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
                //查找供应商
                supplier = await ((Task<SuplierInfo>)this.Invoke(new supplierInfoDelegate(BizHelper.supplierInfo), wb, product.SpuId)).ConfigureAwait(false);
                product.OnSupplierInfoUpdate(AppDatabase.db.ProductItems, supplier);
            }
            if (supplier != null)
            {
                //5分钟之前的消息即过期的下单信息不处理
                if ((DateTime.Now - trade.Created).TotalMinutes > 5)
                {
                    AppendText("[{0}/{1}]不拦截过期交易。", trade.Tid, trade.NumIid);
                    return;
                }

                var interceptType = AppSetting.UserSetting.Get<string>("拦截模式");
                if (interceptType == InterceptMode.无条件拦截模式)
                {
                    await InterceptTrade(supplier, product.SpuId, trade, statistic);
                    return;
                }
                else if (interceptType == InterceptMode.仅拦截亏本交易)
                {
                    if (supplier.profitMin < 0)
                    {
                        await InterceptTrade(supplier, product.SpuId, trade, statistic);
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
                            await InterceptTrade(supplier, product.SpuId, trade, statistic);
                            return;
                        }
                    }

                    //14天内同宝贝付款订单大于1
                    var orderCount = AppDatabase.db.TopTrades.Count(x => x.BuyerNick == trade.BuyerNick && x.NumIid == trade.NumIid);
                    if (orderCount > 1)
                    {
                        //AppendText("14天内同宝贝付款订单大于1拦截，订单ID={0}", trade.Tid);
                        await InterceptTrade(supplier, product.SpuId, trade, statistic);
                        return;
                    }

                    //买家是白名单内买家,不拦截
                    if (WhiteListForm.WhiteList.IndexOf(trade.BuyerNick) != -1)
                    {
                        AppendText("[{0}/{1}]不拦截-{2}。", trade.Tid, trade.NumIid, interceptType);
                        return;
                    }
                    //黑名单买家一律拦截
                    if (BlackListForm.BlackList.IndexOf(trade.BuyerNick) != -1)
                    {
                        //AppendText("黑名单买家拦截，订单ID={0}", trade.Tid);
                        await InterceptTrade(supplier, product.SpuId, trade, statistic);
                        return;
                    }

                    //购买数量超过1件的拦截
                    if (trade.Num > 1)
                    {
                        //AppendText("购买数量超过1件拦截，订单ID={0}", trade.Tid);
                        await InterceptTrade(supplier, product.SpuId, trade, statistic);
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
                trade.Status = result.Data;
                AppDatabase.db.TopTrades.Update(trade);
            }
            await CloseTradeIfPossible(d.tid);
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
            await CloseTradeIfPossible(trade.Tid);
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

            //AppendText(msg.Topic);
            //AppendText(msg.Content);
            //taobao_item_ItemUpdate
            //{ "nick":"cendart","changed_fields":"","num_iid":522571681053} 
            //被删除时会先触发taobao_item_ItemUpdate消息，如果商品在出售中，还会触发下架消息
            var d = DynamicJson.Parse(msg.Content);
            var product = await FindProductById((long)d.num_iid).ConfigureAwait(false);
            if (product != null)
            {
                //这里price是一口价
                //{"price":"9.51","nick":"红指甲与高跟鞋","changed_fields":"price","num_iid":521911440067}
                if (d.price())
                {
                    decimal 新的一口价 = decimal.Parse(d.price);
                    decimal 新的利润 = 新的一口价 - product.进价;
                    AppendText("商品【{0}】一口价更新{1}=>{2}，利润：{3}", product.ItemName, product.一口价, 新的一口价, 新的利润);

                    product.OnProfitInfoUpdate(AppDatabase.db.ProductItems, product.进价, 新的利润);
                }
            }
            BindDGViewProduct();
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

        private async Task SetItemQty(ProductItem product)
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
            else
            {
                AppendText("类型为{0}的商品{1}没有设置为锁定库存，忽略。", product.ItemSubName, product.Id);
            }
            if (lockType == "锁定所有商品" || (lockType == "锁定监控商品" && product.Monitor))
            {
                if (qty != 0 && qty != product.StockQty)
                    await client.ItemHub.ItemQuantityUpdate(product.Id, qty);
            }
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

        private async void ItemHub_ItemAdd(Top.Tmc.Message msg)
        {
            //AppendText(msg.Topic);
            //AppendText(msg.Content);
            //var def = new { num = 999999, title = "QQ币1个直充", price = "0.97", nick = "cendart", num_iid = 523042737153 };
            var d = DynamicJson.Parse(msg.Content);
            AppendText("新增了商品【{0}】", d.title);
            var product = await FindProductById((long)d.num_iid);
            if (product == null)
            {
                product = new ProductItem { Id = d.num_iid, ItemName = d.title, 一口价 = d.price, StockQty = d.num };
                AppDatabase.db.ProductItems.Upsert((ProductItem)product, (string)d.num_iid);
            }
            BindDGViewProduct();
        }

        private async Task<ProductItem> FindProductById(long itemId)
        {
            var product = AppDatabase.db.ProductItems.FindById(itemId);
            if (product == null)
            {
                await SyncAllProductList().ConfigureAwait(false);
            }
            return product;
        }
        public class InterceptMode
        {
            public static string 智能拦截模式 = "智能拦截模式";
            public static string 仅拦截亏本交易 = "仅拦截亏本交易";
            public static string 无条件拦截模式 = "无条件拦截模式";
        }
        private void LoadUserSetting()
        {
            this.SmartInvoke(() =>
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
            await Task.Delay(200);
            var cnt = await ((Task<string>)this.Invoke(new SynchronousLoadStringDelegate(wb.SynchronousLoadString), "http://chongzhi.taobao.com/index.do?method=info&t=1444581327031")).ConfigureAwait(false);
            var info = JsonConvert.DeserializeObject<TaoInfo>(cnt);
            if (info != null && info.status == 200)
            {
                this.SmartInvoke(() =>
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
                timeStart = DateTime.Now.AddDays(-14);
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
            this.SmartInvokeAsync(() => { BindDGViewTBOrder(); });

            this.AppendText("同步订单完成（{0}个）", list.Count);

            if (!startingApp && AppSetting.UserSetting.Get<bool>(this.自动好评交易checkBoxAuto.Name))
            {
                await AutoTradeRate();
            }
        }

        private async Task AutoTradeRate()
        {
            var finishedList = AppDatabase.db.TopTrades.Find(x => x.Status == "TRADE_FINISHED" && x.SellerCanRate && !x.SellerRate && x.EndTime > DateTime.Now.AddDays(-15)).ToList();
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

        private async Task<bool> SyncAllProductList()
        {
            try
            {
                bool success = false;
                if (this.InvokeRequired)
                {
                    return await ((Task<bool>)this.Invoke(new SyncProductListDelegate(this.SyncAllProductList)));
                }
                else
                {
                    //仓库中
                    success = await SyncInStockProductList();
                    if (success)
                    {
                        //出售中
                        success = await SyncOnSaleProductList();
                    }
                }
                if (success)
                {
                    this.SmartInvokeAsync(() =>
                    {
                        BindDGViewProduct();
                        tabControl1.Enabled = true;
                    });
                }
                AppSetting.UserSetting.Set<DateTime?>("LastSyncProductAt", DateTime.Now);
                return success;
            }
            catch (Exception ex)
            {
                AppendText("同步商品异常，你可以稍后重试。");
                return false;
            }
        }

        private async Task<bool> SyncInStockProductList()
        {
            return await SyncProductList("仓库中", "http://chongzhi.taobao.com/item.do?method=list&type=1&keyword=&category=0&supplier=0&promoted=0&order=0&desc=0&page=1&size=100");
        }
        private async Task<bool> SyncOnSaleProductList()
        {
            return await SyncProductList("出售中", "http://chongzhi.taobao.com/item.do?method=list&type=0&keyword=&category=0&supplier=0&promoted=0&order=0&desc=0&page=1&size=100");
        }

        private async Task<bool> SyncProductList(string where, string url)
        {
            var productList = new List<ProductItem>();
            int page = 1;
            HtmlElement body = null;
            ApiPagedResult<List<ProductItem>> pagedList = new ApiPagedResult<List<ProductItem>>();
            while (pagedList.Success && pagedList.HasMore)
            {
                var nextUrl = UrlHelper.SetValue(url, "page", page.ToString());
                body = await wb.SynchronousLoad(nextUrl).ConfigureAwait(false);
                pagedList = ProductItemHelper.GetProductItemList(body, page);
                tbcpCrumbs = body.JquerySelectInputHidden("tbcpCrumbs");
                AppendText("同步{0}商品第{1}页（{2}个）", where, page, pagedList.Data.Count);
                productList.AddRange(pagedList.Data);
                await Task.Delay(100);
                page++;
            }
            if (!pagedList.Success)
            {
                AppendText("同步{0}商品第{1}页出错，{2}", where, page, pagedList.Message);
                return false;
            }
            else
            {
                var dt = DateTime.Now;
                foreach (var item in productList)
                {
                    item.UpdateAt = dt;
                    item.Where = where;
                }
                AppDatabase.UpsertProductList(productList);
                AppendText("同步{0}商品完成！（{1}个）", where, productList.Count);
                return true;
            }
        }

        private void Client_ProxyInvokeException(Exception ex)
        {
            AppendText(ex.Message);
        }

        private void Connection_StateChanged(StateChange state)
        {
            if (!!cts.IsCancellationRequested && !this.IsDisposed)
            {
                this.SmartInvoke(() =>
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
            this.SmartInvokeAsync(() =>
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
                        text = DateTime.Now.ToString("MM-dd HH:mm:ss") + " " + (args.Any() ? string.Format(text, args) : text);
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
                    for (int i = 0; i < Application.OpenForms.Count; i++)
                    {
                        Form item = Application.OpenForms[i];
                        if (item.Handle != this.Handle && !item.IsDisposed)
                        {
                            item.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
            RestartApplication = false;
        }

        public async Task ThreadLoopCloseTrade()
        {
            while (!cts.IsCancellationRequested)
            {
                //如果 （S当前时间-S创建时间>1min) 就关闭单子
                //亦 (S当前时间-1min>S创建时间)
                var dts = ServerNow.AddMinutes(-1);
                var trade = AppDatabase.db.TopTrades.FindOne(x => (x.Status == "WAIT_BUYER_PAY" || x.Status == "TRADE_NO_CREATE_PAY") && dts > x.Created);
                if (trade != null)
                {
                    await CloseTradeIfPossible(trade.Tid);
                }
                else
                    await Task.Delay(500);
            }
        }
        public void ThreadLoopMonitorDelay()
        {
            Task.Factory.StartNew(() =>
            {
                Stopwatch sw = new Stopwatch();
                while (!cts.IsCancellationRequested)
                {
                    sw.Restart();
                    try
                    {
                        var headers = HttpHelper.ReadResponseHeaders("http://chongzhi.taobao.com/welcome.dox?method=welcome");
                        sw.Stop();
                        var dtTaobao = DateTime.Parse(headers["Date"]); //淘宝时间
                        var dtNow = DateTime.Now.AddTicks(-sw.ElapsedTicks); //当前时间-延迟=淘宝发出响应时（仅发出未收到）的本地的时间

                        DateDifference = dtNow - dtTaobao; //本地时间与淘宝时间的时间差

                        //当数据从淘宝过来时候，时间加上diff就是对应的本地时间

                        ServerNow = dtNow - DateDifference;

                        if (DateDifference.TotalMilliseconds > 0)
                        {
                            //本地时间比淘宝时间快
                        }
                        else
                        {

                        }
                        this.SmartInvoke(() =>
                        {
                            延迟.Text = "延迟：" + sw.ElapsedMilliseconds + "ms";
                        });
                    }
                    catch (Exception ex)
                    {
                        this.SmartInvoke(() =>
                        {
                            延迟.Text = "延迟：...";
                        });
                    }
                    Task.Delay(5 * 1000).Wait();
                }
            });
        }

        public async Task ThreadLoopSyncTrade()
        {
            while (!cts.IsCancellationRequested)
            {
                await Task.Delay(1000 * 60 * 5, cts.Token);
                if (!IsAutoRateRunning)
                {
                    IsAutoRateRunning = true;
                    try
                    {
                        await SyncTradeListIncrease(false);
                    }
                    catch (Exception ex)
                    {
                        AppendException(ex);
                    }
                    IsAutoRateRunning = false;
                }
            }
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

        private void buttonAccount_Click(object sender, EventArgs e)
        {
            wbTaoChongZhiMode.NavigatedToAccountURL();
        }

        private void buttonTrade_Click(object sender, EventArgs e)
        {
            wbTaoChongZhiMode.NavigatedToTradeURL();
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
            this.SmartInvokeAsync(() =>
            {
                var selected = GetSelectedProductList();
                dataGridViewItem.DataSource = dt;
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
            blist = new BlackListForm("黑名单管理", "blist.data");
            blist.Show(this);
        }

        private void buttonWlistMgr_Click(object sender, EventArgs e)
        {
            wlist = new WhiteListForm("白名单管理", "wlist.data");
            wlist.Show(this);
        }

        private async Task WBTaoDurexValidationMode()
        {
            await GettbcpCrumbs();
            //二次验证
            var preUrl = "http://chongzhi.taobao.com/ajax.do?method=getDurexParam&type=0";
            var content = await wb.ExecuteTriggerJSONP(preUrl);
            var durexParam = DynamicJson.Parse(content).ret.param;
            if (durexParam != string.Empty)
            {
                wb.TransactionToNext(wbTaoDurexValidateMode);
                Task.Delay(650).ContinueWith(x => { taoLoginForm.SmartInvoke(() => taoLoginForm.Show(this)); });
                await wbTaoDurexValidateMode.Start(durexParam);
                taoLoginForm.SmartInvoke(() => taoLoginForm.Hide());
            }
        }

        private async Task GettbcpCrumbs()
        {
            Moonlight.WindowsForms.Controls.ExtendedWinFormsWebBrowser exWb = null;
            tabPageWB.SmartInvoke(() =>
            {
                exWb = new Moonlight.WindowsForms.Controls.ExtendedWinFormsWebBrowser();
                exWb.ScriptErrorsSuppressed = true;
                tabPageWB.Controls.Add(exWb);
            });
            var body = await WBHelper.GetWBHeper(exWb, true).SynchronousLoadDocument("http://chongzhi.taobao.com/item.do?method=list&type=1&keyword=&category=0&supplier=0&promoted=0&order=0&desc=0&page=1&size=20").ConfigureAwait(false);
            tabPageWB.SmartInvoke(() =>
            {
                tabPageWB.Controls.Remove(exWb);
                tbcpCrumbs = body.JquerySelectInputHidden("tbcpCrumbs");
            });
        }

        private async void UpProduct(object sender, EventArgs e)
        {
            var productIds = GetSelectedProductList();
            var productList = AppDatabase.db.ProductItems.Find(x => x.Where == "仓库中").Where(x => productIds.Contains(x.Id));
            await UpProduct(productList.ToArray());

            //await WBTaoDurexValidationMode().ConfigureAwait(false);
            //var url = "http://chongzhi.taobao.com/item.do?method=up&tbcpCrumbs=24116cd3c30d7e1d344a52d180a13f20-a444b82dbbc2a&itemIds=521911440067,521906389985,522782426384";
            //url = UrlHelper.SetValue(url, "itemIds", string.Join(",", productList.Select(x => x.Id)));
            //url = UrlHelper.SetValue(url, "tbcpCrumbs", tbcpCrumbs);
            ////{ "status":true,"errmap":{ },"failNum":0,"succNum":3,"msg":"\u4e0b\u67b6\u6210\u529f\uff0c\u6210\u529f3\u4e2a"}
            //var result = await (Task<string>)this.Invoke(new SynchronousLoadStringDelegate(wb.SynchronousLoadString), url);
            //OnProductDownOrUpResult(result, productList, "出售中");
        }

        public async Task<bool> SetProductProfit(ProductItem product, Func<ProductItem, decimal> Getprofit)
        {
            decimal profit = Getprofit(product);
            SuplierInfo supplier = null;
            if (!string.IsNullOrEmpty(product.SupplierId))
            {
                supplier = product.GetSuplierInfo();
            }
            else
            {
                //查找供应商
                supplier = await ((Task<SuplierInfo>)this.Invoke(new supplierInfoDelegate(BizHelper.supplierInfo), wb, product.SpuId)).ConfigureAwait(false);
                product.OnSupplierInfoUpdate(AppDatabase.db.ProductItems, supplier);
            }
            if (supplier != null)
            {
                if (supplier.profitData[0].price == product.进价 && product.利润 == profit)
                {
                    //价格无变化不处理
                    AppendText("商品【{0}】进价及利润无变化，跳过。", product.ItemName);
                    return true;
                }
                string profitString = "0.00";
                profitString = profit.ToString("f2");
                var oneprice = (supplier.profitData[0].price + profit).ToString("f2");

                //充值平台不支持改价API
                //var apirsp = await client.ItemHub.ItemUpdatePrice(product.Id, supplier.profitData[0].price + profit);
                //if (apirsp.Success)
                //{
                //    product.OnProfitInfoUpdate(AppDatabase.db.ProductItems, supplier.profitData[0].price, profit);
                //    return true;
                //}
                //else
                //{
                //    AppendText("为商品{0}设置利润时失败。错误消息：{1}", product.Id, apirsp.Message);
                //    return false;
                //}
                var save = await ((Task<TaoJsonpResult>)this.Invoke(new SupplierSaveDelegate(BizHelper.supplierSave), wb, supplier.profitData[0].id, product.SpuId, profitString, profitString, oneprice, product.Id, tbcpCrumbs)).ConfigureAwait(false);
                if (save.status != 200)
                {
                    AppendText("为商品{0}设置利润时失败。错误消息：{1}", product.Id, save.msg);
                    return false;
                }
                else
                {
                    //product.OnProfitInfoUpdate(AppDatabase.db.ProductItems, supplier.profitData[0].price, profit);
                    return true;
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
                await SetProductProfit(product, (x) => profit);
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
                });
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
                    if (qty != 0)
                    {
                        var apix = await client.ItemHub.ItemUpdateList(product.Id, qty);
                        if (apix.Success)
                        {
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
                        AppendText("无法处理，库存设置异常！");
                    }
                }
                else
                {
                    AppendText("类型为{0}的商品{1}没有设置为锁定库存，忽略。", product.ItemSubName, product.Id);
                }
            }
        }

        private async Task UpProduct(long[] Ids)
        {
            var productList = AppDatabase.db.ProductItems.Find(x => x.Where == "仓库中");
            productList = productList.Where(x => Ids.Contains(x.Id));
            if (productList == null || !productList.Any())
            {
                AppendText("没有可以上架的商品");
                return;
            }
            await WBTaoDurexValidationMode().ConfigureAwait(false);
            var url = "http://chongzhi.taobao.com/item.do?method=up&tbcpCrumbs=24116cd3c30d7e1d344a52d180a13f20-a444b82dbbc2a&itemIds=521911440067,521906389985,522782426384";
            url = UrlHelper.SetValue(url, "itemIds", string.Join(",", productList.Select(x => x.Id)));
            url = UrlHelper.SetValue(url, "tbcpCrumbs", tbcpCrumbs);
            //{ "status":true,"errmap":{ },"failNum":0,"succNum":3,"msg":"\u4e0b\u67b6\u6210\u529f\uff0c\u6210\u529f3\u4e2a"}
            var result = await (Task<string>)this.Invoke(new SynchronousLoadStringDelegate(wb.SynchronousLoadString), url);
            OnProductDownOrUpResult(result, productList.ToList(), "出售中");
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
            //await WBTaoDurexValidationMode().ConfigureAwait(false);
            //var url = "http://chongzhi.taobao.com/item.do?method=up&tbcpCrumbs=24116cd3c30d7e1d344a52d180a13f20-a444b82dbbc2a&itemIds=521911440067,521906389985,522782426384";
            //url = UrlHelper.SetValue(url, "itemIds", string.Join(",", productList.Select(x => x.Id)));
            //url = UrlHelper.SetValue(url, "tbcpCrumbs", tbcpCrumbs);
            ////{ "status":true,"errmap":{ },"failNum":0,"succNum":3,"msg":"\u4e0b\u67b6\u6210\u529f\uff0c\u6210\u529f3\u4e2a"}
            //var result = await (Task<string>)this.Invoke(new SynchronousLoadStringDelegate(wb.SynchronousLoadString), url);
            //OnProductDownOrUpResult(result, productList.ToList(), "出售中");
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
            //await WBTaoDurexValidationMode().ConfigureAwait(false);
            //var url = "http://chongzhi.taobao.com/item.do?method=down&tbcpCrumbs=24116cd3c30d7e1d344a52d180a13f20-a444b82dbbc2a&itemIds=521911440067,521906389985,522782426384";
            //url = UrlHelper.SetValue(url, "itemIds", string.Join(",", productList.Select(x => x.Id)));
            //url = UrlHelper.SetValue(url, "tbcpCrumbs", tbcpCrumbs);
            ////{ "status":true,"errmap":{ },"failNum":0,"succNum":3,"msg":"\u4e0b\u67b6\u6210\u529f\uff0c\u6210\u529f3\u4e2a"}
            //var result = await (Task<string>)this.Invoke(new SynchronousLoadStringDelegate(wb.SynchronousLoadString), url);
            //OnProductDownOrUpResult(result, productList, "仓库中");
        }

        private async Task DownProduct(long[] Ids)
        {
            var productList = AppDatabase.db.ProductItems.Find(x => x.Where == "出售中");
            productList = productList.Where(x => Ids.Contains(x.Id));
            if (!productList.Any())
            {
                AppendText("没有可以下架的商品");
                return;
            }
            foreach (var item in productList)
            {
                await client.ItemHub.ItemUpdateDelist(item.Id);
            }
            //await WBTaoDurexValidationMode().ConfigureAwait(false);
            //var url = "http://chongzhi.taobao.com/item.do?method=down&tbcpCrumbs=24116cd3c30d7e1d344a52d180a13f20-a444b82dbbc2a&itemIds=521911440067,521906389985,522782426384";
            //url = UrlHelper.SetValue(url, "itemIds", string.Join(",", productList.Select(x => x.Id)));
            //url = UrlHelper.SetValue(url, "tbcpCrumbs", tbcpCrumbs);
            ////{ "status":true,"errmap":{ },"failNum":0,"succNum":3,"msg":"\u4e0b\u67b6\u6210\u529f\uff0c\u6210\u529f3\u4e2a"}
            //var result = await (Task<string>)this.Invoke(new SynchronousLoadStringDelegate(wb.SynchronousLoadString), url);
            //OnProductDownOrUpResult(result, productList.ToList(), "仓库中");
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
            }

            //await WBTaoDurexValidationMode().ConfigureAwait(false);
            //var url = "http://chongzhi.taobao.com/item.do?method=down&tbcpCrumbs=24116cd3c30d7e1d344a52d180a13f20-a444b82dbbc2a&itemIds=521911440067,521906389985,522782426384";
            //url = UrlHelper.SetValue(url, "itemIds", string.Join(",", productList.Select(x => x.Id)));
            //url = UrlHelper.SetValue(url, "tbcpCrumbs", tbcpCrumbs);
            //var result = await (Task<string>)this.Invoke(new SynchronousLoadStringDelegate(wb.SynchronousLoadString), url);
            //OnProductDownOrUpResult(result, productList, "仓库中");
        }

        private async void OnProductDownOrUpResult(string result, List<ProductItem> productList, string newState)
        {
            //{ "status":true,"errmap":{ },"failNum":0,"succNum":3,"msg":"\u4e0b\u67b6\u6210\u529f\uff0c\u6210\u529f3\u4e2a"}
            var definition = DynamicJson.Parse(result);
            if (definition.status && definition.succNum == productList.Count)
            {
                foreach (var item in productList)
                {
                    item.Where = newState;
                }
                AppDatabase.UpsertProductList(productList);

                this.SmartInvoke(() =>
                {
                    BindDGViewProduct();
                });
                await GetTaoInfo();
            }
            AppendText(definition.msg);
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

        private async void 刷新列表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await SyncAllProductList();
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
        private DateTime ServerNow;

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
                var slectedRows = new List<DataGridViewRow>();
                foreach (var item in rows)
                {
                    var productId = (long)((System.Data.DataRowView)item.DataBoundItem).Row.ItemArray[0];
                    if (productIds.Any(x => x == productId))
                    {
                        item.Selected = true;
                        slectedRows.Add(item);
                    }
                }
            }
            this.SmartInvoke(() =>
            {
                if ((Control.ModifierKeys & Keys.Control) == Keys.Control)// CTRL is pressed
                {
                    AppendText("增选" + productIds.Count());
                }
            });
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
            修改库存ToolStripMenuItem2.ShowingOption = Moonlight.WindowsForms.StateControls.StateItemShowingOption.HideIfNoneTarget;
            锁定库存ToolStripMenuItem.Showing += 锁定库存ToolStripMenuItem_ShownOnContext;
            取消锁定库存ToolStripMenuItem.Showing += 取消锁定库存ToolStripMenuItem_ShownOnContext;
            开启自动上架ToolStripMenuItem.Showing += 开启自动上架ToolStripMenuItem_ShownOnContext;
            关闭自动上架ToolStripMenuItem.Showing += 关闭自动上架ToolStripMenuItem_ShownOnContext;
            获取供应商信息ToolStripMenuItem.ShowingOption = Moonlight.WindowsForms.StateControls.StateItemShowingOption.HideIfNoneTarget;
            反选ToolStripMenuItem1.ShowingOption = Moonlight.WindowsForms.StateControls.StateItemShowingOption.DisableIfNoneTarget;
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

        private async void 刷新ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await SyncAllProductList();
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
            一键不赔钱.Enabled = false;
            await SetProfitDirect(AppSetting.UserSetting.Get<decimal>("赔钱利润")).ConfigureAwait(false);
            await Task.Delay(3).ContinueWith((x) =>
            {
                this.SmartInvoke(() => { 一键赔钱.Enabled = true; 一键不赔钱.Enabled = true; });
            });
        }

        private async void 一键不赔钱_Click(object sender, EventArgs e)
        {
            一键不赔钱.Enabled = false;
            await SetProfitDirect(AppSetting.UserSetting.Get<decimal>("不赔钱利润")).ConfigureAwait(false);
            await Task.Delay(3).ContinueWith((x) =>
              {
                  this.SmartInvoke(() => { 一键赔钱.Enabled = true; 一键不赔钱.Enabled = true; });
              });
        }

        private async void 获取供应商信息ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ids = GetSelectedProductList();
            var products = AppDatabase.db.ProductItems.FindAll().Where(x => ids.Contains(x.Id));
            await SyncSupplierInfo(products.ToArray());
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
    delegate Task<bool> SyncProductListDelegate();
    delegate Task<HtmlElement> SynchronousLoadDelegate(string url);
    delegate Task<string> SynchronousLoadStringDelegate(string url);
    delegate Task InNoneRetTaskDelegate();
    delegate Task SetProfitBySubNameDelegate(List<ProductItem> list);
    delegate Task<TaoJsonpResult> SupplierSaveDelegate(Microsoft.Phone.Tools.ExtendedWinFormsWebBrowser wb, string sup, string spu, string profitMin, string profitMax, string price, long itemId, string tbcpCrumbs);
}

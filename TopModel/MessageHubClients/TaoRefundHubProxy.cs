using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using TopModel.Models;
using Top.Api.Domain;
using Top.Tmc;

namespace TopModel.MessageHubClients
{
    /// <summary>
    /// 淘宝退款消息及操作
    /// </summary>
    public class TaoRefundHubProxy : HubProxyInvoker
    {
        /// <summary>
        /// 退款创建消息
        /// </summary>
        /// <remarks>
        /// 买家收到货，不满意可以进入“我的淘宝”—“我是买家”—“已买到的宝贝”页面找到对应交易订单，点击“申请退款”。 当创建退款时，会产生此消息，同时会创建退款留言，会产生消息“RefundCreateMessage”。目前只有通过页面操作可产生创建退款消息。
        /// </remarks>
        public event Action<Message> RefundCreated;
        /// <summary>
        /// 卖家同意退款协议消息
        /// </summary>
        /// <remarks>卖家收到退款申请，点击同意退款协议 当卖家通过页面同意退款协议时，会发此消息。</remarks>
        public event Action<Message> RefundSellerAgreeAgreement;
        /// <summary>
        /// 卖家拒绝退款协议消息
        /// </summary>
        /// <remarks>卖家收到退款申请，点击拒绝退款协议 当卖家通过页面拒绝退款协议时，会发此消息。 当卖家通过退款api(taobao.refund.refuse)退款时，会发此消息。</remarks>
        public event Action<Message> RefundSellerRefuseAgreement;
        /// <summary>
        /// 买家修改退款协议消息
        /// </summary>
        /// <remarks>
        /// 如果买家开始是拒绝退款协议，修改成了同意，订阅后返回买家修改退款协议信息。 当买家通过页面修改退款协议时，会发此消息。
        /// </remarks>
        public event Action<Message> RefundBuyerModifyAgreement;
        /// <summary>
        /// 买家退货给卖家消息
        /// </summary>
        /// <remarks>买家收到货不满意申请退货 当买家在页面中退货给卖家时，会发此消息。</remarks>
        public event Action<Message> RefundBuyerReturnGoods;
        /// <summary>
        /// 发表退款留言消息
        /// </summary>
        /// <remarks>在退款协议中发表留言 当创建退款时，会同时退款留言所以会产生此消息。 当通过页面修改退款留言时，会产生此消息。 当通过发表退款留言api(taobao.refund.message.add)添加退款消息时，会产生此消息。</remarks>
        public event Action<Message> RefundCreateMessage;
        /// <summary>
        /// 屏蔽退款留言消息
        /// </summary>
        public event Action<Message> RefundBlockMessage;
        /// <summary>
        /// 退款超时提醒消息
        /// </summary>
        /// <remarks>根据退款超时规则，超过规则中的期限。</remarks>
        public event Action<Message> RefundTimeoutRemind;
        /// <summary>
        /// 退款关闭消息
        /// </summary>
        /// <remarks>退款申请未成功，退款关闭。 当页面买家将退款关闭时，会产生此消息。 未确认收货时，发起退款过程，卖家拒绝退款后，买家还确认收货时，此时也会产生此消息。</remarks>
        public event Action<Message> RefundClosed;
        /// <summary>
        /// 退款成功消息
        /// </summary>
        /// <remarks>当退款完成后(卖家退款给买家)，会产生此消息。</remarks>
        public event Action<Message> RefundSuccess;

        /// <summary>
        /// 淘宝退款消息及操作
        /// </summary>
        /// <param name="connection"></param>
        public TaoRefundHubProxy(HubConnection connection) : base(connection, "RefundMessageHub")
        {
            HubProxy.On<Message>("RefundCreated", x => InvokeEvent(RefundCreated, x));
            HubProxy.On<Message>("RefundSellerAgreeAgreement", x => InvokeEvent(RefundSellerAgreeAgreement, x));
            HubProxy.On<Message>("RefundSellerRefuseAgreement", x => InvokeEvent(RefundSellerRefuseAgreement, x));
            HubProxy.On<Message>("RefundBuyerModifyAgreement", x => InvokeEvent(RefundBuyerModifyAgreement, x));
            HubProxy.On<Message>("RefundBuyerReturnGoods", x => InvokeEvent(RefundBuyerReturnGoods, x));
            HubProxy.On<Message>("RefundCreateMessage", x => InvokeEvent(RefundCreateMessage, x));
            HubProxy.On<Message>("RefundBlockMessage", x => InvokeEvent(RefundBlockMessage, x));
            HubProxy.On<Message>("RefundTimeoutRemind", x => InvokeEvent(RefundTimeoutRemind, x));
            HubProxy.On<Message>("RefundClosed", x => InvokeEvent(RefundClosed, x));
            HubProxy.On<Message>("RefundSuccess", x => InvokeEvent(RefundSuccess, x));
        }

        /// <summary>
        /// 卖家拒绝退款
        /// </summary>
        /// <param name="refundId"></param>
        /// <param name="refusemsg"></param>
        /// <param name="tid"></param>
        /// <param name="oid"></param>
        /// <param name="refuseProof"></param>
        /// <returns></returns>
        /// <remarks>卖家拒绝单笔退款（包含退款和退款退货）交易，要求如下： 1. 传入的refund_id和相应的tid, oid必须匹配 2. 如果一笔订单只有一笔子订单，则tid必须与oid相同 3. 只有卖家才能执行拒绝退款操作 4. 以下三种情况不能退款：卖家未发货；7天无理由退换货；网游订单</remarks>
        public async Task<ApiResult<Refund>> RefundRefuse(long refundId, string refusemsg, long tid, long oid, byte[] refuseProof)
        {
            return await ProxyInvoke<ApiResult<Refund>>("RefundRefuse", refundId, refusemsg, tid, oid, refuseProof);
        }
    }
}

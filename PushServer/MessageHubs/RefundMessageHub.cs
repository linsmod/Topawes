using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Top.Api;
using Top.Api.Domain;
using Top.Api.Request;
using Top.Api.Response;
using Top.Api.Util;
using TopModel.Models;
using TopModel;
using Microsoft.AspNet.SignalR;

namespace PushServer.MessageHubs
{
    public interface ITaoRefundMessageClient
    {
        /// <summary>
        /// 退款创建消息
        /// </summary>
        /// <remarks>
        /// 买家收到货，不满意可以进入“我的淘宝”—“我是买家”—“已买到的宝贝”页面找到对应交易订单，点击“申请退款”。 当创建退款时，会产生此消息，同时会创建退款留言，会产生消息“RefundCreateMessage”。目前只有通过页面操作可产生创建退款消息。
        /// </remarks>
        void RefundCreated(object msg);
        /// <summary>
        /// 卖家同意退款协议消息
        /// </summary>
        /// <remarks>卖家收到退款申请，点击同意退款协议 当卖家通过页面同意退款协议时，会发此消息。</remarks>
        void RefundSellerAgreeAgreement(object msg);
        /// <summary>
        /// 卖家拒绝退款协议消息
        /// </summary>
        /// <remarks>卖家收到退款申请，点击拒绝退款协议 当卖家通过页面拒绝退款协议时，会发此消息。 当卖家通过退款api(taobao.refund.refuse)退款时，会发此消息。</remarks>
        void RefundSellerRefuseAgreement(object msg);
        /// <summary>
        /// 买家修改退款协议消息
        /// </summary>
        /// <remarks>
        /// 如果买家开始是拒绝退款协议，修改成了同意，订阅后返回买家修改退款协议信息。 当买家通过页面修改退款协议时，会发此消息。
        /// </remarks>
        void RefundBuyerModifyAgreement(object msg);
        /// <summary>
        /// 买家退货给卖家消息
        /// </summary>
        /// <remarks>买家收到货不满意申请退货 当买家在页面中退货给卖家时，会发此消息。</remarks>
        void RefundBuyerReturnGoods(object msg);
        /// <summary>
        /// 发表退款留言消息
        /// </summary>
        /// <remarks>在退款协议中发表留言 当创建退款时，会同时退款留言所以会产生此消息。 当通过页面修改退款留言时，会产生此消息。 当通过发表退款留言api(taobao.refund.message.add)添加退款消息时，会产生此消息。</remarks>
        void RefundCreateMessage(object msg);
        /// <summary>
        /// 屏蔽退款留言消息
        /// </summary>
        void RefundBlockMessage(object msg);
        /// <summary>
        /// 退款超时提醒消息
        /// </summary>
        /// <remarks>根据退款超时规则，超过规则中的期限。</remarks>
        void RefundTimeoutRemind(object msg);
        /// <summary>
        /// 退款关闭消息
        /// </summary>
        /// <remarks>退款申请未成功，退款关闭。 当页面买家将退款关闭时，会产生此消息。 未确认收货时，发起退款过程，卖家拒绝退款后，买家还确认收货时，此时也会产生此消息。</remarks>
        void RefundClosed(object msg);
        /// <summary>
        /// 退款成功消息
        /// </summary>
        /// <remarks>当退款完成后(卖家退款给买家)，会产生此消息。</remarks>
        void RefundSuccess(object msg);
    }
    [Authorize]
    public class RefundMessageHub : TopawesHub<ITaoRefundMessageClient>
    {
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
        public ApiResult<Refund> RefundRefuse(long refundId, string refusemsg, long tid, long oid, byte[] refuseProof)
        {
            var client = TopManager.GetTopClient();
            RefundRefuseRequest req = new RefundRefuseRequest();
            req.RefundId = refundId;
            req.RefuseMessage = refusemsg;
            req.Tid = tid;
            req.Oid = oid;
            string tempFile = null;
            if (refuseProof != null && refuseProof.Any())
            {
                tempFile = Path.Combine(BaseDirectory, Path.GetTempFileName());
                File.WriteAllBytes(tempFile, refuseProof);
                req.RefuseProof = new FileItem(tempFile);
            }
            RefundRefuseResponse rsp = client.Execute(req, AccessToken);
            if (tempFile != null)
            {
                File.Delete(tempFile);
            }
            return rsp.AsApiResult(()=>rsp.Refund);
        }
    }
}

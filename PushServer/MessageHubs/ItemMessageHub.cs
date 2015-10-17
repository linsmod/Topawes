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
using Top.Api.Response;
using TopModel;
namespace PushServer.MessageHubs
{
    public interface ItemMessageClient
    {
        /// <summary>
        /// 修改商品库存消息
        /// </summary>
        /// <remarks>商品库存发生变化发送消息 当通过api (taobao.item.quantity.update，或taobao.item.sku.update更改数量) 修改商品库存时，会产生上面的消息。商品库存数量和变化量均会返回。</remarks>
        void ItemStockChanged(object msg);

        /// <summary>
        /// 商品卖空消息
        /// </summary>
        /// <remarks>商品卖空。 当商品库存为0时，会产生此消息。</remarks>
        void ItemZeroStock(object msg);

        /// <summary>
        /// 商品更新消息
        /// </summary>
        /// <remarks>卖家更新商品信息，库存变化不发送此消息 当通过商品更新api(taobao.item.update)更新商品时，会产生此消息。 当通过页面更新商品时，会产生此消息。</remarks>
        void ItemUpdate(object msg);

        /// <summary>
        /// 商品删除消息
        /// </summary>
        /// <remarks>卖家删除商品。 当通过商品删除api(taobao.item.delete)删除商品时，会产生此消息。 当通过页面删除商品时，会产生此消息。</remarks>
        void ItemDelete(object msg);

        /// <summary>
        /// 商品下架消息
        /// </summary>
        /// <remarks>卖家将商品下架。 当通过下架api（taobao.item.update.delisting）下架商品时,会产生此消息。 当通过页面下架商品时,会产生此消息。</remarks>
        void ItemDownshelf(object msg);

        /// <summary>
        /// 商品上架消息
        /// </summary>
        /// <remarks>卖家将商品上架。 当通过上架api（taobao.item.update.listing )上架商品时,会产生此消息。 当通过页面上架商品时,会产生此消息。</remarks>
        void ItemUpshelf(object msg);

        /// <summary>
        /// 商品新增消息
        /// </summary>
        /// <remarks> 卖家发布新商品。 当通过商品api 添加新的商品时,会产生此消息。 当通过页面添加新的商品时,会产生此消息。 </remarks>
        void ItemAdd(object msg);
    }

    public class ItemMessageHub : TopawesHub<ItemMessageClient>
    {
        /// <summary>
        /// 修改库存
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="qty"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public ApiResult ItemQuantityUpdate(long itemId, int qty, int type = 1)
        {
            var client = GetTopClient();
            ItemQuantityUpdateRequest request = new ItemQuantityUpdateRequest
            {
                NumIid = itemId,
                Quantity = qty,
                Type = type
            };
            ItemQuantityUpdateResponse rsp = client.Execute(request, AccessToken);
            return rsp.AsApiResult();
        }



        /// <summary>
        /// 下架商品
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public ApiResult ItemUpdateDelist(long itemId)
        {
            ITopClient client = GetTopClient();
            ItemUpdateDelistingRequest req = new ItemUpdateDelistingRequest();
            req.NumIid = itemId;
            ItemUpdateDelistingResponse rsp = client.Execute(req, AccessToken);
            return rsp.AsApiResult(); ;
        }

        /// <summary>
        /// 上架商品
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public ApiResult ItemUpdateList(long itemId, long num)
        {
            ITopClient client = GetTopClient();
            ItemUpdateListingRequest req = new ItemUpdateListingRequest();
            req.NumIid = itemId;
            req.Num = num;
            ItemUpdateListingResponse rsp = client.Execute(req, AccessToken);
            return rsp.AsApiResult();
        }

        /// <summary>
        /// 更新商品价格
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="onePrice"></param>
        /// <returns></returns>
        public ApiResult ItemUpdatePrice(long itemId, double onePrice)
        {
            ITopClient client = GetTopClient();
            ItemUpdateRequest req = new ItemUpdateRequest();
            req.NumIid = itemId;
            req.Price = onePrice.ToString("f2");
            ItemUpdateResponse rsp = client.Execute(req, AccessToken);
            return rsp.AsApiResult();
        }

        /// <summary>
        /// 定时上架商品，未测试。
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="num"></param>
        /// <param name="listTime"></param>
        /// <returns></returns>
        public ApiResult ItemUpdateListWhenTime(long itemId, long? num, DateTime? listTime = null)
        {
            ITopClient client = GetTopClient();
            ItemUpdateRequest req = new ItemUpdateRequest();
            req.NumIid = itemId;
            req.ApproveStatus = "onsale";
            req.ListTime = listTime;
            req.Num = num;
            ItemUpdateResponse rsp = client.Execute(req, AccessToken);
            return rsp.AsApiResult();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using TopModel.Models;
using Top.Tmc;

namespace TopModel.MessageHubClients
{
    /// <summary>
    /// 淘宝商品消息和操作
    /// </summary>
    public class ItemHubProxy : HubProxyInvoker
    {
        /// <summary>
        /// 修改商品库存消息
        /// </summary>
        /// <remarks>商品库存发生变化发送消息 当通过api (taobao.item.quantity.update，或taobao.item.sku.update更改数量) 修改商品库存时，会产生上面的消息。商品库存数量和变化量均会返回。</remarks>
        public event Action<Message> ItemStockChanged;

        /// <summary>
        /// 商品卖空消息
        /// </summary>
        /// <remarks>商品卖空。 当商品库存为0时，会产生此消息。</remarks>
        public event Action<Message> ItemZeroStock;

        /// <summary>
        /// 商品更新消息
        /// </summary>
        /// <remarks>卖家更新商品信息，库存变化不发送此消息 当通过商品更新api(taobao.item.update)更新商品时，会产生此消息。 当通过页面更新商品时，会产生此消息。</remarks>
        public event Action<Message> ItemUpdate;

        /// <summary>
        /// 商品删除消息
        /// </summary>
        /// <remarks>卖家删除商品。 当通过商品删除api(taobao.item.delete)删除商品时，会产生此消息。 当通过页面删除商品时，会产生此消息。</remarks>
        public event Action<Message> ItemDelete;

        /// <summary>
        /// 商品下架消息
        /// </summary>
        /// <remarks>卖家将商品下架。 当通过下架api（taobao.item.update.delisting）下架商品时,会产生此消息。 当通过页面下架商品时,会产生此消息。</remarks>
        public event Action<Message> ItemDownshelf;

        /// <summary>
        /// 商品上架消息
        /// </summary>
        /// <remarks>卖家将商品上架。 当通过上架api（taobao.item.update.listing )上架商品时,会产生此消息。 当通过页面上架商品时,会产生此消息。</remarks>
        public event Action<Message> ItemUpshelf;

        /// <summary>
        /// 商品新增消息
        /// </summary>
        /// <remarks> 卖家发布新商品。 当通过商品api 添加新的商品时,会产生此消息。 当通过页面添加新的商品时,会产生此消息。 </remarks>
        public event Action<Message> ItemAdd;

        /// <summary>
        /// 淘宝商品消息和操作
        /// </summary>
        /// <param name="connection"></param>
        public ItemHubProxy(HubConnection connection) : base(connection, "ItemMessageHub")
        {
            //修改商品库存消息
            HubProxy.On<Message>("ItemStockChanged", (x) => { base.InvokeEvent(ItemStockChanged, x); });

            //商品卖空消息
            HubProxy.On<Message>("ItemZeroStock", x => base.InvokeEvent(ItemZeroStock, x));

            //商品更新消息
            HubProxy.On<Message>("ItemUpdate", x => InvokeEvent(ItemUpdate, x));

            //商品删除消息
            HubProxy.On<Message>("ItemDelete", x => InvokeEvent(ItemDelete, x));

            //商品下架消息
            HubProxy.On<Message>("ItemDownshelf", x => InvokeEvent(ItemDownshelf, x));

            //商品上架消息
            HubProxy.On<Message>("ItemUpshelf", x => InvokeEvent(ItemUpshelf, x));

            //商品新增消息
            HubProxy.On<Message>("ItemAdd", x => InvokeEvent(ItemAdd, x));
        }

        /// <summary>
        /// 修改库存
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="qty"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<ApiResult> ItemQuantityUpdate(long itemId, int qty, int type = 1)
        {
            return await ProxyInvoke<ApiResult>("ItemQuantityUpdate", itemId, qty, type);
        }


        public async Task<ApiResult> ItemUpdatePrice(long itemId, double onePrice)
        {
            return await ProxyInvoke<ApiResult>("ItemUpdatePrice", itemId, onePrice);
        }


        /// <summary>
        /// 下架商品
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public async Task<ApiResult> ItemUpdateDelist(long itemId)
        {
            return await ProxyInvoke<ApiResult>("ItemUpdateDelist", itemId);
        }

        /// <summary>
        /// 上架商品
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public async Task<ApiResult> ItemUpdateList(long itemId, long num)
        {
            return await ProxyInvoke<ApiResult>("ItemUpdateList", itemId, num);
        }

        /// <summary>
        /// 定时上架商品，未测试。
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="num"></param>
        /// <param name="listTime"></param>
        /// <returns></returns>
        public async Task<ApiResult> ItemUpdateListWhenTime(long itemId, long? num, DateTime listTime)
        {
            return await ProxyInvoke<ApiResult>("ItemUpdateListWhenTime", itemId, num, listTime);
        }
    }
}

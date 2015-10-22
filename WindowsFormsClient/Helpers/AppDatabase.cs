using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinFormsClient.Models;
using Top.Api.Domain;
using TopModel.Models;
using TopModel;

namespace WinFormsClient
{
    public class AppDatabase
    {
        public static AppDataStorage db { get; private set; }
        public static string UserName { get; set; }
        public static void Initialize(string userName)
        {
            db = new AppDataStorage(userName);
        }

        public static void InsertProductList(List<ProductItem> items)
        {
            db.ProductItems.InsertBulk(items);
        }

        public static void UpsertProductList(List<ProductItem> items)
        {
            foreach (var item in items)
            {
                var x = db.ProductItems.FindById(item.Id);
                if (x == null)
                {
                    db.ProductItems.Insert(item);
                }
                else
                {
                    x.ItemName = item.ItemName;
                    x.ItemSubName = item.ItemSubName;
                    x.Type = item.Type;
                    x.UpdateAt = DateTime.Now;
                    x.Where = item.Where;
                    x.利润 = item.利润;
                    x.原利润 = item.原利润;
                    x.一口价 = item.一口价;
                    x.进价 = item.进价;
                    db.ProductItems.Update(x);
                }
            }
        }

        public static void UpsertTbOrderList(List<TopTrade> items)
        {
            foreach (var item in items)
            {
                UpsertTopTrade(item);
            }
        }

        public static void UpdateTopTradeState(long id, string state)
        {
            var order = db.TopTrades.FindById(id);
            order.Status = state;
            db.TopTrades.Update(order);
        }

        internal static void UpsertTopTrade(TopTrade item)
        {
            var x = db.TopTrades.FindById(item.Tid);
            if (x == null)
            {
                db.TopTrades.Insert(item);
            }
            else
            {
                x.BuyerNick = item.BuyerNick;
                x.Created = item.Created;
                x.EndTime = item.EndTime;
                x.Num = item.Num;
                x.NumIid = item.NumIid;
                x.Payment = item.Payment;
                x.UpdateAt = DateTime.Now;
                x.PayTime = item.PayTime;
                x.ReceiverAddress = item.ReceiverAddress;
                x.SellerCanRate = item.SellerCanRate;
                x.SellerRate = item.SellerRate;
                x.Status = item.Status;
                db.TopTrades.Update(x);
            }
        }
    }
}

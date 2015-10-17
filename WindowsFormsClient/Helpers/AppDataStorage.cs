using LiteDB;
using Moonlight.EntityStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopModel;
using TopModel.Models;
using WinFormsClient.Models;

namespace WinFormsClient
{
    public class AppDataStorage : DbContext
    {
        public AppDataStorage(string userName) : base(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, userName + ".bin"))
        {

        }

        public LiteCollection<ProductItem> ProductItems
        {
            get
            {
                return Entity<ProductItem>();
            }
        }

        public LiteCollection<TopTrade> TopTrades
        {
            get
            {
                return Entity<TopTrade>();
            }
        }
        public LiteCollection<Statistic> Statistics
        {
            get
            {
                return Entity<Statistic>();
            }
        }
    }
}

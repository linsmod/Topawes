using LiteDB;
using Moonlight.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TopModel;

namespace TopModel.Extensions
{
    static class ProductItemExtension
    {
        public static bool CanUpshelf(this ProductItem product)
        {
            if (KeysHelper.IsControlKeyPressedDown)
            {
                return product.Where == "仓库中";
            }
            return !product.正在上架 && !product.正在下架 && product.Where == "仓库中";
        }


        public static bool CanDownshelf(this ProductItem product)
        {
            if (KeysHelper.IsControlKeyPressedDown)
            {
                return product.Where == "出售中";
            }
            return !product.正在上架 && !product.正在下架 && product.Where == "出售中";
        }

        public static void OnUpshelfing(this ProductItem product, LiteCollection<ProductItem> productItems)
        {
            product.正在上架 = true;
            productItems.Update(product);
        }

        public static void OnDownshelfing(this ProductItem product, LiteCollection<ProductItem> productItems)
        {
            product.正在下架 = true;
            productItems.Update(product);
        }

        public static void OnDownshelf(this ProductItem product, LiteCollection<ProductItem> productItems)
        {
            product.正在上架 =
            product.正在下架 = false;
            product.Where = "仓库中";
            productItems.Update(product);
        }

        public static void OnUpshelf(this ProductItem product, LiteCollection<ProductItem> productItems)
        {
            product.正在上架 =
            product.正在上架 = false;
            product.Where = "出售中";
            productItems.Update(product);
        }
    }
}

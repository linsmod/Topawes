
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFormsClient.BizEventArgs;
using WinFormsClient.Models;
using WinFormsClient.Extensions;
using WinFormsClient.HtmlEntity;
using TopModel.Models;
using TopModel;
using Nx.EasyHtml.Html;

namespace WinFormsClient.Helpers
{
    public class ProductItemHelper
    {
        public static ApiPagedResult<List<ProductItem>> GetProductItemList(IHtmlElement stockTable, int page = 1)
        {
            var result = new ApiPagedResult<List<ProductItem>>();
            var tableEntity = new TableEntity(stockTable);
            if (tableEntity.TBody.TrList.Count == 1 && tableEntity.TBody.TrList[0].TrElement.InnerHtml().IndexOf("没有符合条件的结果") != -1)
            {
                result.HasMore = false;
                result.Data = new List<ProductItem>();
            }
            else
            {
                var list = tableEntity.TBody.TrList.Select(x => x.GetProductItem()).ToList();
                var nextUrl = stockTable.Container.Find("a.page-next");
                if (nextUrl.Any())
                {
                    var pageNext = UrlHelper.GetIntValue(nextUrl.First().Attribute("href").AttributeValue, "page");
                    result.HasMore = pageNext > page;
                }
                else
                {
                    result.HasMore = false;
                }
                result.Data = list;
            }
            tableEntity.Dispose();
            return result;
        }
    }
}

using Microsoft.Phone.Tools;
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

namespace WinFormsClient.Helpers
{
    public class ProductItemHelper
    {
        public static ApiPagedResult<List<ProductItem>> GetProductItemList(HtmlElement body, int page = 1)
        {
            try
            {
                var result = new ApiPagedResult<List<ProductItem>>();
                var tables = body.JQuerySelect("table.stock-table");
                if (tables.Any())
                {
                    var tableEntity = new TableEntity(tables[0]);
                    if (tableEntity.TBody.TrList.Count == 1 && tableEntity.TBody.TrList[0].TrElement.InnerText.StartsWith("没有符合条件的结果"))
                    {
                        result.HasMore = false;
                        result.Data = new List<ProductItem>();
                    }
                    else
                    {
                        var list = tableEntity.TBody.TrList.Select(x => x.GetProductItem()).ToList();

                        var nextUrl = body.JQuerySelect("page-next");
                        if (nextUrl.Any())
                        {
                            var pageNext = UrlHelper.GetIntValue(nextUrl[0].GetAttribute("href"), "page");
                            result.HasMore = pageNext > page;
                        }
                        else
                        {
                            result.HasMore = false;
                        }
                        result.Data = list;
                    }
                }
                else
                {
                    result.Success = false;
                    result.Message = "在页面中没有找到产品数据表格";
                }
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}

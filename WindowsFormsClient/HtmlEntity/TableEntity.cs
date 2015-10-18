using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TopModel;
using WinFormsClient.Extensions;
using WinFormsClient.Models;

namespace WinFormsClient.HtmlEntity
{
    /// <summary>
    /// HTML table元素
    /// </summary>
    public class TableEntity
    {
        public TableEntity(HtmlElement tableElement)
        {
            var head = tableElement.JQuerySelect("thead");
            this.THead = new THeadEntity(head[0]);

            var headTrList = head.JQuerySelect("tr");
            foreach (var tr in headTrList)
            {
                var trEntity = new TrEntity(tr);
                var tdList = tr.Children.ToList();
                foreach (var td in tdList)
                {
                    trEntity.TdList.Add(new TdEntity(td) { Text = td.InnerText });
                }
                this.THead.TrList.Add(trEntity);
            }
            var body = tableElement.JQuerySelect("tbody");
            this.TBody = new TBodyEntity(body[0]);

            var bodyTrList = body.JQuerySelect("tr");
            foreach (var tr in bodyTrList)
            {
                var trEntity = new TrEntity(tr);
                var tdList = tr.Children.ToList();
                foreach (var td in tdList)
                {
                    trEntity.TdList.Add(new TdEntity(td) { Text = td.InnerText });
                }
                this.TBody.TrList.Add(trEntity);
            }
        }
        public THeadEntity THead { get; set; }
        public TBodyEntity TBody { get; set; }
    }

    [DebuggerDisplay("Count={TdList.Count}", Name = "TrEntity", TargetTypeName = "TrEntity")]
    public class TrEntity
    {
        public TrEntity(HtmlElement tr)
        {
            this.TrElement = tr;
        }
        public HtmlElement TrElement { get; private set; }
        public List<TdEntity> TdList = new List<TdEntity>();
        public ProductItem GetProductItem()
        {
            var item = new ProductItem();
            item.Id = long.Parse(TrElement.GetAttribute("data-id"));
            item.SpuId = TrElement.GetAttribute("data-spuid");
            item.ItemName = TrElement.JQuerySelect(".item-name a")[0].InnerText;
            item.ItemSubName = new string(TrElement.JQuerySelect(".item-name")[0].InnerText.Skip(item.ItemName.Length + 6).ToArray());
            item.Type = TrElement.JQuerySelect(".type")[0].InnerText;
            item.面值 = TrElement.JQuerySelect(".price")[0].InnerText;
            decimal price = 0;
            if (decimal.TryParse(TrElement.JQuerySelect(".buy-price")[0].InnerText.Trim(new char[] { ' ', '元' }), out price))
            {
                item.进价 = price;
            }
            item.一口价 = decimal.Parse(TrElement.JQuerySelect(".one-price-text")[0].InnerText.Trim(new char[] { ' ', '元' }));
            return item;
        }
    }

    [DebuggerDisplay("Count={TrList.Count}", Name = "THeadEntity", TargetTypeName = "THeadEntity")]
    public class THeadEntity
    {
        public THeadEntity(HtmlElement theadElement)
        {
            this.THeadElement = theadElement;
        }
        public HtmlElement THeadElement { get; private set; }
        public List<TrEntity> TrList = new List<TrEntity>();
    }

    [DebuggerDisplay("Count={TrList.Count}", Name = "TBodyEntity", TargetTypeName = "TBodyEntity")]
    public class TBodyEntity
    {
        public TBodyEntity(HtmlElement tbodyElement)
        {
            this.TBodyElement = tbodyElement;
        }
        public HtmlElement TBodyElement { get; private set; }
        public List<TrEntity> TrList = new List<TrEntity>();
    }

    [DebuggerDisplay("{Text}", Name = "TdEntity", TargetTypeName = "TdEntity")]
    public class TdEntity
    {
        public TdEntity(HtmlElement tdElement)
        {
            this.TdElement = tdElement;
        }
        public HtmlElement TdElement { get; private set; }
        public string Text { get; set; }

        public override string ToString()
        {
            return this.Text;
        }
    }
}

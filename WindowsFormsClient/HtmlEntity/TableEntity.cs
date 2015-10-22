using Nx.EasyHtml.Html;
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
        public TableEntity(IHtmlElement tableElement)
        {
            var head = tableElement.FindFirst("thead");
            this.THead = new THeadEntity(head);

            var headTrList = head.Find("tr");
            foreach (var tr in headTrList)
            {
                var trEntity = new TrEntity(tr);
                var tdList = tr.Find("td").ToList();
                foreach (var td in tdList)
                {
                    trEntity.TdList.Add(new TdEntity(td) { Text = td.InnerText() });
                }
                this.THead.TrList.Add(trEntity);
            }
            var body = tableElement.FindFirst("tbody");
            this.TBody = new TBodyEntity(body);

            var bodyTrList = body.Find("tr");
            foreach (var tr in bodyTrList)
            {
                var trEntity = new TrEntity(tr);
                var tdList = tr.Find("td").ToList();
                foreach (var td in tdList)
                {
                    trEntity.TdList.Add(new TdEntity(td) { Text = td.InnerText() });
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
        public TrEntity(IHtmlElement tr)
        {
            this.TrElement = tr;
        }
        public IHtmlElement TrElement { get; private set; }
        public List<TdEntity> TdList = new List<TdEntity>();
        public ProductItem GetProductItem()
        {
            var item = new ProductItem();
            item.Id = long.Parse(TrElement.Attribute("data-id").AttributeValue);
            item.SpuId = TrElement.Attribute("data-spuid").AttributeValue;
            item.ItemName = TrElement.FindFirst(".item-name a").InnerText();
            var text = TrElement.FindFirst(".item-name").InnerText();
            var index = TrElement.FindFirst(".item-name").InnerText().IndexOf("类型：");
            item.ItemName = text.Substring(0, index).Trim();
            item.ItemSubName = new string(text.Skip(index + 3).ToArray()).Trim();
            item.Type = TrElement.FindFirst(".type").InnerText();
            item.面值 = TrElement.FindFirst(".price").InnerText();
            decimal price = 0;
            if (decimal.TryParse(TrElement.FindFirst(".buy-price em").InnerText().Trim(new char[] { ' ', '元' }), out price))
            {
                item.进价 = price;
            }
            var onePriceElements = TrElement.Find(".one-price-text");
            if (onePriceElements.Any())
            {
                item.一口价 = decimal.Parse(TrElement.FindFirst(".one-price-text em").InnerText().Trim(new char[] { ' ', '元' }));
            }
            return item;
        }
    }

    [DebuggerDisplay("Count={TrList.Count}", Name = "THeadEntity", TargetTypeName = "THeadEntity")]
    public class THeadEntity
    {
        public THeadEntity(IHtmlElement theadElement)
        {
            this.THeadElement = theadElement;
        }
        public IHtmlElement THeadElement { get; private set; }
        public List<TrEntity> TrList = new List<TrEntity>();
    }

    [DebuggerDisplay("Count={TrList.Count}", Name = "TBodyEntity", TargetTypeName = "TBodyEntity")]
    public class TBodyEntity
    {
        public TBodyEntity(IHtmlElement tbodyElement)
        {
            this.TBodyElement = tbodyElement;
        }
        public IHtmlElement TBodyElement { get; private set; }
        public List<TrEntity> TrList = new List<TrEntity>();
    }

    [DebuggerDisplay("{Text}", Name = "TdEntity", TargetTypeName = "TdEntity")]
    public class TdEntity
    {
        public TdEntity(IHtmlElement tdElement)
        {
            this.TdElement = tdElement;
        }
        public IHtmlElement TdElement { get; private set; }
        public string Text { get; set; }

        public override string ToString()
        {
            return this.Text;
        }
    }
}

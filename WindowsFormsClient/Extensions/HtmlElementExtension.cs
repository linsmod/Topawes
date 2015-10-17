using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsClient.Extensions
{
    public static class HtmlElementExtension
    {
        public static string JquerySelectInputHidden(this HtmlElement body, string name)
        {
            var hiddens = body.JQuerySelect("input[type=hidden]");
            foreach (var item in hiddens)
            {
                var nameOrId = string.IsNullOrEmpty(item.Name) ? item.Id : item.Name;
                if (nameOrId == name)
                {
                    return item.GetAttribute("value");
                }
            }
            return string.Empty;
        }

        public static List<HtmlElement> ToList(this HtmlElementCollection collection)
        {
            var list = new List<HtmlElement>(collection.Count);
            foreach (HtmlElement item in collection)
            {
                list.Add(item);
            }
            return list;
        }

        /// <summary>
        /// 使用JQuery语法筛选元素
        /// </summary>
        /// <param name="element"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static List<HtmlElement> JQuerySelect(this HtmlElement element, string selector)
        {
            var elements = new List<HtmlElement> { element };
            return elements.JQuerySelect(selector);
        }

        /// <summary>
        /// 使用JQuery语法筛选元素
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static List<HtmlElement> JQuerySelect(this IEnumerable<HtmlElement> elements, string selector)
        {
            var HtmlElemSelectorFactory = new HtmlElementSelectorFactory();
            var htmlElementSelector = HtmlElemSelectorFactory.GetSelector(selector);
            return htmlElementSelector.Select(elements.ToList());

        }

        /// <summary>
        /// 使用JQuery语法筛选元素
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static List<HtmlElement> JQuerySelect(this HtmlElementCollection elements, string selector)
        {
            return elements.ToList().JQuerySelect(selector);
        }
    }
}

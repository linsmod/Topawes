using mshtml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows.Forms;

namespace WinFormsClient
{
    public class HtmlHelper
    {
        public static string GetText(Stream stream, Encoding encoding)
        {
            var streamReader = new StreamReader(stream, Encoding.GetEncoding("gb2312"));
            var html = streamReader.ReadToEnd();
            return html;
        }

        public static string UnicodeToGB2312(string str)
        {
            if (str.StartsWith("%5C", StringComparison.OrdinalIgnoreCase))
            {
                str = HttpUtility.UrlDecode(str);
            }
            if (!str.StartsWith("\\")) {
                return str;
            }
            string r = "";
            MatchCollection mc = Regex.Matches(str, @"\\u([\w]{2})([\w]{2})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            byte[] bts = new byte[2];
            foreach (Match m in mc)
            {
                bts[0] = (byte)int.Parse(m.Groups[2].Value, NumberStyles.HexNumber);
                bts[1] = (byte)int.Parse(m.Groups[1].Value, NumberStyles.HexNumber);
                r += Encoding.Unicode.GetString(bts);
            }
            return r;
        }

        public static string GetDocumentText(WebBrowser wb)
        {
            if (!string.IsNullOrEmpty(wb.Document.Encoding))
            {
                return GetText(wb.DocumentStream, Encoding.GetEncoding(wb.Document.Encoding));
            }
            else if (!string.IsNullOrEmpty(wb.Document.DefaultEncoding))
            {
                return GetText(wb.DocumentStream, Encoding.GetEncoding(wb.Document.DefaultEncoding));
            }
            else
            {
                return GetText(wb.DocumentStream, Encoding.UTF8);
            }
        }
    }

    /// <summary>
    /// Html属性选择器[name=value]
    /// </summary>
    public class HtmlElementAttributeSelector : HtmlElementSelector
    {
        public HtmlElementAttributeSelector(string selector) : base(MatchMode.SameLevel)
        {
            this.Selector = selector;
        }
        public string AttributeName { get; set; }
        public string AttributeValue { get; set; }
        public string Operator { get; set; }
        public bool Not { get; set; }
        public override bool Match(HtmlElement element)
        {
            var flag = false;
            var attr = element.GetAttribute(AttributeName);
            if (attr != null)
            {
                switch (Operator)
                {
                    case "=":
                        flag = attr == AttributeValue;
                        break;
                }
                return this.Not ? !flag : flag;
            }
            return flag;
        }
    }

    /// <summary>
    /// Html属性存在性选择器[name]
    /// </summary>
    public class HtmlElementAttributeExistSelector : HtmlElementSelector
    {
        public HtmlElementAttributeExistSelector(string selector) : base(MatchMode.SameLevel)
        {
            this.Selector = selector;
        }
        public string AttributeName { get; set; }
        public override bool Match(HtmlElement element)
        {
            var attr = element.GetAttribute(AttributeName);
            return !string.IsNullOrEmpty(attr);
        }
    }

    /// <summary>
    /// 标签选择器 div
    /// </summary>
    public class HtmlTagAttributeSelector : HtmlElementSelector
    {
        public HtmlTagAttributeSelector(string selector)
        {
            this.Selector = selector;
            this.TagName = selector;
        }
        public string TagName { get; set; }

        public override bool Match(HtmlElement element)
        {
            return element.TagName.Equals(this.TagName, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// ID选择器 #id
    /// </summary>
    public class HtmlIdAttributeSelector : HtmlElementSelector
    {
        public HtmlIdAttributeSelector(string selector)
        {
            this.Selector = selector;
            this.IdAttributeValue = selector.Remove(0, 1);
        }
        public string IdAttributeValue { get; set; }

        public override bool Match(HtmlElement element)
        {
            return element.GetAttribute("id") == IdAttributeValue;
        }
    }

    /// <summary>
    /// Class选择器 .class
    /// </summary>
    public class HtmlClassAttributeSelector : HtmlElementSelector
    {
        public HtmlClassAttributeSelector(string selector)
        {
            this.Selector = selector;
            this.ClassAttributeValue = selector.Remove(0, 1);
        }
        public string ClassAttributeValue { get; set; }

        public override bool Match(HtmlElement element)
        {
            return element.GetAttribute("className").Split(' ').Any(x => x == ClassAttributeValue);
        }
    }

    [DebuggerDisplay("{GetType()},{Selector}")]
    public abstract class HtmlElementSelector
    {
        public HtmlElementSelector(MatchMode matchModel = MatchMode.AnyChildLevel)
        {
            MatchMode = matchModel;
        }
        public string Selector { get; set; }
        public HtmlElementSelector Next { get; set; }
        private bool ProcessDirectSubItems { get; set; }

        public List<HtmlElement> Select(List<HtmlElement> elements)
        {
            var list = new List<HtmlElement>();
            foreach (HtmlElement item in elements)
            {
                if (MatchMode == MatchMode.DirectChildLevel)
                    //直接下级 用于有直接下级标记的选择器 table>tr
                    list.AddRange(SelectMatchedDirectSubElement(item));
                else if (MatchMode == MatchMode.SameLevel)
                {
                    //同级 用于对象属性选择 如table[name=x]
                    var match = SelectMatchedSameLevel(item);
                    if (match != null)
                        list.Add(match);
                }
                else if (MatchMode == MatchMode.AnyChildLevel)
                    //任何下级 用于空格隔开的选择器 div p
                    list.AddRange(SelectMatchedAnySubElement(item));
            }
            list = list.Distinct().ToList();
            if (list.Any() && Next != null)
            {
                return Next.Select(list).Distinct().ToList();
            }
            return list;
        }

        public List<HtmlElement> Select(HtmlElementCollection elements)
        {
            var list = new List<HtmlElement>();
            foreach (HtmlElement item in elements)
            {
                list.Add(item);
            }
            return this.Select(list);
        }

        /// <summary>
        /// 直接下级匹配结果
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public List<HtmlElement> SelectMatchedDirectSubElement(HtmlElement element)
        {
            var list = new List<HtmlElement>();
            foreach (HtmlElement directSub in element.Children)
            {
                if (this.Match(directSub))
                {
                    list.Add(directSub);
                }
            }
            return list;
        }

        /// <summary>
        /// 任一下级匹配结果
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public List<HtmlElement> SelectMatchedAnySubElement(HtmlElement element)
        {
            var list = new List<HtmlElement>();
            if (this.Match(element))
                list.Add(element);
            foreach (HtmlElement child in element.Children)
            {
                list.AddRange(SelectMatchedAnySubElement(child));
            }
            return list;
        }

        public HtmlElement SelectMatchedSameLevel(HtmlElement element)
        {
            if (this.Match(element))
            {
                return element;
            }
            return null;
        }

        public abstract bool Match(HtmlElement element);

        public void SetNext(HtmlElementSelector selector, MatchMode matchModel = MatchMode.AnyChildLevel)
        {
            if (this.Next != null)
            {
                this.Next.SetNext(selector, matchModel);
            }
            else
            {
                this.Next = selector;
                this.Next.MatchMode = matchModel;
            }
        }

        public MatchMode MatchMode { get; set; }
    }

    /// <summary>
    /// 匹配模式
    /// </summary>
    public enum MatchMode
    {
        /// <summary>
        /// 匹配所有子集
        /// </summary>
        AnyChildLevel,
        /// <summary>
        /// 匹配同级
        /// </summary>
        SameLevel,
        /// <summary>
        /// 匹配直接下级
        /// </summary>
        DirectChildLevel,
    }

    public class HtmlElementSelectorFactory
    {
        HtmlElementSelector RootSelector { get; set; }
        public HtmlElementSelector GetSelector(string selector)
        {
            var splits = selector.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            splits = splits.Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
            foreach (var item in splits)
            {
                if (RootSelector == null)
                {
                    RootSelector = GetSelectorInternal(item);
                }
                else
                {
                    RootSelector.SetNext(GetSelectorInternal(item));
                }
            }
            return RootSelector;
        }

        public HtmlElementSelector GetSelectorInternal(string item)
        {
            if (item.IndexOf('.') > 0)
            {
                //table.tb-cls-1
                var dotIndex = item.IndexOf('.');
                var left = item.Substring(0, dotIndex);
                var right = item.Substring(dotIndex, item.Length - dotIndex);
                var next = GetSelectorInternal(left);
                next.SetNext(GetSelector(right), MatchMode.SameLevel);
                return next;
            }
            if (item.IndexOf('#') > 0)
            {
                //table#tb-id-1
                var dotIndex = item.IndexOf('.');
                var left = item.Substring(0, dotIndex);
                var right = item.Substring(dotIndex + 1, item.Length - dotIndex);
                var next = GetSelectorInternal(left);
                next.SetNext(GetSelector(right));
                return next;
            }
            if (item.IndexOf(">") != -1)
            {
                //table>tr
                var subItems = item.Split('>').Select(x => x.Trim()).ToList();
                var next = this.GetSelectorInternal(subItems[0]);
                for (int i = 1; i < subItems.Count; i++)
                {
                    next.SetNext(this.GetSelectorInternal(subItems[i]), MatchMode.DirectChildLevel);
                }
                return next;
            }

            //table[name=tb2]
            var startIndex = item.IndexOf("[");
            var endIndex = item.IndexOf("]");
            if (startIndex != -1 && endIndex != -1)
            {
                var left = item.Split('[')[0];
                var right = item.Substring(startIndex, endIndex - startIndex + 1);
                if (startIndex > 0)
                {
                    var next = GetSelectorInternal(left);
                    next.SetNext(GetSelector(right));
                    return next;
                }

                //[attrName=attrValue]
                var attrNameValue = item.Substring(startIndex + 1, endIndex - startIndex - 1);
                if (attrNameValue.IndexOf("=") != -1)
                {
                    var nv = attrNameValue.Split('=');
                    if (nv.Length == 2)
                    {
                        return new HtmlElementAttributeSelector(item) { AttributeName = nv[0], AttributeValue = nv[1], Not = false, Operator = "=" };
                    }
                    else
                    {
                        return new HtmlElementAttributeExistSelector(item);
                    }
                }
            }
            //.class-item
            if (item.IndexOf(".") == 0)
            {
                var lastDotIndex = item.LastIndexOf(".");
                if (lastDotIndex == 0)
                {
                    return new HtmlClassAttributeSelector(item);
                }
                else
                {
                    var left = item.Substring(0, lastDotIndex);
                    var next = GetSelectorInternal(left);
                    var right = item.Substring(lastDotIndex, item.Length - lastDotIndex);
                    next.SetNext(GetSelector(right), MatchMode.SameLevel);
                    return next;
                }
            }
            //#id-item
            else if (item.IndexOf("#") == 0)
            {
                return new HtmlIdAttributeSelector(item);
            }
            else
            {
                //div
                return new HtmlTagAttributeSelector(item);
            }
            throw new ArgumentException("选择器语法错误", "selector");
        }
    }
}

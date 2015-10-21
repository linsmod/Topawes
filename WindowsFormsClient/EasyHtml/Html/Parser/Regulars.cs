using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nx.EasyHtml.Html.Parser
{
    /// <summary>
    /// 用于分析 HTML DOM 结构的正则表达式列表
    /// </summary>
    public class Regulars
    {
        private static Regulars _instance;
        public static Regulars Instance
        {
            get
            {
                return _instance ?? (_instance = new Regulars());
            }
        }
        public Regex TagName = new Regex("^" + Regulars.tagNamePattern + "$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
        public Regex AttributeName = new Regex("^" + Regulars.attributeNamePattern + "$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
        public Regex Attribute = new Regex(Regulars.attributePattern, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

        public Regex BeginTag = new Regex("^" + Regulars.beginTagPattern + "$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
        public Regex EndTag = new Regex("^" + Regulars.endTagPattern + "$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
        public Regex CommentTag = new Regex("^" + Regulars.commentPattern + "$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
        public Regex SpecialTag = new Regex("^" + Regulars.specialTagPattern + "$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
        public Regex HtmlTag = new Regex(@"\G(?:" + Regulars.tagPattern + ")", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

        /// <summary>用于匹配 HTML 元素标签名的正则表达式</summary>
        private static readonly string tagNamePattern = @"(?<tagName>[A-Za-z][A-Za-z0-9\-_:\.]*)";


        /// <summary>用于匹配一般属性值的正则表达式</summary>
        private static readonly string normalAttributeValuePattern = @"(?<attrValue>([^'""\s][^\s]*)?(?=\s|$))";
        /// <summary>用于匹配用双引号包裹的属性值的正则表达式</summary>
        private static readonly string sqouteAttributeValuePattern = @"('(?<attrValue>[^']*)')";
        /// <summary>用于匹配用单引号包裹的属性值的正则表达式</summary>
        private static readonly string dquoteAttributeValuePattern = @"(""(?<attrValue>[^""]*)"")";

        /// <summary>用于匹配用属性值的正则表达式</summary>
        private static readonly string attributeValuePattern = @"((\s*=\s*(#squote|#dquote|#normal))|(?=\s|$))".Replace("#squote", sqouteAttributeValuePattern).Replace("#dquote", dquoteAttributeValuePattern).Replace("#normal", normalAttributeValuePattern);

        /// <summary>用于匹配用属性名称的的正则表达式</summary>
        private static readonly string attributeNamePattern = @"(?<attrName>[\w:_\-]+)";

        /// <summary>用于匹配用属性表达式的的正则表达式</summary>
        private static readonly string attributePattern = @"(\G|\s)(?<attribute>#attrName#attrValue)".Replace("#attrName", attributeNamePattern).Replace("#attrValue", attributeValuePattern);

        /// <summary>用于匹配用开始标签的正则表达式</summary>
        private static readonly string beginTagPattern = @"<#tagName(?<attributes>([^=]|(?>=\w*'[^']*')|(?>=\w*""[^""]*"")|=)*?)(?<selfClosed>/)?>".Replace("#tagName", tagNamePattern).Replace("#attribute", attributePattern);

        /// <summary>用于匹配用结束标签的正则表达式</summary>
        private static readonly string endTagPattern = @"</(#tagName)?[^>]*>".Replace("#tagName", tagNamePattern);

        /// <summary>用于匹配用注释标签的正则表达式</summary>
        private static readonly string commentPattern = @"<!--(?<commentText>(.|\n)*?)-->";

        /// <summary>用于匹配用声明标签的正则表达式</summary>
        private static readonly string doctypeDeclarationPattern = @"(<!(?i:DOCTYPE)\s+(?<declaration>(.|\n)*?)>)";

        /// <summary>用于匹配用特殊标签的正则表达式</summary>
        private static readonly string specialTagPattern = @"(<\?(?<specialText>(.|\n)*?)\?>)|(<\%(?<specialText>(.|\n)*?)\%>)|(<\#(?<specialText>(.|\n)*?)\#>)|(<\$(?<specialText>(.|\n)*?)\$>)";

        /// <summary>用于匹配用任意标签的正则表达式</summary>
        private static readonly string tagPattern = string.Format(@"(?<beginTag>{0})|(?<endTag>{1})|(?<comment>{2})|(?<special>{3})|(?<doctype>{4})", Regulars.beginTagPattern, Regulars.endTagPattern, Regulars.commentPattern, Regulars.specialTagPattern, Regulars.doctypeDeclarationPattern);

    }
}

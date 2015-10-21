using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nx.EasyHtml.Html
{

  /// <summary>
  /// 定义 HTML 文本节点
  /// </summary>
  public interface IHtmlTextNode : IHtmlNode
  {

    /// <summary>
    /// HTML 文本
    /// </summary>
    string HtmlText
    {
      get;
    }

  }
}

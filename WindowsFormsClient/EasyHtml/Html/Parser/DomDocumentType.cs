using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nx.EasyHtml.Html.Parser
{

  /// <summary>
  /// 表示一个文档类型声明
  /// </summary>
  public class DomDocumentType : DomNode, IHtmlSpecial
  {

    private const string DocumentTypePattern = @"\<!DOCTYPE .+?\>";


    private string _declares;

    internal DomDocumentType( string declares )
    {
      _declares = declares;
    }

    /// <summary>
    /// 对象名称
    /// </summary>
    protected override string ObjectName
    {
      get { return "DocumentType Declaration"; }
    }
  }
}

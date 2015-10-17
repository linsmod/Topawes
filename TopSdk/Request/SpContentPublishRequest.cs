using System;
using System.Collections.Generic;
using Top.Api.Response;
using Top.Api.Util;

namespace Top.Api.Request
{
    /// <summary>
    /// TOP API: taobao.sp.content.publish
    /// </summary>
    public class SpContentPublishRequest : ITopRequest<SpContentPublishResponse>
    {
        /// <summary>
        /// 表示为内容类型，包括三种选项： 1(宝贝);2(图片);3(心得)
        /// </summary>
        public string SchemaName { get; set; }

        /// <summary>
        /// 站长Key
        /// </summary>
        public string SiteKey { get; set; }

        /// <summary>
        /// 内容的自定义标签，数值为文本内容，多个标签以逗号“,”分割。  主要用于细化内容的分类（譬如小清新，棉质、雪纺等），标签名称的长度限制为[0,6] (单位是字符，不区分中英文)，标签名称中不能包含非法内容，且一个内容关联的标签数目不能超过6个
        /// </summary>
        public string Tags { get; set; }

        /// <summary>
        /// 内容具体的信息，用json格式描述，kv对的方式:  className(String，必填)：内容的自定义分类，数值为文本内容，主要用于区分内容的分类（譬如连衣裙、T恤、阿迪达斯等），分类名称的长度限制为(0,5] (单位是字符，不区分中英文)，分类名称中不能包含非法内容，且一个站点下所拥有的总自定义分类数量不能超过16个；    detailUrl(String，必填)：内容的detail页面地址，数值为html链接，主要用于展现内容的detail信息的，此数值必须存在，它是U站首页或淘宝官网搜索到内容之后用户点击进入的跳转页面。（如果站点没有单个内容的detail页面，也可以直接填写站点首页）；    items(String，宝贝必填)： 内容关联的商品列表，数值为商品的detail链接地址，多个宝贝以逗号“,”分割。此参数只有在type = 1 || type = 2（即内容类型为宝贝或图片）的时候才有效，宝贝只能并且必须关联一个商品；图片可以关联0~5个商品；    picUrl (String，图片必填): 内容关联的图片地址，数值为图片的html链接地址，多个图片以逗号“,”分割。此参数只有在type = 2（即内容类型为图片）的时候才有效，且关联的图片数量范围为[1,10]。图片地址必须匹配正则表达式:http://(img01|img02|img03|img04|img1|img2|img3|img4)\.(taobaocdn|tbcdn)\.(com|net|cn).*；    title(String，心得必填): 内容标题，数值为文本内容，此参数只有在type = 3（即内容类型为心得）的时候才有效，且标题的长度限制为(0,30](单位是字符，不区分中英文)，标题中不能含有非法内容，不能含有恶意脚本。    comments (String，选填)： 内容的推荐理由，数值为文本内容，此参数只有在type =1 || type =2 （即内容类型为宝贝或图片）的时候才有效，且推荐理由的长度限制为[0,140](单位是字符，不区分中英文)，推荐理由中不能含有非法内容，不能含有恶意脚本    content(String，心得必填)：内容的心得，数值为文本内容（html形式），此参数只有在type = 3（即内容类型为心得）的时候才有效，且心得长度限制为[100,20000] (单位是字符，不区分中英文), 心得中不能有外链，不能有恶意脚本；心得中包含的商品链接系统自自动提取并保存起来；  content(String，心得必填)：内容的心得，数值为文本内容（html形式），此参数只有在type = 3（即内容类型为心得）的时候才有效，且心得长度限制为[100,20000] (单位是字符，不区分中英文), 心得中不能有外链，不能有恶意脚本；心得中包含的商品链接系统自自动提取并保存起来
        /// </summary>
        public string Value { get; set; }

        private IDictionary<string, string> otherParameters;

        #region ITopRequest Members

        public string GetApiName()
        {
            return "taobao.sp.content.publish";
        }

        public IDictionary<string, string> GetParameters()
        {
            TopDictionary parameters = new TopDictionary();
            parameters.Add("schema_name", this.SchemaName);
            parameters.Add("site_key", this.SiteKey);
            parameters.Add("tags", this.Tags);
            parameters.Add("value", this.Value);
            parameters.AddAll(this.otherParameters);
            return parameters;
        }

        public void Validate()
        {
            RequestValidator.ValidateRequired("schema_name", this.SchemaName);
            RequestValidator.ValidateRequired("site_key", this.SiteKey);
            RequestValidator.ValidateMaxLength("site_key", this.SiteKey, 32);
            RequestValidator.ValidateRequired("value", this.Value);
        }

        #endregion

        public void AddOtherParameter(string key, string value)
        {
            if (this.otherParameters == null)
            {
                this.otherParameters = new TopDictionary();
            }
            this.otherParameters.Add(key, value);
        }
    }
}

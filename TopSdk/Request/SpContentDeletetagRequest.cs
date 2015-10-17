using System;
using System.Collections.Generic;
using Top.Api.Response;
using Top.Api.Util;

namespace Top.Api.Request
{
    /// <summary>
    /// TOP API: taobao.sp.content.deletetag
    /// </summary>
    public class SpContentDeletetagRequest : ITopRequest<SpContentDeletetagResponse>
    {
        /// <summary>
        /// 站长Key
        /// </summary>
        public string SiteKey { get; set; }

        /// <summary>
        /// 标签名称
        /// </summary>
        public string Tagname { get; set; }

        private IDictionary<string, string> otherParameters;

        #region ITopRequest Members

        public string GetApiName()
        {
            return "taobao.sp.content.deletetag";
        }

        public IDictionary<string, string> GetParameters()
        {
            TopDictionary parameters = new TopDictionary();
            parameters.Add("site_key", this.SiteKey);
            parameters.Add("tagname", this.Tagname);
            parameters.AddAll(this.otherParameters);
            return parameters;
        }

        public void Validate()
        {
            RequestValidator.ValidateRequired("site_key", this.SiteKey);
            RequestValidator.ValidateMaxLength("site_key", this.SiteKey, 32);
            RequestValidator.ValidateRequired("tagname", this.Tagname);
            RequestValidator.ValidateMaxLength("tagname", this.Tagname, 6);
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

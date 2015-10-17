using System;
using System.Xml.Serialization;

namespace Top.Api.Response
{
    /// <summary>
    /// AlibabaMemberUserIdTomemberIdConvertResponse.
    /// </summary>
    public class AlibabaMemberUserIdTomemberIdConvertResponse : TopResponse
    {
        /// <summary>
        /// memberId
        /// </summary>
        [XmlElement("member_id")]
        public string MemberId { get; set; }
    }
}

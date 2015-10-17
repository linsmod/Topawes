using System;
using System.Collections.Generic;
using Top.Api.Response;
using Top.Api.Util;

namespace Top.Api.Request
{
    /// <summary>
    /// TOP API: taobao.topats.promotion.coupondetail.get
    /// </summary>
    public class TopatsPromotionCoupondetailGetRequest : ITopRequest<TopatsPromotionCoupondetailGetResponse>
    {
        /// <summary>
        /// 优惠券ID
        /// </summary>
        public Nullable<long> CouponId { get; set; }

        /// <summary>
        /// 优惠券截止的结束时间。其中end_time>start_time并且end_time-start_time<=15天。
        /// </summary>
        public Nullable<DateTime> EndTime { get; set; }

        /// <summary>
        /// 优惠券截止的开始时间。
        /// </summary>
        public Nullable<DateTime> StartTime { get; set; }

        /// <summary>
        /// 优惠券使用情况。可选值：unused：未使用；using：使用中；used：已使用。
        /// </summary>
        public string State { get; set; }

        private IDictionary<string, string> otherParameters;

        #region ITopRequest Members

        public string GetApiName()
        {
            return "taobao.topats.promotion.coupondetail.get";
        }

        public IDictionary<string, string> GetParameters()
        {
            TopDictionary parameters = new TopDictionary();
            parameters.Add("coupon_id", this.CouponId);
            parameters.Add("end_time", this.EndTime);
            parameters.Add("start_time", this.StartTime);
            parameters.Add("state", this.State);
            parameters.AddAll(this.otherParameters);
            return parameters;
        }

        public void Validate()
        {
            RequestValidator.ValidateRequired("end_time", this.EndTime);
            RequestValidator.ValidateRequired("start_time", this.StartTime);
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

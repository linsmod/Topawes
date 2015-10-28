using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsClient.Models
{
    public class TaoJsonpResult
    {
        public int status { get; set; }
        public string msg { get; set; }
    }
    //{"promotedCount":0,"failedSaleCount":-1,"promotedMax":15,"onCount":2,"status":200,"offCount":0,"priceChangedCount":0,"alipayBalance":"500.00"}
    public class TaoInfo : TaoJsonpResult
    {
        public int promotedCount { get; set; }
        public int failedSaleCount { get; set; }
        public int promotedMax { get; set; }
        public int onCount { get; set; }
        public int offCount { get; set; }
        public int priceChangedCount { get; set; }
        public double alipayBalance { get; set; }
    }

    public class DurexParamResult : TaoJsonpResult
    {
        public int code;
        public DurexParam ret { get; set; }
    }

    public class DurexParam
    {
        public string param { get; set; }
    }

    public class TaoUplistResult
    {
        public Dictionary<string, string> errmap { get; set; }
        public int failNum;
        public int succNum;
        public bool status { get; set; }
        public string msg { get; set; }
    }
}

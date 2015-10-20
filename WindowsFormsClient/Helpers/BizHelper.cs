using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinFormsClient.Models;
using WinFormsClient.Extensions;
using WinFormsClient.Helpers;
using TopModel;
using Moonlight.WindowsForms.Controls;

namespace WinFormsClient
{
    public class BizHelper
    {


        /// <summary>
        /// 查询供应商信息(AJAX)
        /// </summary>
        /// <param name="wb"></param>
        /// <param name="spu"></param>
        /// <returns></returns>
        public static async Task<SuplierInfo> supplierInfo(WBHelper wbHelper, string spu)
        {
            var url = "http://chongzhi.taobao.com/item.do?spu={0}&action=edit&method=supplierInfo&_=" + DateTime.Now.Ticks;
            url = string.Format(url, spu);
            var content = await wbHelper.WB.ExecuteTriggerJSONP(url);
            var suplierInfo = JsonConvert.DeserializeObject<SuplierInfo>(content);
            if (suplierInfo.profitData != null && suplierInfo.profitData.Any())
            {
                //查到供应商
                return suplierInfo;
            }
            else
            {
                //无供应商,平台默认会自动关单
                return null;
            }
        }
        /// <summary>
        /// 设置供应商(非AJAX)
        /// </summary>
        /// <param name="wb"></param>
        /// <param name="sup"></param>
        /// <param name="spu"></param>
        /// <param name="profitMin"></param>
        /// <param name="profitMax"></param>
        /// <param name="price">一口价</param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public static async Task<TaoJsonpResult> supplierSave(WBHelper helper, string sup, string spu, string profit,string price, long itemId, string tbcpCrumbs)
        {
            //profitMode=0 保证我赚钱
            //profitMode=2 自定义
            var url = "http://chongzhi.taobao.com/item.do?method=supplierSave&sup={0}&mode=2&spu={1}&itemId={2}&profitMode=2&profitMin={3}&profitMax={4}&price={5}&tbcpCrumbs={6}";
            url = string.Format(url, sup, spu, itemId, profit, profit, price, tbcpCrumbs);
            var content = await helper.SynchronousLoadString(url);
            return JsonConvert.DeserializeObject<TaoJsonpResult>(content);
        }
    }
}

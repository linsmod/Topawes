using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;
using TopModel.Models;

namespace PushServer.MessageHubs
{
    [Authorize]
    public class TradeRateMessageHub : TopawesHub
    {
        public ApiResult TrateRate(long itemId)
        {
            ITopClient client = GetTopClient();
            TraderateAddRequest req = new TraderateAddRequest();
            req.Tid = itemId;
            //req.Oid = 1234L;
            req.Result = "good";
            req.Role = "seller";
            //req.Content = "好评！";
            req.Anony = false;
            TraderateAddResponse rsp = client.Execute(req, AccessToken);
            return new ApiResult(!rsp.IsError, rsp.ErrMsg + " " + rsp.SubErrMsg);
        }
    }
}

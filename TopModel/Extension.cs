using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Top.Api;
using TopModel.Models;

namespace TopModel
{
    public static class Extension
    {
        public static ApiResult AsApiResult(this TopResponse response)
        {
            if (response.IsError)
            {
                return new ApiResult(false, response.ErrMsg + " " + response.SubErrMsg);
            }
            return new ApiResult(true, "");
        }
        public static ApiResult<T> AsApiResult<T>(this TopResponse response, T data)
        {
            if (response.IsError)
            {
                return new ApiResult<T>(false, response.ErrMsg + " " + response.SubErrMsg);
            }
            return new ApiResult<T>(true, "") { Data = data };
        }

        public static ApiPagedResult<T> AsApiPagedResult<T>(this TopResponse response, T dataList, bool hasMore) where T : new()
        {
            if (response.IsError)
            {
                return new ApiPagedResult<T>(false, response.ErrMsg + " " + response.SubErrMsg);
            }
            return new ApiPagedResult<T>(true, "") { Data = dataList, HasMore = hasMore };
        }
    }
}

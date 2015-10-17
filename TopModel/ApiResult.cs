using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopModel.Models
{
    public class ApiResult
    {
        public ApiResult(bool su, string msg)
        {
            this.Success = su;
            this.Message = msg;
        }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class ApiResult<T> : ApiResult
    {
        public ApiResult(bool v1, string v2) : base(v1, v2) { }
        public T Data { get; set; }
    }

    public class ApiPagedResult<T> : ApiResult
        where T : new()
    {
        public ApiPagedResult() : this(true, "")
        {

        }
        public ApiPagedResult(bool v1, string v2) : base(v1, v2)
        {
            this.HasMore = true;
            this.Data = new T();
        }
        public T Data { get; set; }

        public bool HasMore { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.Helpers.Response
{
    public class ApiSuccessResponse<T> : ApiResponse<T> where T : class
    {
        public ApiSuccessResponse(int statusCode, string message = null, T data = null) : base(statusCode, message, data)
        {
        }
    }
}

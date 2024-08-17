using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.Helpers.Response
{
    public class ApiNotFoundResponse<T>:ApiResponse<T> where T : class
    {
        public ApiNotFoundResponse(string message): base(404, message)
        {
        }
        public ApiNotFoundResponse(string message = null, T data = null) : base(404, message, data)
        {
        }
    }
}

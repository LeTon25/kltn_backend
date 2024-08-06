using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.Helpers.Response
{
    public class ApiNotFoundResponse:ApiResponse
    {
        public ApiNotFoundResponse(string message): base(404, message)
        {
        }
    }
}

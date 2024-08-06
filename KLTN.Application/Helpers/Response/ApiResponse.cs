using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.Helpers.Response
{
    public class ApiResponse
    {
        public int StatusCode { get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; }

        public ApiResponse(int statusCode, string message = null)
        {
            StatusCode = statusCode;
            Message = message ?? GetDefaultMessageForStatusCode(statusCode);
        }
        private static string GetDefaultMessageForStatusCode(int statusCode)
        {
            switch (statusCode)
            {
                case 404:
                    return "Tài nguyên không tồn tại";

                case 500:
                    return "Có lỗi xảy ra";

                default:
                    return null;
            }
        }
    }
}

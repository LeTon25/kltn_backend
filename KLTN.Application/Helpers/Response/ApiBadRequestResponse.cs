using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.Helpers.Response
{
    public class ApiBadRequestResponse : ApiResponse
    {
        public IEnumerable<string> Errors { get; }
        public ApiBadRequestResponse(IdentityResult identityResult)
           : base(400)
        {
            Errors = identityResult.Errors
                .Select(x => x.Code + " - " + x.Description).ToArray();
        }
        public ApiBadRequestResponse(string message)
           : base(400, message)
        {
        }
    }
}

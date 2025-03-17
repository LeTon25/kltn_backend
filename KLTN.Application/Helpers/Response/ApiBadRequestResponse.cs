using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
namespace KLTN.Application.Helpers.Response
{
    public class ApiBadRequestResponse<T> : ApiResponse<T> where T : class
    {
        public ApiBadRequestResponse(ModelStateDictionary modelState) : base(400) 
        {
            if (modelState.IsValid)
            {
                throw new ArgumentException("ModelState must be invalid", nameof(modelState));
            }

            var Errors = string.Join(",",modelState.SelectMany(x => x.Value.Errors)
                .Select(x => x.ErrorMessage).ToArray());
            if (!string.IsNullOrEmpty(this.Message)) 
                 this.Message += ", " + Errors;
            else this.Message = Errors;
        }
        public ApiBadRequestResponse(IdentityResult identityResult)
           : base(400)
        {
            var Errors =string.Join(",",identityResult.Errors
                .Select(x => x.Code + " - " + x.Description).ToArray());
            if (!string.IsNullOrEmpty(this.Message))
                this.Message += ", " + Errors;
            else this.Message = Errors;
        }
        public ApiBadRequestResponse(string message)
           : base(400, message)
        {
        }
    }
}

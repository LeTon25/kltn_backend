using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.Helpers.Filter
{
    public class RoleRequirementFilter : IAuthorizationFilter
    {
        private readonly string[] RequireRoles;
        public RoleRequirementFilter(string[] requireRoles) {
            this.RequireRoles = requireRoles;   
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userRoleClaims = context.HttpContext.User.Claims
                                        .Where(c=>c.Type == ClaimTypes.Role )
                                        .Select(c=>c.Value) 
                                        .ToList();  
            if(userRoleClaims != null && userRoleClaims.Count() >0 )
            {
                foreach(var item in this.RequireRoles)
                {
                    if(userRoleClaims.Contains(item))
                    {
                        return;
                    }    
                }    
                context.Result = new ForbidResult();
            }
            context.Result = new ForbidResult();
        }
    }
}
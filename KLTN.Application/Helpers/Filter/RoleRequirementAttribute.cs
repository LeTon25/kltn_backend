using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Application.Helpers.Filter
{
    public class RoleRequirementAttribute : TypeFilterAttribute
    {
        public RoleRequirementAttribute(string[] requireRoles):base(typeof(RoleRequirementFilter))
        {
            Arguments  = new object[] { requireRoles };
        }
    }
}

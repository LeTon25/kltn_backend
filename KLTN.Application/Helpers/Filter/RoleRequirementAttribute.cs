using Microsoft.AspNetCore.Mvc;


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

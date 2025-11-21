using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PROG_MVC_POE_P2.Filters;


[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _roles;
    public AuthorizeRoleAttribute(params string[] roles) => _roles = roles ?? Array.Empty<string>();

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var session = context.HttpContext.Session;
        var role = session.GetString("UserRole");
        if (string.IsNullOrEmpty(role) || (_roles.Length > 0 && !_roles.Contains(role)))
        {
            // redirect to login
            context.Result = new RedirectToActionResult("Login", "Account", new { area = "" });
        }
    }
}
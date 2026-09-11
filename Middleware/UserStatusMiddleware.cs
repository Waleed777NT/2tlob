using Microsoft.AspNetCore.Identity;
using _2tlob.Models;
using _2tlob.Enum;

namespace _2tlob.Middleware
{
    public class UserStatusMiddleware
    {
        private readonly RequestDelegate _next;

        public UserStatusMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user != null && user.Status == UserStatus.Suspended)
                {
                    await signInManager.SignOutAsync();
                    context.Response.Redirect("/Account/AccessDenied?reason=suspended");
                    return;
                }
            }

            await _next(context);
        }
    }
}

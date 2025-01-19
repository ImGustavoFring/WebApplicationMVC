using System.Security.Claims;
using WebApplicationMVC.Services;

namespace WebApplicationMVC.Middlewares
{
    public class VisitTrackerMiddleware
    {
        private readonly RequestDelegate _next;

        public VisitTrackerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, VisitTrackerService visitTracker)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userIdClaim != null && int.TryParse(userIdClaim, out int userId))
                {
                    visitTracker.RecordVisit(userId);
                }
            }

            await _next(context);
        }
    }
}

namespace _2tlob.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Security headers
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
            
            // Content-Security-Policy
            context.Response.Headers.Append("Content-Security-Policy",
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
                "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://fonts.googleapis.com; " +
                "font-src 'self' https://fonts.gstatic.com https://cdn.jsdelivr.net; " +
                "img-src 'self' data: https:; " +
                "connect-src 'self'; " +
                "frame-ancestors 'self';");

            await _next(context);
        }
    }
}

using Infrastructure.Service;
using System.Security.Claims;

namespace EventManagerAPI.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public string GetCurrentUser()
        {
            ClaimsPrincipal user = _httpContextAccessor.HttpContext.User;

            string username = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return username;
        }
    }
}

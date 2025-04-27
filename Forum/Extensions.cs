using System.Security.Claims;
using System;

namespace Forum
{
    public static class Extensions
    {
        // Расширение для удобного получения ID пользователя
        public static int? GetUserId(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                return null;
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }

            return null; 
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Extensions
{
    public static class ClaimsPrincipleExtensions
    {
        public static string GetUsername(this ClaimsPrincipal user)
        {
            // NameIdentifier = JwtRegisteredClaimNames.NameId
            // Name = JwtRegisteredClaimNames.UniqueName 
            return user.FindFirst(ClaimTypes.Name)?.Value;
        }

        public static int GetUserId(this ClaimsPrincipal user)
        {
            // NameIdentifier = JwtRegisteredClaimNames.NameId
            // Name = JwtRegisteredClaimNames.UniqueName 
            return int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

namespace CetCources.Extensions
{
    public static class IdentityExtensions
    {
        public static string FullName(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("FullName");
            return (claim != null) ? claim.Value : string.Empty;
        }

        public static string Comments(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("Comments");
            return (claim != null) ? claim.Value : string.Empty;
        }

        public static string TermsAccepted(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("TermsAccepted");
            return (claim != null) ? claim.Value : string.Empty;
        }
    }
}
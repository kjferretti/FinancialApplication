using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

namespace FinancialApplication.Helpers
{
    public static class Extensions
    {
        public static string GetHouseholdId(this IIdentity user)
        {
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)user;
            Claim HouseholdClaim = claimsIdentity.Claims.FirstOrDefault(c => c.Type == "HouseholdId");

            if (HouseholdClaim != null)
                return HouseholdClaim.Value;
            else
                return null;
        }

        public static bool IsInHousehold(this IIdentity user)
        {
            ClaimsIdentity claimsUser = (ClaimsIdentity)user;
            Claim householdId = claimsUser.Claims.FirstOrDefault(c => c.Type == "HouseholdId");
            return (householdId != null && !string.IsNullOrWhiteSpace(householdId.Value));
        }

        public static string GetFullName(this IIdentity user)
        {
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)user;
            Claim NameClaim = claimsIdentity.Claims.FirstOrDefault(c => c.Type == "FullName");

            if (NameClaim != null)
                return NameClaim.Value;
            else
                return null;
        }
    }
}
using FinancialApplication.Models.CodeFirst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinancialApplication.Models
{
    public class NeedHouseholdViewModel
    {
        // For new household creation
        public string Name { get; set; }
        public string OwnerId { get; set; }
        public string[] Invitees { get; set; }
        public string[] ExistingUsers { get; set; }

        // For joining existing household
        public int? HouseholdId { get; set; }
    }
    public class HouseholdIndexViewModel
    {
        public HouseholdIndexViewModel()
        {
            Members = new HashSet<ApplicationUser>();
            JoinRequests = new HashSet<JoinRequest>();
        }

        // For new household creation
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<ApplicationUser> Members { get; set; }
        public ICollection<JoinRequest> JoinRequests { get; set; }
        public string[] Invitees { get; set; }
        public string[] ExistingUsers { get; set; }
    }
}
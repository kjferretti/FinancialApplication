using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinancialApplication.Models.CodeFirst
{
    public class JoinRequest
    {
        // Key
        public int Id { get; set; }

        // Properties
        public bool Approved { get; set; }
        public DateTimeOffset DateRequested { get; set; }

        // Virtual Singles
        public string RequesterId { get; set; }
        public virtual ApplicationUser Requester { get; set; }
        public int HouseholdId { get; set; }
        public virtual Household Household { get; set; }

        //Virtual Manys
    }
}
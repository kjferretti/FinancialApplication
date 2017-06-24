using System;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FinancialApplication.Models.CodeFirst
{
    public class Invitation
    {
        // Key
        public int Id { get; set; }

        // Properties
        [Display(Description = "Email")]
        public string UserEmail { get; set; }

        // Virtual Singles
        public int HouseholdId { get; set; }
        public virtual Household Household { get; set; }

        // Virtual Manys

    }
}
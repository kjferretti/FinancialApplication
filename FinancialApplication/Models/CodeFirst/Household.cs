using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FinancialApplication.Models.CodeFirst
{
    public class Household
    {
        public Household()
        {
            Members = new HashSet<ApplicationUser>();
            Accounts = new HashSet<Account>();
            Invitations = new HashSet<Invitation>();
            JoinRequests = new HashSet<JoinRequest>();
            BudgetCategories = new HashSet<BudgetCategory>();
            Budgets = new HashSet<Budget>();
        }

        // Key
        public int Id { get; set; }

        // Properties
        [Required]
        public string Name { get; set; }
        public string OwnerId { get; set; }

        // Virtual Singles
        

        // Virtual Manys
        public virtual ICollection<ApplicationUser> Members { get; set; }
        public virtual ICollection<Account> Accounts { get; set; }
        public virtual ICollection<Invitation> Invitations { get; set; }
        public virtual ICollection<JoinRequest> JoinRequests { get; set; }
        public virtual ICollection<BudgetCategory> BudgetCategories { get; set; }
        public virtual ICollection<Budget> Budgets { get; set; }
    }
}
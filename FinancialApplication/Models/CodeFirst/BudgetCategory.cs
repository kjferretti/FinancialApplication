using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinancialApplication.Models.CodeFirst
{
    public class BudgetCategory
    {
        // Key
        public int Id { get; set; }

        // Properties
        public string Name { get; set; }

        // Virtual Singles
        public int? HouseholdId { get; set; }
        public virtual Household Household { get; set; }

        // Virtual Manys
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
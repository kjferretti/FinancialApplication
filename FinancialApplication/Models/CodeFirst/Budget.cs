using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FinancialApplication.Models.CodeFirst
{
    public class Budget
    {
        // Key
        public int Id { get; set; }

        // Properties
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        [Range(0.01, 1000000000.00, ErrorMessage = "Amount must be a number between 0.01 amd 1000000000.00")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        // Virtual Singles
        public int? HouseholdId { get; set; }
        public virtual Household Household { get; set; }
        public int BudgetCategoryId { get; set; }
        public virtual BudgetCategory BudgetCategory { get; set; }

        //// Virtual Manys
    }
}
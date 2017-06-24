using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FinancialApplication.Models.CodeFirst
{
    public class Transaction
    {
        // Key
        public int Id { get; set; }

        // Properties
        [Required]
        public string Description { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Currency)]
        [Range(0.01, 1000000000.00, ErrorMessage = "Amount must be a number between 0.01 amd 1000000000.00")]
        public decimal Amount { get; set; }
        public bool ForReconciled { get; set; }
        public int? ReconciledTransactionId { get; set; }
        public bool Expense { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        [DataType(DataType.Date)]
        public DateTimeOffset Date { get; set; }

        // Virtual Singles
        public int AccountId { get; set; }
        public virtual Account Account { get; set; }
        [Display(Name = "Category")]
        public int BudgetCategoryId { get; set; }
        public virtual BudgetCategory BudgetCategory { get; set; }
        public string MadeById { get; set; }
        public virtual ApplicationUser MadeBy { get; set; }

        // Virtual Manys
    }
}
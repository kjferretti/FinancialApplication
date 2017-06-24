using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FinancialApplication.Models
{
    public class TransactionCreateViewModel
    {
        [Required]
        public string Description { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Currency)]
        [Range(0.01, 1000000000.00, ErrorMessage = "Amount must be a number between 0.01 amd 1000000000.00")]
        public decimal Amount { get; set; }
        public int AccountId { get; set; }
        public int BudgetCategoryId { get; set; }
        public string Type { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTimeOffset Date { get; set; }
    }
    public class TransactionReconcileViewModel
    {
        [Required]
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Currency)]
        [Range(0.01, 1000000000.00, ErrorMessage = "Amount must be a number between 0.01 amd 1000000000.00")]
        public decimal NewAmount { get; set; }
        public int ReconciledTransactionId { get; set; }
    }
    public class TransactionEditViewModel
    {
        public int Id { get; set; }
        [Required]
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public bool Expense { get; set; }
        public DateTimeOffset Date { get; set; }
        public int AccountId { get; set; }
        public int BudgetCategoryId { get; set; }
        public string MadeById { get; set; }
    }
}
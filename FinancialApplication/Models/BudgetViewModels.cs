using FinancialApplication.Models.CodeFirst;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FinancialApplication.Models
{
    public class BudgetCreateViewModel
    {
        [Required]
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Currency)]
        [Range(0.01, 1000000000.00, ErrorMessage = "Amount must be a number between 0.01 amd 1000000000.00")]
        public decimal InitialAmount { get; set; }
        [Required]
        public string Category { get; set; }
    }

    public class BudgetDetailsViewModel
    {
        public BudgetDetailsViewModel()
        {
            BudgetTransactions = new List<Transaction>();
        }
        public Budget Budget { get; set; }
        public ICollection<Transaction> BudgetTransactions { get; set; }
        public decimal CurrentAmount { get; set; }
    }

    public class BudgetIndexViewModel
    {
        public BudgetIndexViewModel()
        {
            Budgets = new List<Budget>();
        }
        public decimal[] CurrentAmounts { get; set; }
        public ICollection<Budget> Budgets { get; set; }
    }
}
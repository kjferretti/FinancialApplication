using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FinancialApplication.Models.CodeFirst
{
    public class Account
    {
        // Key
        public int Id { get; set; }

        // Properties
        [Required]
        public string Name { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Currency)]
        [Display(Name = "Current Balance")]
        [Range(0.01, 1000000000.00, ErrorMessage = "Amount must be a number between 0.01 amd 1000000000.00")]
        public decimal CurrentBalance { get; set; }
        [Display(Name = "Reconciled Balance")]
        [DataType(DataType.Currency)]
        public decimal ReconciledBalance { get; set; }

        // Virtual Singles
        [Display(Name = "Household")]
        public int HouseholdId { get; set; }
        public virtual Household Household { get; set; }

        // Virtual Manys
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
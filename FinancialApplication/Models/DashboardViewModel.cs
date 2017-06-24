using FinancialApplication.Models.CodeFirst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinancialApplication.Models
{
    public class DashboardViewModel
    {
        public DashboardViewModel()
        {
            Transactions = new List<Transaction>();
        }
        public string[] AreaXdata { get; set; }
        public int[] AreaYdata { get; set; }
        public string[] BarXdata { get; set; }
        public int[] BarYdata { get; set; }
        public ICollection<Transaction> Transactions { get; set; }
    }
}
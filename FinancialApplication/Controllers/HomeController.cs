using FinancialApplication.Helpers;
using FinancialApplication.Models;
using FinancialApplication.Models.CodeFirst;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FinancialApplication.Controllers
{
    [RequireHttps]
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            if (!User.Identity.IsInHousehold())
            {
                return RedirectToAction("NoHouseholdIndex");
            }
            // Dashboard
            var transactions = db.Transactions.ToList().Where(t => t.Account.HouseholdId == Convert.ToInt32(User.Identity.GetHouseholdId()));
            DashboardViewModel vm = new DashboardViewModel();
            vm.AreaXdata = new string[30];
            vm.AreaYdata = new int[30];
            for (int i = 1; i <= 30; i++)
            {
                vm.AreaXdata[i - 1] = Convert.ToString(i);
            }
            int k = 0;
            DateTimeOffset dt = new DateTimeOffset();
            foreach (var x in vm.AreaXdata)
            {
                dt = new DateTimeOffset(2017, 6, k+1, 0, 0, 0, TimeSpan.Zero);
                vm.AreaYdata[k] = transactions.Where(t => t.Date.ToString("MM/dd/yyyy") == dt.ToString("MM/dd/yyyy")).Select(t => t.Amount).Sum(m => Convert.ToInt32(m));
                k++;
            }

            Household household = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            vm.BarXdata = new string[household.Budgets.Count];
            vm.BarYdata = new int[household.Budgets.Count];
            var budgets = household.Budgets.ToList();
            transactions = new List<Transaction>();
            k = 0;
            foreach (var budget in budgets)
            {
                vm.BarXdata[k] = budget.BudgetCategory.Name;
                transactions = db.Transactions.Where(t => t.BudgetCategoryId == budget.BudgetCategoryId).ToList();
                vm.BarYdata[k] = transactions.Select(t => t.Amount).Sum(m => Convert.ToInt32(m));
                k++;
            }
            var accounts = household.Accounts.AsEnumerable();
            vm.Transactions = db.Transactions.AsEnumerable().Where(t => accounts.Select(a => a.Id).Contains(t.AccountId) && t.Date > DateTimeOffset.Now.AddDays(-7)).ToList();

            return View(vm);
        }

        // Limit this action to only users who don't belong to a household.
        [AuthorizeNotInHousehold]
        public ActionResult NoHouseholdIndex()
        {
            string currentUserId = User.Identity.GetUserId();

            ViewBag.Households = db.Households.ToList();

            NeedHouseholdViewModel vm = new NeedHouseholdViewModel();
            vm.ExistingUsers = new string[db.Users.Count()];
            int i = 0;
            foreach (ApplicationUser user in db.Users.ToList())
            {
                bool notOwnName = user.Id != currentUserId;
                if (notOwnName)
                {
                    vm.ExistingUsers[i] = user.FullName;
                    i++;
                }
            }

            ViewBag.CreateClasses = "tab-pane active";
            ViewBag.JoinClasses = "tab-pane";
            ViewBag.Create = "checked";
            ViewBag.Join = "";
            ViewBag.RequestSent = "";

            string tempRequest = TempData["requestSent"] as string;
            if (tempRequest == "sent")
            {
                ViewBag.CreateClasses = "tab-pane";
                ViewBag.JoinClasses = "tab-pane active";
                ViewBag.Create = "";
                ViewBag.Join = "checked";
                ViewBag.RequestSent = "Your request to join this household has been sent, when you are approved you will gain access to this household and a notification will be sent";
            }

            return View(vm);
        }

        [Authorize]
        public ActionResult Unauthorized()
        {
            return View();
        }

        [ChildActionOnly]
        public PartialViewResult UserPartial()
        {
            var currentUserId = User.Identity.GetUserId();
            ApplicationUser currentUser = db.Users.Find(currentUserId);
            return PartialView("~/Views/Shared/_UserPartial.cshtml", currentUser);
        }
    }
}
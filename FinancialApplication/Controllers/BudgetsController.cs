using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FinancialApplication.Models;
using FinancialApplication.Models.CodeFirst;
using FinancialApplication.Helpers;
using Microsoft.AspNet.Identity;

namespace FinancialApplication.Controllers
{
    [AuthorizeHousehold]
    public class BudgetsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Budgets
        public ActionResult Index()
        {
            var budgets = db.Budgets.Include(b => b.BudgetCategory).ToList();
            BudgetIndexViewModel vm = new BudgetIndexViewModel();
            vm.Budgets = budgets;
            IEnumerable<Transaction> transactions = new List<Transaction>();
            decimal transactionSum = 0;
            int i = 0;
            vm.CurrentAmounts = new decimal[budgets.Count];
            foreach (var budget in vm.Budgets)
            {
                transactions = db.Transactions.Where(t => t.BudgetCategoryId == budget.BudgetCategoryId).ToList();
                transactionSum = budget.Amount;
                foreach (var transaction in transactions)
                {
                    if (transaction.Expense)
                    {
                        transactionSum -= transaction.Amount;
                    }
                    else
                    {
                        transactionSum += transaction.Amount;
                    }
                }
                vm.CurrentAmounts[i] = transactionSum;
                i++;
            }
            Household household = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            ViewBag.HouseholdName = household.Name;
            string temp = TempData["noAccounts"] as string;
            if (temp == "noAccounts")
            {
                ViewBag.NoAccounts = "You need to create an account for this household to utilize transaction functionality";
            }
            return View(vm);
        }

        // GET: Budgets/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Budget budget = db.Budgets.Find(id);
            if (budget == null)
            {
                return HttpNotFound();
            }
            if (budget.HouseholdId != Convert.ToInt32(User.Identity.GetHouseholdId()))
            {
                return RedirectToAction("Unauthorized", "Home");
            }
            BudgetDetailsViewModel vm = new BudgetDetailsViewModel();
            vm.Budget = budget;
            vm.CurrentAmount = budget.Amount;
            vm.BudgetTransactions = db.Transactions.Where(t => budget.BudgetCategoryId == t.BudgetCategoryId).ToList();
            foreach (var transaction in vm.BudgetTransactions)
            {
                vm.CurrentAmount -= transaction.Amount;
            }
            return View(vm);
        }

        // GET: Budgets/Create
        public ActionResult Create()
        {
            Household household = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            ViewBag.HouseholdName = household.Name;
            return View();
        }

        // POST: Budgets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "InitialAmount,Category")] BudgetCreateViewModel model)
        {
            BudgetCategory category = new BudgetCategory();
            Household household = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            if (ModelState.IsValid && !household.BudgetCategories.Select(bc => bc.Name).Contains(model.Category))
            {
                category.Name = model.Category;
                category.HouseholdId = Convert.ToInt32(User.Identity.GetHouseholdId());
                db.BudgetCategories.Add(category);
                db.SaveChanges();

                Budget budget = new Budget();
                budget.Amount = model.InitialAmount;
                budget.HouseholdId = Convert.ToInt32(User.Identity.GetHouseholdId());
                budget.BudgetCategoryId = category.Id;
                db.Budgets.Add(budget);
                db.SaveChanges();
                return RedirectToAction("Details", new { id = budget.Id });
            }

            ViewBag.HouseholdName = household.Name;
            return View(model);
        }

        // GET: Transactions/Create
        public ActionResult CreateTransaction(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Budget budget = db.Budgets.Find(id);
            if (budget == null)
            {
                return HttpNotFound();
            }
            string currentUserId = User.Identity.GetUserId();
            if (!budget.Household.Members.Select(m => m.Id).Contains(currentUserId))
            {
                return RedirectToAction("Unauthorized", "Home");
            }
            Household household = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            if (!household.Accounts.Any())
            {
                TempData["noAccounts"] = "noAccounts";
                return RedirectToAction("Index");
            }
            ViewBag.BudgetName = budget.BudgetCategory.Name;
            ViewBag.BudgetCategoryId = budget.BudgetCategoryId;
            ViewBag.AccountId = new SelectList(db.Accounts, "Id", "Name");
            return View();
        }

        // POST: Transactions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateTransaction([Bind(Include = "Description,Amount,Type,AccountId,BudgetCategoryId,Date")] TransactionCreateViewModel vm)
        {
            Household userHousehold = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            if (ModelState.IsValid && (userHousehold.BudgetCategories.Select(bc => bc.Id).Contains(vm.BudgetCategoryId) || db.BudgetCategories.Find(vm.BudgetCategoryId).Name == "General") && userHousehold.Accounts.Select(a => a.Id).Contains(vm.AccountId))
            {
                Transaction transaction = new Transaction();
                transaction.Date = vm.Date;
                transaction.Description = vm.Description;
                transaction.Amount = vm.Amount;
                transaction.AccountId = vm.AccountId;
                transaction.BudgetCategoryId = vm.BudgetCategoryId;
                if (vm.Type == "+")
                {
                    transaction.Expense = false;
                }
                else
                {
                    transaction.Expense = true;
                }
                transaction.ForReconciled = false;
                Account account = db.Accounts.Find(transaction.AccountId);

                transaction.MadeById = User.Identity.GetUserId();
                db.Transactions.Add(transaction);
                db.SaveChanges();

                if (transaction.Expense)
                {
                    account.CurrentBalance = account.CurrentBalance - transaction.Amount;
                    account.ReconciledBalance -= transaction.Amount;
                }
                else
                {
                    account.CurrentBalance = account.CurrentBalance + transaction.Amount;
                    account.ReconciledBalance += transaction.Amount;
                }
                db.Entry(account).Property("CurrentBalance").IsModified = true;
                db.SaveChanges();

                db.Entry(account).Property("ReconciledBalance").IsModified = true;
                db.SaveChanges();

                Budget budget = userHousehold.Budgets.FirstOrDefault(b => b.BudgetCategoryId == vm.BudgetCategoryId);
                return RedirectToAction("Details", new { id = budget.Id });
            }

            if (vm.AccountId == 0 || !db.Accounts.ToList().Select(a => a.Id).Contains(vm.AccountId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account accountB = db.Accounts.Find(vm.AccountId);
            string currentUserId = User.Identity.GetUserId();
            if (!accountB.Household.Members.Select(m => m.Id).Contains(currentUserId))
            {
                return RedirectToAction("Unauthorized", "Home");
            }

            ViewBag.AccountName = accountB.Name;
            ViewBag.AccountId = vm.AccountId;
            ViewBag.BudgetCategoryId = new SelectList(db.BudgetCategories, "Id", "Name");
            return View();
        }

        // GET: Budgets/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Budget budget = db.Budgets.Find(id);
            if (budget == null)
            {
                return HttpNotFound();
            }
            if (budget.HouseholdId != Convert.ToInt32(User.Identity.GetHouseholdId()))
            {
                return RedirectToAction("Unauthorized", "Home");
            }
            return View(budget);
        }

        // POST: Budgets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Amount,HouseholdId,BudgetCategoryId")] Budget budget)
        {
            if (ModelState.IsValid)
            {
                db.Entry(budget).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", new { id = budget.Id });
            }
            return View(budget);
        }

        // GET: Budgets/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Budget budget = db.Budgets.Find(id);
            if (budget == null)
            {
                return HttpNotFound();
            }
            if (budget.HouseholdId != Convert.ToInt32(User.Identity.GetHouseholdId()))
            {
                return RedirectToAction("Unauthorized", "Home");
            }
            if (budget.BudgetCategory.Name == "General")
            {
                return RedirectToAction("Unauthorized", "Home");
            }
            decimal currentAmount = budget.Amount;
            foreach (var transaction in db.Transactions.Where(t => t.BudgetCategoryId == budget.BudgetCategoryId).ToList())
            {
                currentAmount -= transaction.Amount;
            }
            ViewBag.AmountRemaining = currentAmount;
            ViewBag.TransactionsCount = db.Transactions.Where(t => t.BudgetCategoryId == budget.BudgetCategoryId).ToList().Count();
            return View(budget);
        }

        // POST: Budgets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Budget budget = db.Budgets.Find(id);
            BudgetCategory budgetCategory = budget.BudgetCategory;
            Household household = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            foreach (var transaction in db.Transactions.Where(t => t.BudgetCategoryId == budgetCategory.Id).ToList())
            {
                transaction.BudgetCategoryId = household.BudgetCategories.FirstOrDefault(bc => bc.Name == "General").Id;
                db.Entry(transaction).Property("BudgetCategoryId").IsModified = true;
                db.SaveChanges();
            }
            db.BudgetCategories.Remove(budgetCategory);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
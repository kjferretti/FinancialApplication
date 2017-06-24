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
    public class TransactionsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Transactions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            if (transaction.Account.HouseholdId != Convert.ToInt32(User.Identity.GetHouseholdId()))
            {
                return RedirectToAction("Unauthorized", "Home");
            }
            return View(transaction);
        }

        // GET: Transactions/Create
        public ActionResult Create(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account account = db.Accounts.Find(id);
            if (account == null)
            {
                return HttpNotFound();
            }
            string currentUserId = User.Identity.GetUserId();
            if (!account.Household.Members.Select(m => m.Id).Contains(currentUserId))
            {
                return RedirectToAction("Unauthorized", "Home");
            }
            ViewBag.AccountName = account.Name;
            ViewBag.AccountId = id;
            ViewBag.BudgetCategoryId = new SelectList(db.BudgetCategories, "Id", "Name");
            return View();
        }

        // POST: Transactions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Description,Amount,Type,AccountId,BudgetCategoryId,Date")] TransactionCreateViewModel vm)
        {
            Household userHousehold = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            if (ModelState.IsValid && (userHousehold.BudgetCategories.Select(bc => bc.Id).Contains(vm.BudgetCategoryId) || db.BudgetCategories.Find(vm.BudgetCategoryId).Name == "General") && userHousehold.Accounts.Select(a => a.Id).Contains(vm.AccountId))
            {
                Transaction transaction = new Transaction();
                transaction.Description = vm.Description;
                transaction.Amount = vm.Amount;
                transaction.AccountId = vm.AccountId;
                transaction.BudgetCategoryId = vm.BudgetCategoryId;
                transaction.Date = vm.Date;
                if(vm.Type == "+")
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
                }
                else
                {
                    account.CurrentBalance = account.CurrentBalance + transaction.Amount;
                }
                db.Entry(account).Property("CurrentBalance").IsModified = true;
                db.SaveChanges();
                account.ReconciledBalance = account.CurrentBalance;
                db.Entry(account).Property("ReconciledBalance").IsModified = true;
                db.SaveChanges();

                return RedirectToAction("Details", "Accounts", new { id = account.Id });
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

        // GET: Transactions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            Account account = transaction.Account;
            string currentUserId = User.Identity.GetUserId();
            if (!account.Household.Members.Select(m => m.Id).Contains(currentUserId))
            {
                return RedirectToAction("Unauthorized", "Home");
            }
            ViewBag.AccountName = account.Name;
            ViewBag.BudgetCategoryId = new SelectList(db.BudgetCategories, "Id", "Name", transaction.BudgetCategoryId);
            TransactionEditViewModel vm = new TransactionEditViewModel();
            vm.Id = transaction.Id;
            vm.Description = transaction.Description;
            vm.Amount = transaction.Amount;
            vm.Expense = transaction.Expense;
            vm.Date = transaction.Date;
            vm.AccountId = transaction.AccountId;
            vm.BudgetCategoryId = transaction.BudgetCategoryId;
            vm.MadeById = transaction.MadeById;
            return View(vm);
        }

        // POST: Transactions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Description,Amount,Expense,Date,AccountId,BudgetCategoryId,MadeById")] TransactionEditViewModel vm)
        {
            Transaction transaction = new Transaction();

            if (ModelState.IsValid)
            {
                transaction.Id = vm.Id;
                transaction.Description = vm.Description;
                transaction.Amount = vm.Amount;
                transaction.Expense = vm.Expense;
                transaction.Date = vm.Date;
                transaction.AccountId = vm.AccountId;
                transaction.BudgetCategoryId = vm.BudgetCategoryId;
                transaction.MadeById = vm.MadeById;
                transaction.ForReconciled = false;
                db.Entry(transaction).State = EntityState.Modified;
                db.SaveChanges();

                Transaction rTransaction = db.Transactions.FirstOrDefault(t => t.ReconciledTransactionId == transaction.Id);
                if (rTransaction != null)
                {
                    rTransaction.Description = transaction.Description;
                    rTransaction.BudgetCategoryId = transaction.BudgetCategoryId;

                    db.Entry(rTransaction).Property("Description").IsModified = true;
                    db.SaveChanges();
                    db.Entry(rTransaction).Property("BudgetCategoryId").IsModified = true;
                    db.SaveChanges();
                }

                return RedirectToAction("Index", "Accounts");
            }
            ViewBag.BudgetCategoryId = new SelectList(db.BudgetCategories, "Id", "Name", transaction.BudgetCategoryId);
            return View(transaction);
        }

        // GET: Transactions/Edit/5
        public ActionResult Reconcile(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            Account account = transaction.Account;
            string currentUserId = User.Identity.GetUserId();
            if (!account.Household.Members.Select(m => m.Id).Contains(currentUserId))
            {
                return RedirectToAction("Unauthorized", "Home");
            }
            //put in logic for not being able to reconcile more than once
            ViewBag.AccountName = account.Name;
            TransactionReconcileViewModel vm = new TransactionReconcileViewModel();
            vm.ReconciledTransactionId = id.GetValueOrDefault();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reconcile([Bind(Include = "ReconciledTransactionId,NewAmount")] TransactionReconcileViewModel vm)
        {
            Transaction rTransaction = db.Transactions.AsNoTracking().FirstOrDefault(t => t.Id == vm.ReconciledTransactionId);
            Account account = db.Accounts.Find(rTransaction.AccountId);
            if (ModelState.IsValid)
            {
                Transaction transaction = new Transaction();
                transaction.Description = rTransaction.Description;
                transaction.ForReconciled = true;
                transaction.ReconciledTransactionId = vm.ReconciledTransactionId;
                transaction.Expense = rTransaction.Expense;
                transaction.Date = rTransaction.Date;
                transaction.AccountId = rTransaction.AccountId;
                transaction.BudgetCategoryId = rTransaction.BudgetCategoryId;
                transaction.MadeById = rTransaction.MadeById;
                transaction.Amount = vm.NewAmount - rTransaction.Amount;
                db.Transactions.Add(transaction);
                db.SaveChanges();

                if (transaction.Expense)
                {
                    account.ReconciledBalance = account.CurrentBalance - (transaction.Amount);
                }
                else
                {
                    account.ReconciledBalance = account.CurrentBalance + (transaction.Amount);
                }
                db.Entry(account).Property("ReconciledBalance").IsModified = true;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AccountName = account.Name;
            return View(vm);
        }

        // GET: Transactions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            if ((transaction.Account.HouseholdId != Convert.ToInt32(User.Identity.GetHouseholdId())) || transaction.ForReconciled)
            {
                return RedirectToAction("Unauthorized", "Home");
            }
            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Transaction transaction = db.Transactions.AsNoTracking().FirstOrDefault(t => t.Id == id);
            Account account = db.Accounts.Find(transaction.AccountId);
            Transaction rTransaction = db.Transactions.AsNoTracking().FirstOrDefault(t => t.ReconciledTransactionId == transaction.Id);
            if (rTransaction != null)
            {
                account.ReconciledBalance += rTransaction.Amount;
            }
            if (transaction.Expense)
            {
                account.CurrentBalance += transaction.Amount;
                account.ReconciledBalance += transaction.Amount;
            }
            else
            {
                account.CurrentBalance -= transaction.Amount;
                account.ReconciledBalance -= transaction.Amount;
            }
            if (rTransaction != null)
            {
                rTransaction = db.Transactions.Find(rTransaction.Id);
                db.Transactions.Remove(rTransaction);
                db.SaveChanges();
            }

            db.Entry(account).Property("ReconciledBalance").IsModified = true;
            db.SaveChanges();
            db.Entry(account).Property("CurrentBalance").IsModified = true;
            db.SaveChanges();

            transaction = db.Transactions.Find(id);
            db.Transactions.Remove(transaction);
            db.SaveChanges();

            return RedirectToAction("Index", "Accounts");
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

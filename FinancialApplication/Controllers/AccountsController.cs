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
    public class AccountsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Accounts
        public ActionResult Index()
        {
            int userHouseholdId = Convert.ToInt32(User.Identity.GetHouseholdId());
            var accounts = db.Accounts.Where(a => a.HouseholdId == userHouseholdId).Include(a => a.Household);
            ViewBag.HouseholdName = db.Households.Find(userHouseholdId).Name;
            return View(accounts.ToList());
        }

        // GET: Accounts/Details/5
        public ActionResult Details(int? id)
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
            return View(account);
        }

        // GET: Accounts/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Accounts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,CurrentBalance")] Account account)
        {
            if (ModelState.IsValid)
            {
                account.HouseholdId = Convert.ToInt32(User.Identity.GetHouseholdId());
                account.ReconciledBalance = account.CurrentBalance;
                db.Accounts.Add(account);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.HouseholdId = Convert.ToInt32(User.Identity.GetHouseholdId());
            return View(account);
        }

        // GET: Accounts/Edit/5
        public ActionResult Edit(int? id)
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
            return View(account);
        }

        // POST: Accounts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,CurrentBalance,ReconciledBalance,HouseholdId")] Account account)
        {
            if (ModelState.IsValid)
            {
                db.Entry(account).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(account);
        }

        // GET: Accounts/Delete/5
        public ActionResult Delete(int? id)
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
            return View(account);
        }

        // POST: Accounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Account account = db.Accounts.Find(id);
            foreach (var transaction in account.Transactions.ToList())
            {
                db.Transactions.Remove(transaction);
                db.SaveChanges();
            }
            db.Accounts.Remove(account);
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

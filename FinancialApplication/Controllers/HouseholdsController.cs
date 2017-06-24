using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FinancialApplication.Models;
using FinancialApplication.Models.CodeFirst;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Collections.Generic;
using FinancialApplication.Helpers;

namespace FinancialApplication.Controllers
{
    public class HouseholdsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Households
        [AuthorizeHousehold]
        public ActionResult Index()
        {
            // Use this for details since there will be no reason to view all households
            Household household = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            HouseholdIndexViewModel vm = new HouseholdIndexViewModel();
            vm.Id = household.Id;
            vm.Name = household.Name;
            vm.Members = household.Members;
            vm.JoinRequests = household.JoinRequests;
            vm.ExistingUsers = db.Users.ToList().Where(u => !household.Members.Contains(u)).Select(u => u.FullName).ToArray();

            return View(vm);
        }

        // POST: Households/Create
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Name, Invitees")] NeedHouseholdViewModel householdVM)
        {
            if (ModelState.IsValid)
            {
                string currentUserId = User.Identity.GetUserId();
                ApplicationUser currentUser = db.Users.Find(currentUserId);

                // Create new household
                Household household = new Household();
                household.Name = householdVM.Name;
                household.OwnerId = currentUserId;
                db.Households.Add(household);
                db.SaveChanges();
                
                currentUser.HouseholdId = household.Id;
                db.Entry(currentUser).Property("HouseholdId").IsModified = true;
                db.SaveChanges();

                BudgetCategory budgetCategory = new BudgetCategory();
                budgetCategory.Name = "General";
                budgetCategory.HouseholdId = household.Id;
                db.BudgetCategories.Add(budgetCategory);
                db.SaveChanges();

                Budget budget = new Budget();
                budget.Amount = 500.00M;
                budget.HouseholdId = household.Id;
                budget.BudgetCategoryId = budgetCategory.Id;
                db.Budgets.Add(budget);
                db.SaveChanges();

                if (householdVM.Invitees != null)
                {
                    // Send Invites
                    foreach (string invitee in householdVM.Invitees)
                    {
                        // good code for invite method also
                        bool userIsNotAlreadyHouseholdMember = household.Members.ToList().FirstOrDefault(m => m.Email == invitee || m.FullName == invitee) == null;

                        if (userIsNotAlreadyHouseholdMember)
                        {
                            // Taking this out because there will never be any invitations for this project as its just been created
                            // Use this code for the invite action and make sure its specific to household (I've already edited it)
                            // bool noExistingInvitation = !household.Invitations.Select(i => i.UserEmail).Contains(email) ;
                            //if (noExistingInvitation)
                            //{

                            bool userIsRegistered = db.Users.ToList().FirstOrDefault(u => u.FullName == invitee) != null;

                            //bool userIsRegistered = false;
                            //foreach (ApplicationUser user in db.Users.ToList())
                            //{
                            //    fullName = $"{user.FirstName} {user.LastName}";
                            //    if (fullName == invitee)
                            //    {
                            //        userIsRegistered = true;
                            //    }
                            //}
                            //don't need this because it's already covered by userIsNotAlreadyHouseholdMember (may be useful elsewhere)
                            //bool didNotEnterOwnName = currentUserId != db.Users.FirstOrDefault(u => u.FullName == invitee).Id;

                            if (userIsRegistered)
                            {
                                string email = db.Users.ToList().FirstOrDefault(u => u.FullName == invitee && u.Email != currentUser.Email).Email;
                                Invitation invitation = new Invitation();
                                invitation.HouseholdId = household.Id;
                                invitation.UserEmail = email;
                                db.Invitations.Add(invitation);
                                db.SaveChanges();

                                //email notification
                                var callbackUrl = Url.Action("JoinAccept", "Households", new { householdId = household.Id }, protocol: Request.Url.Scheme);
                                try
                                {
                                    var from = "ReFund Budgeter<kjferretti@gmail.com>";
                                    var to = email;
                                    var message = new MailMessage(from, to)
                                    {
                                        Subject = "You've Been Invited!",
                                        Body = $"You were invited to join household \"<strong>{household.Name}</strong>\" by <strong>{currentUser.FirstName} {currentUser.LastName}</strong>. Click <a href='{callbackUrl}'>here</a> to join.",
                                        IsBodyHtml = true
                                    };
                                    var svc = new PersonalEmail();
                                    await svc.SendAsync(message);
                                    ViewBag.Message = "Email has been sent";
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.Message);
                                    await Task.FromResult(0);
                                }
                            }
                            else
                            {
                                // If use is not registered then they should have provided an email
                                ValidEmailChecker VEC = new ValidEmailChecker();
                                bool validEmail = VEC.IsValidEmail(invitee);

                                if (validEmail)
                                {
                                    Invitation invitation = new Invitation();
                                    invitation.HouseholdId = household.Id;
                                    invitation.UserEmail = invitee;
                                    db.Invitations.Add(invitation);
                                    db.SaveChanges();

                                    bool emailBelongsToExistingUser = db.Users.Select(u => u.Email).Contains(invitee);
                                    if (emailBelongsToExistingUser)
                                    {
                                        //email notification
                                        var callbackUrl = Url.Action("JoinAccept", "Households", new { householdId = household.Id }, protocol: Request.Url.Scheme);
                                        try
                                        {
                                            var from = "ReFund Budgeter<kjferretti@gmail.com>";
                                            var to = invitee;
                                            var message = new MailMessage(from, to)
                                            {
                                                Subject = "You've Been Invited!",
                                                Body = $"You were invited to join household \"<strong>{household.Name}</strong>\" by <strong>{currentUser.FirstName} {currentUser.LastName}</strong>. Click <a href='{callbackUrl}'>here</a> to join.",
                                                IsBodyHtml = true
                                            };
                                            var svc = new PersonalEmail();
                                            await svc.SendAsync(message);
                                            ViewBag.Message = "Email has been sent";
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e.Message);
                                            await Task.FromResult(0);
                                        }
                                    }
                                    else
                                    {
                                        //email notification
                                        var callbackUrl = Url.Action("Register", "Account", null, protocol: Request.Url.Scheme);
                                        try
                                        {
                                            var from = "ReFund Budgeter<kjferretti@gmail.com>";
                                            var to = invitee;
                                            var message = new MailMessage(from, to)
                                            {
                                                Subject = "You've Been Invited!",
                                                Body = $"You were invited to join household \"<strong>{household.Name}</strong>\" by <strong>{currentUser.FirstName} {currentUser.LastName}</strong>. If you would like to join, you must first register for Refund Budgeter. After that, you can choose the join option to become a part of this household. Click <a href='{callbackUrl}'>here</a> to get started.",
                                                IsBodyHtml = true
                                            };
                                            var svc = new PersonalEmail();
                                            await svc.SendAsync(message);
                                            ViewBag.Message = "Email has been sent";
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e.Message);
                                            await Task.FromResult(0);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Refresh User's session so that they are associated with new household
                await ControllerContext.HttpContext.RefreshAuthentication(currentUser);

                return RedirectToAction("Index");
            }

            return RedirectToAction("JoinAccept", "Home");
        }

        // GET: Households/JoinAccept/5
        public ActionResult JoinAccept(int? householdId)
        {
            if (householdId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Household household = db.Households.Find(householdId);
            if (household == null)
            {
                return HttpNotFound();
            }
            ApplicationUser currentUser = db.Users.Find(User.Identity.GetUserId());
            var userInvitation = household.Invitations.FirstOrDefault(i => i.UserEmail == currentUser.Email && i.HouseholdId == householdId);
            if (userInvitation != null)
            {
                return View(household);
            }
            return RedirectToAction("Unauthorized", "Home");
        }

        // POST: Households/JoinAccept/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> JoinAccept(int householdId)
        {
            string currentUserId = User.Identity.GetUserId();
            ApplicationUser currentUser = db.Users.Find(currentUserId);

            currentUser.HouseholdId = householdId;
            db.Entry(currentUser).Property("HouseholdId").IsModified = true;
            db.SaveChanges();

            // Make sure since they joined the house to get rid of all outstanding invitations and joinRequests
            var invitations = db.Invitations.Where(i => i.UserEmail == currentUser.Email && i.HouseholdId == householdId).ToList();
            foreach (var invitation in invitations)
            {
                db.Invitations.Remove(invitation);
                db.SaveChanges();
            }
            var joinRequests = db.JoinRequests.Where(jr => jr.RequesterId == currentUserId && jr.HouseholdId == householdId).ToList();
            foreach (var joinRequest in joinRequests)
            {
                db.JoinRequests.Remove(joinRequest);
                db.SaveChanges();
            }

            // Refresh User's session so that they are associated with new household
            await ControllerContext.HttpContext.RefreshAuthentication(currentUser);

            return RedirectToAction("Index");
        }

        // GET: Households/Edit/5
        [AuthorizeHousehold]
        public ActionResult Edit(int? householdId)
        {
            if (householdId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Household household = db.Households.Find(householdId);
            if (household == null)
            {
                return HttpNotFound();
            }
            return View(household);
        }

        // POST: Households/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name")] Household household)
        {
            if (ModelState.IsValid)
            {
                db.Entry(household).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(household);
        }

        [AuthorizeHousehold]
        public ActionResult Invite(int? householdId)
        {
            if (householdId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Household household = db.Households.Find(householdId);
            if (household == null)
            {
                return HttpNotFound();
            }
            Invitation invitation = new Invitation();
            invitation.HouseholdId = householdId.GetValueOrDefault();
            return View(invitation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Invite([Bind(Include = "Invitees")] HouseholdIndexViewModel vm)
        {
            Household household = db.Households.Find(Convert.ToInt32(User.Identity.GetHouseholdId()));
            string currentUserId = User.Identity.GetUserId();
            ApplicationUser currentUser = db.Users.Find(currentUserId);
            if (ModelState.IsValid)
            {
                if (vm.Invitees != null)
                {
                    // Send Invites
                    foreach (string invitee in vm.Invitees)
                    {
                        bool userIsNotAlreadyHouseholdMember = household.Members.ToList().FirstOrDefault(m => m.Email == invitee || m.FullName == invitee) == null; // this will do for now but need to figure out way of solving 2 same names, either make names be unique (not optimal) or use email as value always

                        if (userIsNotAlreadyHouseholdMember)
                        {
                            bool userIsRegistered = db.Users.ToList().FirstOrDefault(u => u.FullName == invitee) != null;

                            if (userIsRegistered)
                            {
                                string email = db.Users.ToList().FirstOrDefault(u => u.FullName == invitee && u.Email != currentUser.Email).Email;
                                Invitation invitation = new Invitation();
                                invitation.HouseholdId = household.Id;
                                invitation.UserEmail = email;
                                db.Invitations.Add(invitation);
                                db.SaveChanges();

                                //email notification
                                var callbackUrl = Url.Action("JoinAccept", "Households", new { householdId = household.Id }, protocol: Request.Url.Scheme);
                                try
                                {
                                    var from = "ReFund Budgeter<kjferretti@gmail.com>";
                                    var to = email;
                                    var message = new MailMessage(from, to)
                                    {
                                        Subject = "You've Been Invited!",
                                        Body = $"You were invited to join household \"<strong>{household.Name}</strong>\" by <strong>{currentUser.FirstName} {currentUser.LastName}</strong>. Click <a href='{callbackUrl}'>here</a> to join.",
                                        IsBodyHtml = true
                                    };
                                    var svc = new PersonalEmail();
                                    await svc.SendAsync(message);
                                    ViewBag.Message = "Email has been sent";
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.Message);
                                    await Task.FromResult(0);
                                }
                            }
                            else
                            {
                                // If use is not registered then they should have provided an email
                                ValidEmailChecker VEC = new ValidEmailChecker();
                                bool validEmail = VEC.IsValidEmail(invitee);

                                if (validEmail)
                                {
                                    Invitation invitation = new Invitation();
                                    invitation.HouseholdId = household.Id;
                                    invitation.UserEmail = invitee;
                                    db.Invitations.Add(invitation);
                                    db.SaveChanges();

                                    bool emailBelongsToExistingUser = db.Users.Select(u => u.Email).Contains(invitee);
                                    if (emailBelongsToExistingUser)
                                    {
                                        //email notification
                                        var callbackUrl = Url.Action("JoinAccept", "Households", new { householdId = household.Id }, protocol: Request.Url.Scheme);
                                        try
                                        {
                                            var from = "ReFund Budgeter<kjferretti@gmail.com>";
                                            var to = invitee;
                                            var message = new MailMessage(from, to)
                                            {
                                                Subject = "You've Been Invited!",
                                                Body = $"You were invited to join household \"<strong>{household.Name}</strong>\" by <strong>{currentUser.FirstName} {currentUser.LastName}</strong>. Click <a href='{callbackUrl}'>here</a> to join.",
                                                IsBodyHtml = true
                                            };
                                            var svc = new PersonalEmail();
                                            await svc.SendAsync(message);
                                            ViewBag.Message = "Email has been sent";
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e.Message);
                                            await Task.FromResult(0);
                                        }
                                    }
                                    else
                                    {
                                        //email notification
                                        var callbackUrl = Url.Action("Register", "Account", null, protocol: Request.Url.Scheme);
                                        try
                                        {
                                            var from = "ReFund Budgeter<kjferretti@gmail.com>";
                                            var to = invitee;
                                            var message = new MailMessage(from, to)
                                            {
                                                Subject = "You've Been Invited!",
                                                Body = $"You were invited to join household \"<strong>{household.Name}</strong>\" by <strong>{currentUser.FirstName} {currentUser.LastName}</strong>. If you would like to join, you must first register for Refund Budgeter. After that, you can choose the join option to become a part of this household. Click <a href='{callbackUrl}'>here</a> to get started.",
                                                IsBodyHtml = true
                                            };
                                            var svc = new PersonalEmail();
                                            await svc.SendAsync(message);
                                            ViewBag.Message = "Email has been sent";
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e.Message);
                                            await Task.FromResult(0);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            return RedirectToAction("Index");
        }

        // POST: Households/Join
        public async Task<ActionResult> Join([Bind (Include = "HouseholdId")] NeedHouseholdViewModel householdVM)
        {
            if (ModelState.IsValid)
            {
                Household household = db.Households.Find(householdVM.HouseholdId);
                if (household != null)
                {
                    string currentUserId = User.Identity.GetUserId();
                    ApplicationUser currentUser = db.Users.Find(currentUserId);

                    // Check if user was already invited to this household
                    var existingInvitation = db.Invitations.FirstOrDefault(i => i.UserEmail == currentUser.Email && i.HouseholdId == household.Id);

                    // If not, we need to make a JoinRequest
                    if (existingInvitation == null)
                    {
                        var joinRequest = db.JoinRequests.FirstOrDefault(jr => jr.RequesterId == currentUserId && jr.HouseholdId == household.Id);
                        if (joinRequest == null)
                        {
                            joinRequest = new JoinRequest();
                            joinRequest.HouseholdId = household.Id;
                            joinRequest.RequesterId = currentUserId;
                            joinRequest.Approved = false;
                            joinRequest.DateRequested = DateTimeOffset.Now;
                            db.JoinRequests.Add(joinRequest);
                            db.SaveChanges();
                        }

                        TempData["requestSent"] = "sent";

                        return RedirectToAction("NoHouseholdIndex", "Home");
                    }
                    else
                    {
                        currentUser.HouseholdId = household.Id;
                        db.Entry(currentUser).Property("HouseholdId").IsModified = true;
                        db.SaveChanges();

                        var invitations = db.Invitations.Where(i => i.UserEmail == currentUser.Email && i.HouseholdId == household.Id).ToList();
                        foreach (var invitation in invitations)
                        {
                            db.Invitations.Remove(invitation);
                            db.SaveChanges();
                        }
                        var joinRequests = db.JoinRequests.Where(jr => jr.RequesterId == currentUserId && jr.HouseholdId == household.Id).ToList();
                        foreach (var joinRequest in joinRequests)
                        {
                            db.JoinRequests.Remove(joinRequest);
                            db.SaveChanges();
                        }

                        // Refresh User's session so that they are associated with new household
                        await ControllerContext.HttpContext.RefreshAuthentication(currentUser);

                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    return HttpNotFound();
                }
            }

            // Use ajax to repopulate the form for join and keep it on that tab
            return RedirectToAction("NoHouseholdIndex", "Home");
        }

        public async Task<ActionResult> Leave()
        {
            int? householdId = Convert.ToInt32(User.Identity.GetHouseholdId());
            if (householdId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Household household = db.Households.Find(householdId);
            if (household == null)
            {
                return HttpNotFound();
            }
            string currentUserId = User.Identity.GetUserId();
            ApplicationUser currentUser = db.Users.Find(currentUserId);
            currentUser.HouseholdId = null;
            db.Entry(currentUser).Property("HouseholdId").IsModified = true;
            db.SaveChanges();

            if (!household.Members.Any())
            {
                //also delete all joinrequests,categories,budgets,invitations,accounts,transactions,etc

                db.Households.Remove(household);
                db.SaveChanges();

                
            }

            // Refresh User's session so that they are no longer associated with old household
            await ControllerContext.HttpContext.RefreshAuthentication(currentUser);

            return RedirectToAction("Index", "Home");
        }

        // GET: Households/Delete/5
        [AuthorizeHousehold]
        public ActionResult Approve(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            JoinRequest joinRequest = db.JoinRequests.Find(id);
            if (joinRequest == null)
            {
                return HttpNotFound();
            }
            ApplicationUser user = joinRequest.Requester;
            user.HouseholdId = Convert.ToInt32(User.Identity.GetHouseholdId());
            db.Entry(user).Property("HouseholdId").IsModified = true;
            db.SaveChanges();

            db.JoinRequests.Remove(joinRequest);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: Households/Delete/5
        [AuthorizeHousehold]
        public ActionResult Reject(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            JoinRequest joinRequest = db.JoinRequests.Find(id);
            if (joinRequest == null)
            {
                return HttpNotFound();
            }

            db.JoinRequests.Remove(joinRequest);
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

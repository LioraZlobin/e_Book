using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using e_Book.Models;
using eBookLibrary.Models;

namespace e_Book.Controllers
{
    public class AccountController : Controller
    {
        private LibraryDbContext db = new LibraryDbContext();

        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(string email, string password)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == email && u.Password == password);
            if (user != null)
            {
                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                    1,
                    user.Email,
                    DateTime.Now,
                    DateTime.Now.AddMinutes(30),
                    false,
                    user.Role, // שמירת התפקיד ב-Ticket
                    FormsAuthentication.FormsCookiePath
                );

                string encryptedTicket = FormsAuthentication.Encrypt(ticket);
                HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                Response.Cookies.Add(cookie);

                // שמירת התפקיד ב-Session
                Session["Role"] = user.Role;
                Session["UserId"] = user.UserId;
                Session["UserName"] = user.Name;


                // הפנייה לפי תפקיד
                if (user.Role == "Admin")
                {
                    return RedirectToAction("AdminDashboard", "Account");
                }
                else
                {
                    return RedirectToAction("Index", "Books");
                }
            }

            ViewBag.Error = "שם משתמש או סיסמה אינם נכונים";
            return View();
        }

        public ActionResult AdminDashboard()
        {
            var userCount = db.Users.Count();
            var totalPurchases = db.Purchases.Count();
            var totalBorrows = db.Borrows.Count();

            ViewBag.UserCount = userCount;
            ViewBag.TotalPurchases = totalPurchases;
            ViewBag.TotalBorrows = totalBorrows;

            return View();
        }



        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("Login", "Account");
        }

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(string name, string email, string password, int age)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || age <= 0)
            {
                ViewBag.Error = "כל השדות חייבים להיות מלאים.";
                return View();
            }

            var existingUser = db.Users.FirstOrDefault(u => u.Email == email);
            if (existingUser != null)
            {
                ViewBag.Error = "האימייל הזה כבר רשום במערכת.";
                return View();
            }

            User newUser = new User
            {
                Name = name,
                Email = email,
                Password = password,
                Age = age, // הוספת הגיל למשתמש
                Role = "User" // כל משתמש חדש יקבל תפקיד ברירת מחדל "User"
            };

            db.Users.Add(newUser);
            db.SaveChanges();

            // לאחר ההרשמה נבצע התחברות אוטומטית
            FormsAuthentication.SetAuthCookie(newUser.Email, false);

            return RedirectToAction("Index", "Books");
        }

    }
}
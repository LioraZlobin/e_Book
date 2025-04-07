using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using e_Book.Models;
using eBookLibrary.Models;
using System.Security.Cryptography;
using System.Text;
using e_Book.Services;  // הוספת השורה הזאת


namespace e_Book.Controllers
{
    [RequireHttps]
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
            var user = db.Users.FirstOrDefault(u => u.Email == email);

            if (user != null)
            {
                // הצפנת הסיסמה שהמשתמש הזין
                string encryptedPassword = EncryptPassword(password);

                // השוואת הסיסמה המוצפנת שנשמרה בבסיס הנתונים עם הסיסמה שהוזנה
                if (user.Password == encryptedPassword)
                {
                    FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                        1,
                        user.Email,
                        DateTime.Now,
                        DateTime.Now.AddMinutes(30),
                        false,
                        user.Role,
                        FormsAuthentication.FormsCookiePath
                    );

                    string encryptedTicket = FormsAuthentication.Encrypt(ticket);
                    HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                    Response.Cookies.Add(cookie);

                    Session["Role"] = user.Role;
                    Session["UserId"] = user.UserId;
                    Session["UserName"] = user.Name;

                    if (user.Role == "Admin")
                    {
                        return RedirectToAction("AdminDashboard", "Account");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Books");
                    }
                }
                else
                {
                    ViewBag.Error = "סיסמה שגויה. נסה שוב.";
                    return View();
                }
            }

            ViewBag.Error = "אימייל זה לא נמצא במערכת. אנא נסה שוב.";
            return View();
        }



        // הפונקציה שתבצע את ההצפנה
        public string EncryptPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ממיר את הסיסמה לארגומנט byte[]
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                // הופך את ה-byte[] לערך Hex
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
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
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(string email, string newPassword)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                // הצפנת הסיסמה החדשה
                string encryptedPassword = EncryptPassword(newPassword);
                user.Password = encryptedPassword;

                // שמירת השינויים
                db.SaveChanges();

                // שליחה למייל על שינוי הסיסמה
                var emailService = new EmailService();
                emailService.SendEmail(user.Email, "הסיסמה שלך שונתה", "הסיסמה שלך שונתה בהצלחה.");

                ViewBag.Message = "הסיסמה החדשה נשלחה לאימייל שלך.";
                return RedirectToAction("Login", "Account");
            }
            else
            {
                ViewBag.Error = "לא נמצא משתמש עם אימייל זה.";
            }

            return View("Login", "Account");
        }





        // פונקציה ליצירת סיסמה אקראית
        private string GenerateRandomPassword()
        {
            var random = new Random();
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            string newPassword = new string(Enumerable.Repeat(validChars, 8)
                                      .Select(s => s[random.Next(s.Length)]).ToArray());
            return newPassword;
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

            // הצפנת הסיסמה לפני שמירתה
            string encryptedPassword = EncryptPassword(password);

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
                Password = encryptedPassword,  // שמירת הסיסמה המוצפנת
                Age = age,
                Role = "User"  // כל משתמש חדש יקבל תפקיד ברירת מחדל "User"
            };

            db.Users.Add(newUser);
            db.SaveChanges();

            // לאחר ההרשמה נבצע התחברות אוטומטית
            FormsAuthentication.SetAuthCookie(newUser.Email, false);

            return RedirectToAction("Index", "Books");
        }



    }
}
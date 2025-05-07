using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using e_Book.Models;
using eBookLibrary.Models;
using E_Book.Helpers;
using System.Data.Entity.Validation;
using System.Data.SqlClient;


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

            // החולשה - השאילתא כוללת גם את הסיסמה ולא בודקת בצורה בטוחה
            //var user = db.Users.SqlQuery(
            //  "SELECT * FROM Users WHERE Email = '" + email + "' AND Password = '" + PasswordHelper.HashPassword(password) + "'"
            //).FirstOrDefault();

            if (user != null)
            {
                // יצירת כרטיסיה (Ticket)
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

                return user.Role == "Admin"
                    ? RedirectToAction("AdminDashboard", "Account")
                    : RedirectToAction("Index", "Books");
            }

            ViewBag.Error = "אימייל או סיסמה אינם נכונים. נסה שוב.";
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

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ForgotPassword(string email)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Error = "לא נמצא משתמש עם כתובת אימייל זו.";
                return View();
            }

            // בעתיד אפשר לייצר טוקן ייחודי ולשלוח קישור – כאן נעבור ישר
            TempData["EmailForReset"] = email;
            return RedirectToAction("ResetPassword");
        }


        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("Login", "Account");
        }
        [AllowAnonymous]
  
        public ActionResult ResetPassword()
        {
            return View();
        }



        [HttpPost]
        [AllowAnonymous]
        public ActionResult ResetPassword(string email, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(newPassword))
            {
                ViewBag.Error = "יש למלא את כל השדות.";
                return View();
            }

            string hashedPassword = PasswordHelper.HashPassword(newPassword);

            // שלב 1: בדיקה אם המשתמש קיים
            var userExists = db.Users.Any(u => u.Email == email);
            if (!userExists)
            {
                ViewBag.Error = "אימייל זה לא נמצא במערכת.";
                return View();
            }

            // שלב 2: עדכון ישיר ב-SQL כדי לא להפעיל ולידציות של EF
            db.Database.ExecuteSqlCommand(
                "UPDATE Users SET Password = @p0 WHERE Email = @p1",
                hashedPassword, email
            );

            TempData["SuccessMessage"] = "הסיסמה עודכנה בהצלחה.";
            return RedirectToAction("Login");
        }




        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }
        public static bool IsValidCardNumber(string cardNumber)
        {
            int sum = 0;
            bool alternate = false;

            for (int i = cardNumber.Length - 1; i >= 0; i--)
            {
                char c = cardNumber[i];
                if (!char.IsDigit(c))
                    return false;

                int digit = c - '0';
                if (alternate)
                {
                    digit *= 2;
                    if (digit > 9)
                        digit -= 9;
                }
                sum += digit;
                alternate = !alternate;
            }

            return (sum % 10 == 0);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(string name, string email, string password, int age, string creditCardNumber, string expiryDate, string cvc)
        {
            // בדיקה אם שדות ריקים
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) || age <= 0 ||
                string.IsNullOrWhiteSpace(creditCardNumber) || string.IsNullOrWhiteSpace(expiryDate) || string.IsNullOrWhiteSpace(cvc))
            {
                ModelState.AddModelError("", "כל השדות חייבים להיות מלאים.");
            }

            // אימייל תקין
            if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ModelState.AddModelError("Email", "כתובת אימייל אינה תקינה.");
            }

            // בדיקת ת\"ז – בדיוק 9 ספרות
            if (name != null && name.All(char.IsDigit) && name.Length == 9)
            {
                // אם במקרה הכנסת ת\"ז במקום שם
                ModelState.AddModelError("Name", "נא להזין שם ולא תעודת זהות.");
            }

            // סיסמה – עד 10 תווים, לפחות אות אחת ומספר
            if (password.Length > 10)
            {
                ModelState.AddModelError("Password", "אורך הסיסמה חייב להיות לכל היותר 10 תווים.");
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[a-zA-Z]") ||
                !System.Text.RegularExpressions.Regex.IsMatch(password, @"\d"))
            {
                ModelState.AddModelError("Password", "הסיסמה חייבת לכלול לפחות אות אחת באנגלית ולפחות מספר אחד.");
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"^[a-zA-Z0-9]+$"))
            {
                ModelState.AddModelError("Password", "הסיסמה חייבת להיות מורכבת מתווים באנגלית ומספרים בלבד.");
            }

            // אימייל כבר קיים
            var existingUser = db.Users.FirstOrDefault(u => u.Email == email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "האימייל הזה כבר רשום במערכת.");
            }

            // כרטיס אשראי לפי אלגוריתם לונה
            if (!IsValidCardNumber(creditCardNumber))
            {
                ModelState.AddModelError("CreditCardNumber", "מספר כרטיס האשראי אינו תקף לפי אלגוריתם לונה.");
            }

            // תוקף בפורמט MM/YY
            if (!System.Text.RegularExpressions.Regex.IsMatch(expiryDate, @"^(0[1-9]|1[0-2])\/([0-9]{2})$"))
            {
                ModelState.AddModelError("ExpiryDate", "תוקף הכרטיס חייב להיות בפורמט MM/YY.");
            }
             else
    {
        // בדוק אם התאריך לא עבר את התאריך הנוכחי
        string[] parts = expiryDate.Split('/');
        int month = int.Parse(parts[0]);
        int year = int.Parse(parts[1]);

        // תאריך הנוכחי
        DateTime currentDate = DateTime.Now;
        int currentMonth = currentDate.Month;
        int currentYear = currentDate.Year % 100; // שנה לשניים האחרונים (כלומר 2025 -> 25)

        // השוואת החודש והשנה שהוזנו לתאריך הנוכחי
        if (year < currentYear || (year == currentYear && month < currentMonth))
        {
            ModelState.AddModelError("ExpiryDate", "תוקף הכרטיס לא יכול להיות עבר.");
        }
    }
            // CVC – 3 ספרות בדיוק
            if (cvc.Length != 3 || !cvc.All(char.IsDigit))
            {
                ModelState.AddModelError("CVC", "CVC חייב להיות בדיוק 3 ספרות.");
            }

            // סיום ולידציה
            if (!ModelState.IsValid)
            {
                return View();
            }

            // יצירת משתמש
            string hashedPassword = PasswordHelper.HashPassword(password);
            User newUser = new User
            {
                Name = name,
                Email = email,
                Password = hashedPassword,
                Age = age,
                Role = "User",
                CreditCardNumber = creditCardNumber,
                ExpiryDate = expiryDate,
                CVC = cvc
            };

            db.Users.Add(newUser);
            db.SaveChanges();

            FormsAuthentication.SetAuthCookie(newUser.Email, false);
            return RedirectToAction("Index", "Books");
        }
        [AllowAnonymous]
        public ActionResult TestInjectionSafe(string email)
        {
            // שימוש ב-LINQ כדי לבצע את השאילתה בצורה בטוחה
            var results = db.Users
                            .Where(u => u.Email == email)
                            .Select(u => u.CreditCardNumber)
                            .ToList();

            ViewBag.Results = results;
            return View("TestInjection");
        }



        [AllowAnonymous]
        public ActionResult TestInjectionFree(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                ViewBag.Error = "SQL query cannot be empty.";
                return View("TestInjection");
            }

            // Check for potentially harmful SQL commands like DROP or DELETE using IndexOf for case-insensitive search
            if (sql.IndexOf("DROP", StringComparison.OrdinalIgnoreCase) >= 0 || sql.IndexOf("DELETE", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                ViewBag.Error = "Unsafe SQL query detected.";
                return View("TestInjection");
            }

            // Safely execute the query using parameters
            var results = db.Database.SqlQuery<string>("SELECT CreditCardNumber FROM Users WHERE Email = @p0", new SqlParameter("@p0", sql))
                                     .ToList();

            ViewBag.Results = results;
            return View("TestInjection");
        }









    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using eBookLibrary.Models;
using e_Book.Models;
using e_Book.Services;

namespace e_Book.Controllers
{
    public class BorrowsController : Controller
    {
        private LibraryDbContext db = new LibraryDbContext();
        private readonly EmailService _emailService = new EmailService();

        // GET: Borrows
        public ActionResult Index()
        {
            return View(db.Borrows.ToList());
        }


        public void SendReminderEmails()
        {
            try
            {
                // שליפת ההשאלות שתאריך ההחזרה שלהן הוא 5 ימים מהיום
                var borrows = db.Borrows
                    .Where(b => !b.IsReturned && DbFunctions.DiffDays(DateTime.Now, b.DueDate) == 5 && !b.IsReminderSent)
                    .Include(b => b.Book)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"Found {borrows.Count} borrows with due date in 5 days.");

                foreach (var borrow in borrows)
                {
                    try
                    {
                        var user = db.Users.FirstOrDefault(u => u.UserId == borrow.UserId);
                        if (user != null && !string.IsNullOrEmpty(user.Email))
                        {
                            string subject = "תזכורת: החזרת ספר";
                            string body = $@"
                                  <div dir='rtl' style='text-align:right; font-family:Arial, sans-serif;'>
                                        <h2>תזכורת להחזרת ספר</h2>
                                        <p>שלום {user.Name},</p>
                                        <p>נותרו לך 5 ימים להחזרת הספר <strong>{borrow.Book.Title}</strong>.</p>
                                        <p><strong>תאריך החזרה:</strong> {borrow.DueDate:dd/MM/yyyy}</p>
                                        <p>תודה,<br/>צוות הספרייה</p>
                                  </div>";

                            // שליחת מייל
                            _emailService.SendEmail(user.Email, subject, body);
                            borrow.IsReminderSent = true;
                            System.Diagnostics.Debug.WriteLine($"Email sent to {user.Email} for book \"{borrow.Book.Title}\".");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"User or email not found for borrow ID {borrow.BorrowId}.");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error sending email for borrow ID {borrow.BorrowId}: {ex.Message}");
                    }
                }
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SendReminderEmails: {ex.Message}");
            }
        }

        [HttpGet]
        public ActionResult TestReminderEmails()
        {
            SendReminderEmails(); // הפעלת הפונקציה
            return Content("Reminder emails sent successfully."); // הודעה על הצלחה
        }




        // GET: Borrows/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Borrow borrow = db.Borrows.Find(id);
            if (borrow == null)
            {
                return HttpNotFound();
            }
            return View(borrow);
        }

        // GET: Borrows/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Borrows/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "UserId,BookId")] Borrow borrow)
        {
            var book = db.Books.Find(borrow.BookId);

            // בדיקה אם יש עותקים זמינים
            if (book.AvailableCopies <= 0)
            {
                // הוספה לרשימת המתנה
                db.WaitingLists.Add(new WaitingList { BookId = borrow.BookId, UserId = borrow.UserId });
                db.SaveChanges();
                TempData["Message"] = "הספר אינו זמין כרגע. נוספת לרשימת ההמתנה.";
                return RedirectToAction("Index");
            }

            // בדיקה אם המשתמש השאיל כבר 3 ספרים
            var userBorrows = db.Borrows.Count(b => b.UserId == borrow.UserId && !b.IsReturned);
            if (userBorrows >= 3)
            {
                TempData["Error"] = "אינך יכול להשאיל יותר מ-3 ספרים במקביל.";
                return RedirectToAction("Index");
            }

            // יצירת השאלה
            borrow.BorrowDate = DateTime.Now;
            borrow.DueDate = DateTime.Now.AddDays(30);
            borrow.IsReturned = false;

            db.Borrows.Add(borrow);
            book.AvailableCopies--; // הורדת עותק זמין
            db.SaveChanges();

            TempData["Success"] = "הספר הושאל בהצלחה.";
            return RedirectToAction("Index");
        }


        // GET: Borrows/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Borrow borrow = db.Borrows.Find(id);
            if (borrow == null)
            {
                return HttpNotFound();
            }
            return View(borrow);
        }

        // POST: Borrows/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "BorrowId,UserId,BookId,BorrowDate,DueDate")] Borrow borrow)
        {
            if (ModelState.IsValid)
            {
                db.Entry(borrow).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(borrow);
        }

        // GET: Borrows/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Borrow borrow = db.Borrows.Find(id);
            if (borrow == null)
            {
                return HttpNotFound();
            }
            return View(borrow);
        }

        // POST: Borrows/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Borrow borrow = db.Borrows.Find(id);
            db.Borrows.Remove(borrow);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReturnBook(int? borrowId, int? bookId)
        {
            if (borrowId.HasValue)
            {
                // הטיפול במקרה של החזרת ספר שהושאל
                var borrow = db.Borrows.FirstOrDefault(b => b.BorrowId == borrowId);
                if (borrow == null || borrow.IsReturned)
                {
                    TempData["Error"] = "הספר כבר הוחזר או אינו קיים.";
                    return RedirectToAction("UserLibrary");
                }

                borrow.IsReturned = true; // סימון הספר כהוחזר
                var book = db.Books.FirstOrDefault(b => b.BookId == borrow.BookId);


                if (book == null)
                {
                    TempData["Error"] = "הספר אינו קיים.";
                    return RedirectToAction("UserLibrary");
                }

                // הגדלת העותקים הזמינים
                book.AvailableCopies++;

                // שליפת 3 הראשונים ברשימת ההמתנה
                var waitingUsers = db.WaitingLists
                    .Where(w => w.BookId == book.BookId)
                    .OrderBy(w => w.Position)
                    .Take(3)
                    .ToList();

                foreach (var waitingUser in waitingUsers)
                {
                    var user = db.Users.FirstOrDefault(u => u.UserId == waitingUser.UserId);
                    if (user != null && !string.IsNullOrEmpty(user.Email))
                    {
                        string subject = "הספר זמין להשאלה!";
                        string body = $@"
                     <div dir='rtl' style='text-align:right; font-family:Arial, sans-serif;'>
                          <h2>הספר '{book.Title}' זמין להשאלה</h2>
                          <p>שלום {user.Name},</p>
                          <p>הספר שחיכית לו חזר למלאי. מהרו לבצע השאלה לפני שיאזל.</p>
                          <p>שימו לב: הספר יוקצה למי שישלים את תהליך ההשאלה ראשון.</p>
                          <p>תודה,<br/>צוות הספרייה</p>
                     </div>";

                        _emailService.SendEmail(user.Email, subject, body);
                    }
                }
                db.SaveChanges();
                TempData["Success"] = "הספר הוחזר בהצלחה ונשלחו הודעות לממתינים.";
            }
            else if (bookId.HasValue)
            {
                // הטיפול במקרה של החזרת ספר שנרכש (אם רלוונטי)
                int userId = GetCurrentUserId();

                var purchasedBook = db.Purchases.FirstOrDefault(p => p.BookId == bookId && p.UserId == userId);
                if (purchasedBook == null)
                {
                    TempData["Error"] = "הספר אינו קיים או לא נרכש.";
                    return RedirectToAction("UserLibrary");
                }

                // מחיקת הרכישה
                db.Purchases.Remove(purchasedBook);
                TempData["Success"] = "הרכישה הוסרה מהספרייה האישית שלך.";
            }
            else
            {
                TempData["Error"] = "פרמטר לא תקין.";
                return RedirectToAction("UserLibrary");
            }

            db.SaveChanges();
            return RedirectToAction("UserLibrary");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePurchasedBook(int bookId)
        {
            int userId = GetCurrentUserId();

            var purchasedBook = db.Purchases.FirstOrDefault(p => p.BookId == bookId && p.UserId == userId);
            if (purchasedBook == null)
            {
                TempData["Error"] = "הספר לא נמצא ברשימת הרכישות שלך.";
                return RedirectToAction("UserLibrary");
            }

            // מחיקת הרכישה
            db.Purchases.Remove(purchasedBook);
            db.SaveChanges();

            TempData["Success"] = "הספר נמחק בהצלחה מהספרייה האישית שלך.";
            return RedirectToAction("UserLibrary");
        }



        public ActionResult UserLibrary()
        {
            int userId = GetCurrentUserId();

            // שליפת ספרים מושאלים
            var borrowedBooks = db.Borrows
                .Where(b => b.UserId == userId && !b.IsReturned)
                .Select(b => new UserLibraryItem
                {
                    BookId = b.BookId,
                    BorrowId = b.BorrowId,
                    Title = b.Book.Title,
                    BorrowDate = b.BorrowDate,
                    DueDate = b.DueDate,
                    IsBorrowed = true,
                    IsPurchased = false,
                    DownloadLink = b.Book.DownloadLink
                })
                .ToList();

            // שליפת ספרים שנרכשו
            var purchasedBooks = db.Purchases
                .Where(p => p.UserId == userId)
                .Select(p => new UserLibraryItem
                {
                    BookId = p.BookId,
                    Title = p.Book.Title,
                    PurchaseDate = p.PurchaseDate,
                    IsBorrowed = false,
                    IsPurchased = true,
                    DownloadLink = p.Book.DownloadLink
                })
                .ToList();

            // שילוב הספרים המושאלים והרכושים לרשימה אחת
            var userLibrary = borrowedBooks.Union(purchasedBooks).ToList();

            return View(userLibrary);
        }



        private int GetCurrentUserId()
        {
            // חישוב מזהה משתמש נוכחי
            return db.Users.FirstOrDefault(u => u.Email == User.Identity.Name)?.UserId ?? 0;
        }

        [HttpPost]
        [Authorize]
        public ActionResult Borrow(int bookId)
        {
            // שליפת המשתמש המחובר
            var userEmail = User.Identity.Name;
            var user = db.Users.FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // בדיקת מגבלת השאלת ספרים
            var activeBorrows = db.Borrows.Where(b => b.UserId == user.UserId && !b.IsReturned).Count();
            if (activeBorrows >= 3)
            {
                TempData["Error"] = "לא ניתן להשאיל יותר מ-3 ספרים בו-זמנית.";
                return RedirectToAction("Index", "Books");
            }

            // שליפת הספר ובדיקת עותקים זמינים
            var book = db.Books.Find(bookId);
            if (book == null)
            {
                TempData["Error"] = "הספר לא נמצא.";
                return RedirectToAction("Index", "Books");
            }

            // בדיקת הגבלת גיל
            if (!string.IsNullOrEmpty(book.AgeRestriction) &&
                int.TryParse(book.AgeRestriction.Replace("+", ""), out int minAge) &&
                user.Age < minAge)
            {
                TempData["Error"] = $"אינך עומד בדרישות הגיל ({book.AgeRestriction}).";
                return RedirectToAction("Index", "Books");
            }



            // יצירת השאלה חדשה
            var borrow = new Borrow
            {
                UserId = user.UserId,
                BookId = bookId,
                BorrowDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(30)
            };

            db.Borrows.Add(borrow);
            book.AvailableCopies -= 1; // עדכון כמות העותקים
            db.SaveChanges();

            TempData["Success"] = "הספר הושאל בהצלחה!";
            return RedirectToAction("Index", "Books");
        }

        [HttpGet]
        public ActionResult DownloadBook(int bookId)
        {
            // בדיקת מזהה משתמש נוכחי מתוך ה-Session
            if (Session["UserId"] == null)
            {
                TempData["Error"] = "אנא התחבר כדי להוריד ספרים.";
                return RedirectToAction("Login", "Account");
            }

            int userId = int.Parse(Session["UserId"].ToString());

            // בדיקה אם המשתמש רכש או השאיל את הספר
            bool hasAccess = db.Purchases.Any(p => p.UserId == userId && p.BookId == bookId) ||
                             db.Borrows.Any(b => b.UserId == userId && b.BookId == bookId);

            if (hasAccess)
            {
                // חיפוש הספר והפניה לקישור ההורדה אם קיים
                var book = db.Books.FirstOrDefault(b => b.BookId == bookId);
                if (book != null && !string.IsNullOrEmpty(book.DownloadLink))
                {
                    return Redirect(book.DownloadLink);
                }

                TempData["Error"] = "לא נמצא קישור הורדה לספר זה.";
            }
            else
            {
                TempData["Error"] = "אין לך גישה להוריד את הספר הזה.";
            }

            return RedirectToAction("UserLibrary", "Borrows");
        }


        public void ProcessExpiredBorrows()
        {
            try
            {
                // שליפת כל ההשאלות שפג תוקפן ולא הוחזרו
                var expiredBorrows = db.Borrows
                    .Where(b => !b.IsReturned && b.DueDate < DateTime.Now)
                    .Include(b => b.Book)
                    .ToList();

                foreach (var borrow in expiredBorrows)
                {
                    // סימון הספר כהוחזר
                    borrow.IsReturned = true;

                    // הגדלת כמות העותקים הזמינים
                    if (borrow.Book != null)
                    {
                        borrow.Book.AvailableCopies++;
                    }
                }

                // שמירת השינויים במסד הנתונים
                db.SaveChanges();

                TempData["Success"] = "התהליך לעדכון השאלות שפג תוקפן הושלם בהצלחה.";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ProcessExpiredBorrows: {ex.Message}");
                TempData["Error"] = "אירעה שגיאה בתהליך עדכון ההשאלות שפג תוקפן.";
            }
        }


        [HttpGet]
        public ActionResult RunBorrowCleanup()
        {
            ProcessExpiredBorrows();
            return Content("Borrow cleanup process completed successfully.");
        }


    }
}

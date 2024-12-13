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

namespace e_Book.Controllers
{
    public class BorrowsController : Controller
    {
        private LibraryDbContext db = new LibraryDbContext();

        // GET: Borrows
        public ActionResult Index()
        {
            return View(db.Borrows.ToList());
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
        public ActionResult ReturnBook(int borrowId)
        {
            var borrow = db.Borrows.Find(borrowId);
            if (borrow == null || borrow.IsReturned)
            {
                TempData["Error"] = "הספר כבר הוחזר או אינו קיים.";
                return RedirectToAction("UserLibrary");
            }

            borrow.IsReturned = true; // סימון הספר כהוחזר
            var book = db.Books.Find(borrow.BookId);

            if (book == null)
            {
                TempData["Error"] = "הספר אינו קיים.";
                return RedirectToAction("UserLibrary");
            }

            // טיפול ברשימת ההמתנה
            var waitingUser = db.WaitingLists.FirstOrDefault(w => w.BookId == book.BookId);
            if (waitingUser != null)
            {
                // הסרת המשתמש הראשון מרשימת ההמתנה
                db.WaitingLists.Remove(waitingUser);

                // הוספת הספר לעגלת הקניות של המשתמש הראשון
                var newCartItem = new CartItem
                {
                    UserId = waitingUser.UserId,
                    BookId = book.BookId,
                    Quantity = 1,
                    TransactionType = "borrow"
                };

                db.CartItems.Add(newCartItem);
                TempData["Success"] = "הספר הועבר למשתמש הראשון ברשימת ההמתנה.";
            }
            else
            {
                // אין רשימת המתנה, עדכון העותקים הזמינים
                book.AvailableCopies++;
                TempData["Success"] = "הספר הוחזר בהצלחה.";
            }

            db.SaveChanges();
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

            if (book.AvailableCopies <= 0)
            {
                // הוספת המשתמש לרשימת ההמתנה
                var waitingPosition = db.WaitingLists.Count(w => w.BookId == bookId) + 1;
                db.WaitingLists.Add(new WaitingList
                {
                    BookId = bookId,
                    UserId = user.UserId,
                    Position = waitingPosition,
                    AddedDate = DateTime.Now
                });
                db.SaveChanges();

                TempData["Error"] = $"הספר אינו זמין כרגע. נוספת לרשימת ההמתנה במקום {waitingPosition}.";
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




    }
}

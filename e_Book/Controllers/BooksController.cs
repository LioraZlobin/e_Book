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
using System.Web.Security;
using System.IO;

namespace e_Book.Controllers
{
    [RequireHttps]
    public class BooksController : Controller
    {
        private LibraryDbContext db = new LibraryDbContext();

        // GET: Books
        [AllowAnonymous]
        public ActionResult Index()
        {
            var userId = GetLoggedInUserId();

            if (userId > 0)
            {
                var user = db.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    ViewBag.UserAge = user.Age; // הגיל של המשתמש
                }
            }
            else
            {
                ViewBag.UserAge = 0; // משתמש לא מחובר
            }

            var books = db.Books.ToList();

            // טעינת משובים עם פרטי המשתמש
            ViewBag.ServiceFeedbacks = db.Feedbacks
                .Include(f => f.User) // טעינת המידע של המשתמש
                .Where(f => f.IsPurchaseFeedback)
                .OrderByDescending(f => f.FeedbackDate)
                .ToList();

            return View(books);
        }


        private int GetLoggedInUserId()
        {
            var userEmail = User.Identity.Name; // משתמש מחובר לפי ה-Email
            var user = db.Users.FirstOrDefault(u => u.Email == userEmail);
            return user?.UserId ?? 0; // אם המשתמש לא נמצא, נחזיר 0
        }



        // GET: Books/Details/5
        [AllowAnonymous]
        public ActionResult Details(int? id)
        {
            var book = db.Books.Find(id);
            if (book == null)
                return HttpNotFound();

            var userId = GetLoggedInUserId();

            // בדיקה אם המשתמש שילם על הספר
            var canRate = db.Payments.Any(p => p.UserId == userId && p.BookId == id);

            ViewBag.CanRate = canRate;

            // טעינת משובים לספר
            ViewBag.BookFeedbacks = db.Feedbacks
                .Where(f => f.BookId == id && !f.IsPurchaseFeedback)
                .ToList();

            return View(book);
        }


        // GET: Books/Create
        public ActionResult Create()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                TempData["Error"] = "אין לך הרשאה להוסיף ספרים חדשים ולבצע שינויים באתר.";
                return RedirectToAction("Index");
            }
            var newBook = new Book
            {
                AgeRestriction = "All Ages" // Default value for AgeRestriction
            };

            return View(newBook);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "BookId,Title,Author,Publisher,PriceBuy,PriceBorrow,AvailableCopies,Format,YearPublished,IsBorrowable, AgeRestriction,ImageUrl,Synopsis, DownloadLink")] Book book)
        {
            if (book.PriceBorrow >= book.PriceBuy)
            {
                ModelState.AddModelError("PriceBorrow", "מחיר ההשאלה חייב להיות קטן ממחיר הקנייה.");
            }

            // בדיקה אם IsBorrowable מסומן אך PriceBorrow לא הוזן
            if (book.IsBorrowable&&book.PriceBorrow==0)
            {
                ModelState.AddModelError("PriceBorrow", "יש להזין מחיר השאלה עבור ספרים שניתנים להשאלה.");
            }
            // בדיקה אם ספר עם אותם פרטים כבר קיים (כותרת, מחבר, הוצאה לאור ושנת הוצאה)
            bool isBookExists = db.Books.Any(b =>
                b.Title.Equals(book.Title, StringComparison.OrdinalIgnoreCase) &&
                b.Author.Equals(book.Author, StringComparison.OrdinalIgnoreCase) &&
                b.Publisher.Equals(book.Publisher, StringComparison.OrdinalIgnoreCase) &&
                b.YearPublished == book.YearPublished); // הוספת תנאי לשנת ההוצאה

            if (isBookExists)
            {
                // הוספת הודעת שגיאה לתצוגה
                TempData["Error"] = "לא ניתן להוסיף ספר. ספר עם אותו מחבר, כותרת, הוצאה ושנת הוצאה כבר קיים במערכת.";
                return View(book);
            }

            if (ModelState.IsValid)
            {
                db.Books.Add(book);
                db.SaveChanges();
                TempData["Success"] = "הספר נוסף בהצלחה!";
                return RedirectToAction("Index");
            }

            return View(book);
        }

        // GET: Books/Edit/5
        public ActionResult Edit(int? id)
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                TempData["Error"] = "אין לך הרשאה לערוך ספרים באתר.";
                return RedirectToAction("Index");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "BookId,Title,Author,Publisher,PriceBuy,PriceBorrow,PreviousPrice,AvailableCopies,Format,Genre,Popularity,YearPublished,IsBorrowable,DiscountEndDate,AgeRestriction,ImageUrl,Synopsis,DownloadLink")] Book book)
        {
            if (ModelState.IsValid)
            {
                // בדיקה אם מחיר ההשאלה קטן ממחיר הקנייה
                if (book.PriceBorrow >= book.PriceBuy)
                {
                    ModelState.AddModelError("PriceBorrow", "מחיר ההשאלה חייב להיות קטן ממחיר הקנייה.");
                    return View(book); // חזרה לטופס אם יש בעיה
                }

                // שליפת הספר המקורי ממסד הנתונים
                var originalBook = db.Books.AsNoTracking().FirstOrDefault(b => b.BookId == book.BookId);

                if (originalBook != null && book.PriceBuy < originalBook.PriceBuy)
                {
                    // עדכון מחיר קודם
                    book.PreviousPrice = originalBook.PriceBuy;
                    // הגדרת תאריך סיום הנחה לשבוע מהיום
                    book.DiscountEndDate = DateTime.Now.AddDays(7);
                }

                // עדכון הספר במסד הנתונים
                db.Entry(book).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Success"] = "הספר עודכן בהצלחה!";
                return RedirectToAction("Index");
            }

            TempData["Error"] = "שגיאה בעדכון הספר.";
            return View(book);
        }


        // GET: Books/Delete/5
        public ActionResult Delete(int? id)
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                TempData["Error"] = "אין לך הרשאה למחוק ספרים באתר.";
                return RedirectToAction("Index");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Book book = db.Books.Find(id);
            db.Books.Remove(book);
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

        [AllowAnonymous]
        public ActionResult Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction("Index");
            }

            var books = db.Books
                .Where(b => b.Title.Contains(query) || b.Author.Contains(query) || b.Publisher.Contains(query))
                .ToList();

            // אתחול ViewBag.ServiceFeedbacks
            ViewBag.ServiceFeedbacks = db.Feedbacks
                .Include(f => f.User) // טוען את המידע של המשתמש
                .Where(f => f.IsPurchaseFeedback)
                .OrderByDescending(f => f.FeedbackDate)
                .ToList();

            return View("Index", books); // השתמשי בתצוגת Index כדי להציג את התוצאות
        }

        [AllowAnonymous]
        public ActionResult Filter(decimal? minPrice, decimal? maxPrice, string format)
        {
            var books = db.Books.AsQueryable();

            if (minPrice.HasValue)
            {
                books = books.Where(b => b.PriceBuy >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                books = books.Where(b => b.PriceBuy <= maxPrice.Value);
            }

            if (!string.IsNullOrEmpty(format))
            {
                books = books.Where(b => b.Format == format);
            }

            // אתחול ViewBag.ServiceFeedbacks
            ViewBag.ServiceFeedbacks = db.Feedbacks
                .Include(f => f.User) // טוען את המידע של המשתמש
                .Where(f => f.IsPurchaseFeedback)
                .OrderByDescending(f => f.FeedbackDate)
                .ToList();

            return View("Index", books.ToList()); // הצגת התוצאות בתצוגת Index
        }

        [AllowAnonymous]
        public ActionResult Sort(string sortBy)
        {
            var books = db.Books.AsQueryable();

            switch (sortBy)
            {
                case "price_asc":
                    books = books.OrderBy(b => b.PriceBuy);
                    break;
                case "price_desc":
                    books = books.OrderByDescending(b => b.PriceBuy);
                    break;
                case "popularity":
                    books = books.OrderByDescending(b => b.Popularity); // הוספת סינון לפי פופולריות
                    break;
                case "year":
                    books = books.OrderByDescending(b => b.YearPublished);
                    break;
                case "year_asc":
                    books = books.OrderBy(b => b.YearPublished);
                    break;
                default:
                    books = books.OrderBy(b => b.Title);
                    break;

            }

            // אתחול ViewBag.ServiceFeedbacks
            ViewBag.ServiceFeedbacks = db.Feedbacks
                .Include(f => f.User) // טוען את המידע של המשתמש
                .Where(f => f.IsPurchaseFeedback)
                .OrderByDescending(f => f.FeedbackDate)
                .ToList();

            return View("Index", books.ToList());
        }

        [AllowAnonymous]
        public ActionResult FilterDiscountedBooks()
        {
            var discountedBooks = db.Books
                .Where(b => b.PreviousPrice.HasValue && b.DiscountEndDate.HasValue && b.DiscountEndDate > DateTime.Now)
                .ToList();

            // אתחול ViewBag.ServiceFeedbacks
            ViewBag.ServiceFeedbacks = db.Feedbacks
                .Include(f => f.User) // טוען את המידע של המשתמש
                .Where(f => f.IsPurchaseFeedback)
                .OrderByDescending(f => f.FeedbackDate)
                .ToList();

            return View("Index", discountedBooks); // מציג את הספרים בתצוגת Index
        }


        [AllowAnonymous]
        public ActionResult FilterByGenre(string genre)
        {
            if (string.IsNullOrEmpty(genre))
            {
                return RedirectToAction("Index");
            }

            // סינון הספרים לפי הז'אנר שנבחר (תמיכה בעברית, אי-תלות ברישיות)
            var booksByGenre = db.Books
                .Where(b => b.Genre.Trim().Equals(genre.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();

            // אתחול ViewBag.ServiceFeedbacks
            ViewBag.ServiceFeedbacks = db.Feedbacks
                .Include(f => f.User) // טוען את המידע של המשתמש
                .Where(f => f.IsPurchaseFeedback)
                .OrderByDescending(f => f.FeedbackDate)
                .ToList();

            return View("Index", booksByGenre);
        }

        [AllowAnonymous]
        public ActionResult FilterByPrice(decimal? minPrice, decimal? maxPrice)
        {
            var books = db.Books.AsQueryable();

            // בדיקת תנאים עבור טווחי המחירים
            if (minPrice.HasValue)
            {
                books = books.Where(b => b.PriceBuy >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                books = books.Where(b => b.PriceBuy <= maxPrice.Value);
            }

            // אתחול ViewBag.ServiceFeedbacks
            ViewBag.ServiceFeedbacks = db.Feedbacks
                .Include(f => f.User) // טוען את המידע של המשתמש
                .Where(f => f.IsPurchaseFeedback)
                .OrderByDescending(f => f.FeedbackDate)
                .ToList();

            return View("Index", books.ToList());
        }
        [AllowAnonymous]
        public ActionResult FilterByBorrowPrice(decimal? minPrice, decimal? maxPrice)
        {
            var books = db.Books.AsQueryable();

            // סינון רק ספרים שניתן להשאיל אותם
            books = books.Where(b => b.IsBorrowable == true);

            // סינון לפי מחיר ההשאלה (PriceBorrow)
            if (minPrice.HasValue)
            {
                books = books.Where(b => b.PriceBorrow >= minPrice.Value); // מינימום מחיר השאלה
            }

            if (maxPrice.HasValue)
            {
                books = books.Where(b => b.PriceBorrow <= maxPrice.Value); // מקסימום מחיר השאלה
            }

            // אתחול ViewBag.ServiceFeedbacks
            ViewBag.ServiceFeedbacks = db.Feedbacks
                .Include(f => f.User) // טוען את המידע של המשתמש
                .Where(f => f.IsPurchaseFeedback)
                .OrderByDescending(f => f.FeedbackDate)
                .ToList();

            return View("Index", books.ToList());
        }



        [AllowAnonymous]
        public ActionResult FilterByAvailability(string availability)
        {
            if (string.IsNullOrEmpty(availability))
            {
                return RedirectToAction("Index");
            }

            IQueryable<Book> books = db.Books;

            switch (availability.ToLower())
            {
                case "borrow":
                    books = books.Where(b => b.IsBorrowable == true); // ספרים להשאלה בלבד
                    break;
                case "buy":
                    books = books.Where(b => b.IsBorrowable == false); // ספרים לקנייה בלבד (IsBorrowable == false או 0)
                    break;
                default:
                    return RedirectToAction("Index");
            }

            // אתחול ViewBag.ServiceFeedbacks
            ViewBag.ServiceFeedbacks = db.Feedbacks
                .Include(f => f.User) // טוען את המידע של המשתמש
                .Where(f => f.IsPurchaseFeedback)
                .OrderByDescending(f => f.FeedbackDate)
                .ToList();

            return View("Index", books.ToList());
        }


        private string GetUserRole()
        {
            if (Session["Role"] != null)
            {
                return Session["Role"].ToString();
            }

            if (User.Identity.IsAuthenticated)
            {
                FormsIdentity identity = (FormsIdentity)User.Identity;
                FormsAuthenticationTicket ticket = identity.Ticket;
                return ticket.UserData; // התפקיד מאוחסן ב-UserData
            }

            return null; // משתמש לא מחובר
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdatePrice(int bookId, decimal newPrice, DateTime? discountEndDate = null)
        {
            var book = db.Books.Find(bookId);
            if (book == null)
            {
                TempData["Error"] = "הספר לא נמצא.";
                return RedirectToAction("Index");
            }

            if (newPrice < book.PriceBuy)
            {
                book.PreviousPrice = book.PriceBuy; // שמירת המחיר הקודם
                book.PriceBuy = newPrice;
                book.DiscountEndDate = discountEndDate ?? DateTime.Now.AddDays(7); // ברירת מחדל: שבוע
            }
            else
            {
                book.PriceBuy = newPrice;
                book.PreviousPrice = null;
                book.DiscountEndDate = null;
            }

            db.SaveChanges();
            TempData["Success"] = "מחיר הספר עודכן בהצלחה.";
            return RedirectToAction("Index");
        }

        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddFeedback(int BookId, string Content, int Rating, bool IsPurchaseFeedback)
        {
            var userId = GetLoggedInUserId();

            if (userId <= 0)
            {
                TempData["Error"] = "משתמש לא מחובר.";
                return RedirectToAction("Login", "Account");
            }

            // בדיקה אם המשתמש יכול להוסיף משוב על ספר
            if (!IsPurchaseFeedback)
            {
                var hasAccess = db.Borrows.Any(b => b.UserId == userId && b.BookId == BookId && !b.IsReturned) ||
                                db.Purchases.Any(p => p.UserId == userId && p.BookId == BookId);

                if (!hasAccess)
                {
                    TempData["Error"] = "רק משתמשים שרכשו או השאילו את הספר יכולים לדרג אותו.";
                    return RedirectToAction("Details", new { id = BookId });
                }
            }

            // שמירת המשוב
            var feedback = new Feedback
            {
                UserId = userId,
                BookId = BookId, // דירוג רכישה לא מחובר לספר
                Content = Content,
                Rating = Rating,
                FeedbackDate = DateTime.Now,
                IsPurchaseFeedback = IsPurchaseFeedback
            };

            db.Feedbacks.Add(feedback);
            db.SaveChanges();

            TempData["Success"] = "המשוב שלך נשמר בהצלחה!";
            return RedirectToAction(IsPurchaseFeedback ? "Index" : "Details", new { id = BookId });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddServiceFeedback(string Content, int Rating)
        {
            var userId = GetLoggedInUserId();

            if (userId <= 0)
            {
                TempData["Error"] = "משתמש לא מחובר.";
                return RedirectToAction("Index");
            }

            // Check if the user has ever made a payment in the Payments table
            bool hasPayments = db.Payments.Any(p => p.UserId == userId);

            if (!hasPayments)
            {
                TempData["Error"] = "רק משתמשים שרכשו ספר מסויים מהאתר יכולים להוסיף חוות דעת.";
                return RedirectToAction("Index");
            }

            // Save general feedback
            var feedback = new Feedback
            {
                UserId = userId,
                Content = Content,
                Rating = Rating,
                FeedbackDate = DateTime.Now,
                IsPurchaseFeedback = true
            };

            db.Feedbacks.Add(feedback);
            db.SaveChanges();

            TempData["Success"] = "חוות הדעת שלך נשמרה בהצלחה!";
            return RedirectToAction("Index");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BuyNow(int bookId)
        {
            var userId = GetLoggedInUserId();

            // בדיקה אם המשתמש מחובר
            if (userId <= 0)
            {
                TempData["Error"] = "עליך להתחבר כדי לבצע רכישה.";
                return RedirectToAction("Login", "Account");
            }

            // בדיקה אם הספר קיים
            var book = db.Books.FirstOrDefault(b => b.BookId == bookId);
            if (book == null)
            {
                TempData["Error"] = "הספר לא נמצא.";
                return RedirectToAction("Index");
            }

            // בדיקה אם הספר כבר נמצא בספרייה האישית של המשתמש
            bool isInLibrary = db.Purchases.Any(p => p.UserId == userId && p.BookId == bookId) ||
                               db.Borrows.Any(b => b.UserId == userId && b.BookId == bookId && !b.IsReturned);

            if (isInLibrary)
            {
                TempData["Error"] = "הספר כבר נמצא בספרייה האישית שלך";
                return RedirectToAction("Index");
            }

            // הוספת הספר ישירות לעגלת הקניות
            var cartItem = new CartItem
            {
                UserId = userId,
                BookId = bookId,
                Quantity = 1, // ספר אחד כברירת מחדל
                TransactionType="buy"
            };

            db.CartItems.Add(cartItem);
            db.SaveChanges();

            // הפניה לדף התשלום (Checkout)
            return RedirectToAction("Checkout", "CartItems", new { bookId, transactionType = "buy" });
        }


    }
}

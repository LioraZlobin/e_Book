using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using eBookLibrary.Models;
using e_Book.Models;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using e_Book.Services;
using System.Text;

namespace e_Book.Controllers
{
    [RequireHttps]
    public class CartItemsController : Controller
    {
        private LibraryDbContext db = new LibraryDbContext();
        private readonly EmailService _emailService = new EmailService();

        // GET: CartItems
        public ActionResult Index()
        {
            var userId = GetLoggedInUserId(); // פונקציה שמחזירה את מזהה המשתמש המחובר
            if (userId <= 0)
            {
                TempData["Error"] = "משתמש לא מחובר.";
                return RedirectToAction("Login", "Account");
            }

            var cartItems = db.CartItems
                              .Include(c => c.Book)
                              .Where(c => c.UserId == userId) // סינון לפי המשתמש המחובר
                              .ToList();

            return View(cartItems);
        }
        [HttpPost]
        public ActionResult GetBookPrice(int cartItemId, string transactionType)
        {
            try
            {
                var cartItem = db.CartItems.Include(c => c.Book).FirstOrDefault(c => c.CartItemId == cartItemId);
                if (cartItem == null)
                {
                    return Json(new { success = false, message = "הפריט לא נמצא בעגלה." });
                }

                if (transactionType == "buy")
                {
                    cartItem.TransactionType = "buy";
                }
                else if (transactionType == "borrow")
                {
                    cartItem.TransactionType = "borrow";
                }
                db.SaveChanges();

                var grandTotal = db.CartItems.Where(c => c.UserId == cartItem.UserId)
                                .Sum(c => (c.TransactionType == "buy" ? c.Book.PriceBuy : c.Book.PriceBorrow) * c.Quantity);

                return Json(new
                {
                    success = true,
                    price = (cartItem.TransactionType == "buy" ? cartItem.Book.PriceBuy : cartItem.Book.PriceBorrow).ToString("C"),
                    total = ((cartItem.TransactionType == "buy" ? cartItem.Book.PriceBuy : cartItem.Book.PriceBorrow) * cartItem.Quantity).ToString("C"),
                    grandTotal = grandTotal.ToString("C")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"שגיאה: {ex.Message}" });
            }
        }


        // פונקציה לדוגמה לקבלת מזהה המשתמש המחובר
        private int GetLoggedInUserId()
        {
            var userEmail = User.Identity.Name; // נניח ששם המשתמש הוא ה-Email
            var user = db.Users.FirstOrDefault(u => u.Email == userEmail);
            return user?.UserId ?? 0; // אם המשתמש לא נמצא, נחזיר 0
        }


        // GET: CartItems/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CartItem cartItem = db.CartItems.Find(id);
            if (cartItem == null)
            {
                return HttpNotFound();
            }
            return View(cartItem);
        }

        // GET: CartItems/Create
        public ActionResult Create()
        {
            ViewBag.BookId = new SelectList(db.Books, "BookId", "Title");
            ViewBag.UserId = new SelectList(db.Users, "UserId", "Name");
            return View();
        }

        // POST: CartItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CartItemId,UserId,BookId,Quantity")] CartItem cartItem)
        {
            if (ModelState.IsValid)
            {
                db.CartItems.Add(cartItem);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.BookId = new SelectList(db.Books, "BookId", "Title", cartItem.BookId);
            ViewBag.UserId = new SelectList(db.Users, "UserId", "Name", cartItem.UserId);
            return View(cartItem);
        }

        // GET: CartItems/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CartItem cartItem = db.CartItems.Find(id);
            if (cartItem == null)
            {
                return HttpNotFound();
            }
            ViewBag.BookId = new SelectList(db.Books, "BookId", "Title", cartItem.BookId);
            ViewBag.UserId = new SelectList(db.Users, "UserId", "Name", cartItem.UserId);
            return View(cartItem);
        }

        // POST: CartItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CartItemId,UserId,BookId,Quantity")] CartItem cartItem)
        {
            if (ModelState.IsValid)
            {
                db.Entry(cartItem).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.BookId = new SelectList(db.Books, "BookId", "Title", cartItem.BookId);
            ViewBag.UserId = new SelectList(db.Users, "UserId", "Name", cartItem.UserId);
            return View(cartItem);
        }

        // GET: CartItems/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CartItem cartItem = db.CartItems.Find(id);
            if (cartItem == null)
            {
                return HttpNotFound();
            }
            return View(cartItem);
        }

        // POST: CartItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CartItem cartItem = db.CartItems.Find(id);
            db.CartItems.Remove(cartItem);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult Checkout()
        {
            var userId = GetLoggedInUserId(); // בדיקת מזהה המשתמש המחובר
            if (userId <= 0)
            {
                TempData["Error"] = "משתמש לא מחובר.";
                return RedirectToAction("Login", "Account");
            }

            var cartItems = db.CartItems
                              .Include(c => c.Book)
                              .Where(c => c.UserId == userId)
                              .ToList();


            return View(cartItems);
        }

        // הוספת ספר לעגלה
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddToCart(int bookId, string transactionType)
        {
            var userId = GetLoggedInUserId();

            if (userId <= 0)
            {
                TempData["Error"] = "משתמש לא מחובר.";
                return RedirectToAction("Login", "Account");
            }

            var book = db.Books.FirstOrDefault(b => b.BookId == bookId);
            var user = db.Users.FirstOrDefault(u => u.UserId == userId);

            if (book == null)
            {
                TempData["Error"] = "הספר לא נמצא.";
                return RedirectToAction("Index", "Books");
            }

            // אם המשתמש בחר "כן" או "לא" להצטרף לרשימת ההמתנה
            if (Request.Form["confirmWaitList"] == "yes")
            {
                var waitingList = db.WaitingLists.Where(w => w.BookId == bookId).OrderBy(w => w.Position).ToList();
                var position = waitingList.Count + 1;

                db.WaitingLists.Add(new WaitingList
                {
                    UserId = userId,
                    BookId = bookId,
                    Position = position,
                    AddedDate = DateTime.Now
                });

                db.SaveChanges();
                TempData["Success"] = $"נוספת לרשימת ההמתנה בתור מספר {position}.";
                return RedirectToAction("Index", "Books");
            }
            else if (Request.Form["confirmWaitList"] == "no")
            {
                TempData["Info"] = "לא נוספת לרשימת ההמתנה.";
                return RedirectToAction("Index", "Books");
            }

            // בדיקה אם הספר כבר בעגלה
            var existingItem = db.CartItems.FirstOrDefault(c => c.UserId == userId && c.BookId == bookId);
            if (existingItem != null)
            {
                TempData["Error"] = "הספר כבר נמצא בעגלת הקניות שלך.";
                return RedirectToAction("Index", "Books");
            }

            // בדיקה אם הספר כבר נמצא בספריה האישית (נרכש או מושאל)
            var alreadyOwned = db.Borrows.Any(b => b.UserId == userId && b.BookId == bookId && !b.IsReturned) ||
                               db.Purchases.Any(p => p.UserId == userId && p.BookId == bookId);

            if (alreadyOwned)
            {
                TempData["Error"] = "הספר כבר נמצא בספריה האישית שלך.";
                return RedirectToAction("Index", "Books");
            }

            // השאלה עם בדיקת עותקים זמינים
            if (transactionType == "borrow")
            {
                var activeBorrowsCount = db.Borrows.Count(b => b.UserId == userId && !b.IsReturned);
                var cartBorrowsCount = db.CartItems.Count(c => c.UserId == userId && c.TransactionType == "borrow");
                int totalBorrows = activeBorrowsCount + cartBorrowsCount;

                if (totalBorrows >= 3)
                {
                    TempData["Error"] = "לא ניתן להשאיל יותר מ-3 ספרים בו-זמנית (כולל ספרים בעגלה).";
                    return RedirectToAction("Index", "Books");
                }

                if (book.IsBorrowable)
                {
                    if (book.AvailableCopies > 1)
                    {
                        // הוספה רגילה לעגלה
                        db.CartItems.Add(new CartItem
                        {
                            UserId = userId,
                            BookId = bookId,
                            Quantity = 1,
                            TransactionType = "borrow"
                        });
                        db.SaveChanges();
                        TempData["Success"] = $"הספר '{book.Title}' נוסף לעגלה להשאלה.";
                    }
                    else if (book.AvailableCopies == 1)
                    {
                        var waitingList = db.WaitingLists.Where(w => w.BookId == bookId).OrderBy(w => w.Position).ToList();
                        var userInWaitingList = waitingList.FirstOrDefault(w => w.UserId == userId);

                        if (!waitingList.Any())
                        {
                            // אין רשימת המתנה
                            db.CartItems.Add(new CartItem
                            {
                                UserId = userId,
                                BookId = bookId,
                                Quantity = 1,
                                TransactionType = "borrow"
                            });
                            db.SaveChanges();
                            TempData["Success"] = $"הספר '{book.Title}' נוסף לעגלה להשאלה.";
                        }
                        else
                        {
                            if (userInWaitingList != null)
                            {
                                if(userInWaitingList.ExpirationTime.HasValue && userInWaitingList.ExpirationTime < DateTime.Now)
                                {
                                    db.WaitingLists.Remove(userInWaitingList);
                                    db.SaveChanges();
                                    TempData["error"] = "תוקף ההמתנה שלך פג.";
                                    return RedirectToAction("Index", "Books");
                                }
                                else if (waitingList.Take(3).Any(w => w.UserId == userId))
                                {
                                    db.CartItems.Add(new CartItem
                                    {
                                        UserId = userId,
                                        BookId = bookId,
                                        Quantity = 1,
                                        TransactionType = "borrow"
                                    });
                                    // הסרת המשתמש מרשימת ההמתנה
                                    db.WaitingLists.Remove(userInWaitingList);

                                    // עדכון המיקומים ברשימת ההמתנה
                                    foreach (var item in waitingList.Where(w => w.Position > userInWaitingList.Position))
                                    {
                                        item.Position--;
                                    }
                                    db.SaveChanges();
                                    TempData["Success"] = $"הספר '{book.Title}' נוסף לעגלה להשאלה.";
                                }
                                else
                                {
                                    TempData["Error"] = "אתה נמצא ברשימת ההמתנה אך אינך בין השלושה הראשונים.";
                                }
                            }
                            else
                            {
                                // הוספת המשתמש לרשימת ההמתנה
                                var position = waitingList.Count + 1;
                                db.WaitingLists.Add(new WaitingList
                                {
                                    UserId = userId,
                                    BookId = bookId,
                                    Position = position,
                                    AddedDate = DateTime.Now
                                });
                                db.SaveChanges();
                                TempData["Info"] = $"הספר אינו זמין כרגע. אתה במקום {position} ברשימת ההמתנה.";
                            }
                        }
                    }
                    else
                    {
                        // עותקים שווים ל-0
                        var waitingList = db.WaitingLists.Where(w => w.BookId == bookId).OrderBy(w => w.Position).ToList();
                        var userInWaitingList = waitingList.FirstOrDefault(w => w.UserId == userId);

                        if (userInWaitingList != null)
                        {
                            TempData["Info"] = $"אתה כבר נמצא ברשימת ההמתנה במקום {userInWaitingList.Position}.";
                        }
                        else
                        {
                            var position = waitingList.Count + 1;
                            int estimatedDays = waitingList.Count * 30; // כל משתמש מחזיק עד 30 ימים
                            TempData["Info"] = $"הספר אינו זמין כרגע. ישנם {waitingList.Count} אנשים ברשימת ההמתנה. זמן משוער לחזרה: {estimatedDays} ימים.";
                            TempData["Prompt"] = "האם תרצה להצטרף לרשימת ההמתנה?";
                            TempData["BookId"] = bookId; // שמירת המידע של הספר להצגה ב-View

                            // הוספת המשתמש לרשימת ההמתנה אם בחר "כן"
                            if (Request.Form["confirmWaitList"] == "yes")
                            {
                                db.WaitingLists.Add(new WaitingList
                                {
                                    UserId = userId,
                                    BookId = bookId,
                                    Position = position,
                                    AddedDate = DateTime.Now
                                });
                                db.SaveChanges();
                                TempData["Success"] = $"נוספת לרשימת ההמתנה בתור מספר {position}.";
                            }
                            else if (Request.Form["confirmWaitList"] == "no")
                            {
                                TempData["Info"] = "לא נוספת לרשימת ההמתנה.";
                            }
                        }
                    }
                }
                else
                {
                    TempData["Error"] = $"הספר '{book.Title}' אינו ניתן להשאלה.";
                }
            }

            else if (transactionType == "buy")
            {
                db.CartItems.Add(new CartItem
                {
                    UserId = userId,
                    BookId = bookId,
                    Quantity = 1,
                    TransactionType = "buy"
                });
                db.SaveChanges();
                TempData["Success"] = $"הספר '{book.Title}' נוסף לעגלה לרכישה.";
            }

            return RedirectToAction("Index", "Books");
        }




        [HttpPost]
        public ActionResult EditQuantity(int cartItemId, int delta)
        {
            var cartItem = db.CartItems.Find(cartItemId);
            if (cartItem != null)
            {
                // אם הכמות כבר 1 והמשתמש מנסה להגדיל, אין שינוי
                if (delta > 0 && cartItem.Quantity >= 1)
                {
                    return Json(new { success = false, message = "לא ניתן להגדיל את הכמות מעל 1." });
                }

                cartItem.Quantity += delta;

                // מחיקת הפריט אם הכמות קטנה או שווה ל-0
                if (cartItem.Quantity <= 0)
                {
                    db.CartItems.Remove(cartItem);
                }

                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "הפריט לא נמצא בעגלה." });
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing && db != null)
            {
                db.Dispose();
                db = null; // וודא שהאובייקט לא נגיש עוד לאחר Dispose
            }
            base.Dispose(disposing);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessPayment() // לתשלום בכרטיס אשראי
        {
            int userId = GetLoggedInUserId();
            if (userId <= 0)
            {
                TempData["Error"] = "עליך להתחבר כדי לבצע תשלום.";
                return RedirectToAction("Login", "Account");
            }

            // שליפת כל הפריטים בעגלה של המשתמש
            var cartItems = db.CartItems.Include(c => c.Book).Where(c => c.UserId == userId).ToList();

            if (cartItems.Count == 0)
            {
                TempData["Error"] = "עגלת הקניות ריקה.";
                return RedirectToAction("Index", "CartItems");
            }

            decimal totalAmount = 0;

            foreach (var item in cartItems)
            {
                var book = db.Books.Find(item.BookId);
                if (book == null)
                {
                    TempData["Error"] = $"לא נמצא ספר עם מזהה {item.BookId}.";
                    continue;
                }



                // שמירת השאלה או רכישה
                if (item.TransactionType == "borrow")
                {

                    // בדיקת מגבלת 3 השאלות
                    var activeBorrowsCount = db.Borrows.Count(b => b.UserId == userId && !b.IsReturned);
                    if (activeBorrowsCount >= 3)
                    {
                        TempData["Error"] = "לא ניתן להשאיל יותר מ-3 ספרים בו-זמנית.";
                        return RedirectToAction("Index", "CartItems");
                    }

                    if (book.AvailableCopies <= 0)
                    {
                        TempData["Error"] = $"אין מספיק עותקים זמינים לספר '{book.Title}'.";
                        return RedirectToAction("Index", "CartItems");
                    }

                    db.Borrows.Add(new Borrow
                    {
                        UserId = userId,
                        BookId = book.BookId,
                        BorrowDate = DateTime.Now,
                        DueDate = DateTime.Now.AddDays(30),
                        IsReturned = false
                    });
                    //db.Borrows.Add(borrow);
                    totalAmount += book.PriceBorrow * item.Quantity; // סכום כולל להשאלה
                }
                else if (item.TransactionType == "buy")
                {
                    db.Purchases.Add(new Purchase
                    {
                        UserId = userId,
                        BookId = book.BookId,
                        PurchasePrice = book.PriceBuy,
                        PurchaseDate = DateTime.Now
                    });
                    //db.Purchases.Add(purchase);
                    totalAmount += book.PriceBuy * item.Quantity; // סכום כולל לרכישה
                }

                if (item.TransactionType == "borrow")
                {
                    // עדכון מלאי
                    book.AvailableCopies--;
                }

                // שמירת תשלום
                db.Payments.Add(new Payment
                {
                    UserId = userId,
                    BookId = item.BookId,
                    Amount = item.TransactionType == "buy" ? book.PriceBuy : book.PriceBorrow,
                    PaymentMethod = "CreditCard",
                    PaymentDate = DateTime.Now
                });

            }



            // מחיקת כל הפריטים מהעגלה
            db.CartItems.RemoveRange(cartItems);
            db.SaveChanges();

            // שליחת מייל למשתמש
            var user = db.Users.FirstOrDefault(u => u.UserId == userId);
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                string subject = "תשלום הושלם בהצלחה";
                string body = $@"
                     <div dir='rtl' style='text-align:right; font-family:Arial, sans-serif;'>
                            <h2>תשלום הושלם בהצלחה</h2>
                            <p>שלום {user.Name},</p>
                            <p>תודה על רכישתך/השאלתך בספרייה. להלן פרטי ההזמנה שלך:</p>
                            <ul>";

                foreach (var item in cartItems)
                {
                    var book = db.Books.Find(item.BookId);
                    if (book != null)
                    {
                        body += $@"
                    <li>
                        <strong>שם הספר:</strong> {book.Title}<br/>
                        <strong>סוג העסקה:</strong> {(item.TransactionType == "buy" ? "רכישה" : "השאלה")}<br/>
                        <strong>כמות:</strong> {item.Quantity}<br/>
                        <strong>סכום:</strong> {(item.TransactionType == "buy" ? book.PriceBuy : book.PriceBorrow).ToString("C")}
                    </li>";
                    }
                }

                body += $@"
                    </ul>
                    <p><strong>סה&quot;כ לתשלום:</strong> {totalAmount.ToString("C")}</p>
                    <p>תודה,<br/>צוות הספרייה</p>
                 </div>";

                _emailService.SendEmail(user.Email, subject, body);
            }

            TempData["Success"] = "התשלום הושלם בהצלחה! הספרים נוספו לרשומות שלך.";
            return RedirectToAction("Index", "Books");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveFromCart(int cartItemId)
        {
            var cartItem = db.CartItems.Include(c => c.Book).FirstOrDefault(c => c.CartItemId == cartItemId);

            if (cartItem == null)
            {
                TempData["Error"] = "הפריט לא נמצא בעגלת הקניות.";
                return RedirectToAction("Index");
            }

            var book = cartItem.Book;
            db.CartItems.Remove(cartItem);

            db.SaveChanges();
            TempData["Success"] = "הפריט הוסר מהעגלה.";
            return RedirectToAction("Index");
        }



        [HttpGet]
        public ActionResult FinalizePayment()// לתשלום דרך פייפאל
        {
            try
            {
                var userId = GetLoggedInUserId();

                if (userId <= 0)
                {
                    TempData["Error"] = "משתמש לא מחובר.";
                    return RedirectToAction("Login", "Account");
                }

                // שליפת כל הפריטים בעגלה של המשתמש
                var cartItems = db.CartItems.Include(c => c.Book).Where(c => c.UserId == userId).ToList();

                if (!cartItems.Any())
                {
                    TempData["Error"] = "עגלת הקניות ריקה.";
                    return RedirectToAction("Index", "CartItems");
                }

                decimal totalAmount = 0;

                foreach (var item in cartItems)
                {
                    var book = db.Books.FirstOrDefault(b => b.BookId == item.BookId);
                    if (book == null)
                    {
                        TempData["Error"] = $"לא נמצא ספר עם מזהה {item.BookId}.";
                        continue;
                    }

                    // הוספה לטבלאות רכישות או השאלות
                    if (item.TransactionType == "buy")
                    {
                        db.Purchases.Add(new Purchase
                        {
                            UserId = userId,
                            BookId = item.BookId,
                            PurchasePrice = book.PriceBuy,
                            PurchaseDate = DateTime.Now
                        });

                        totalAmount += book.PriceBuy * item.Quantity; // סכום כולל לרכישה
                    }
                    else if (item.TransactionType == "borrow")
                    {

                        // בדיקת מגבלת 3 השאלות
                        var activeBorrowsCount = db.Borrows.Count(b => b.UserId == userId && !b.IsReturned);
                        if (activeBorrowsCount >= 3)
                        {
                            TempData["Error"] = "לא ניתן להשאיל יותר מ-3 ספרים בו-זמנית.";
                            return RedirectToAction("Index", "CartItems");
                        }

                        if (book.AvailableCopies <= 0)
                        {
                            TempData["Error"] = $"אין מספיק עותקים זמינים לספר '{book.Title}'.";
                            return RedirectToAction("Index", "CartItems");
                        }

                        db.Borrows.Add(new Borrow
                        {
                            UserId = userId,
                            BookId = item.BookId,
                            BorrowDate = DateTime.Now,
                            DueDate = DateTime.Now.AddDays(30),
                            IsReturned = false
                        });

                        totalAmount += book.PriceBorrow * item.Quantity; // סכום כולל להשאלה
                    }

                    if (item.TransactionType == "borrow")
                    {
                        // עדכון מלאי
                        book.AvailableCopies--;
                    }

                    // שמירת תשלום
                    db.Payments.Add(new Payment
                    {
                        UserId = userId,
                        BookId = item.BookId,
                        Amount = item.TransactionType == "buy" ? book.PriceBuy : book.PriceBorrow,
                        PaymentMethod = "PayPal",
                        PaymentDate = DateTime.Now
                    });
                }

                // מחיקת כל הפריטים מהעגלה
                db.CartItems.RemoveRange(cartItems);

                // שמירה של כל השינויים במסד הנתונים
                db.SaveChanges();

                // שליחת מייל למשתמש עם פרטי ההזמנה
                var user = db.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    string subject = "תשלום הושלם בהצלחה";
                    string body = $@"
                     <div dir='rtl' style='text-align:right; font-family:Arial, sans-serif;'>
                            <h2>תשלום הושלם בהצלחה</h2>
                            <p>שלום {user.Name},</p>
                            <p>תודה על רכישתך/השאלתך בספרייה. להלן פרטי ההזמנה שלך:</p>
                            <ul>";

                    foreach (var item in cartItems)
                    {
                        var book = db.Books.Find(item.BookId);
                        if (book != null)
                        {
                            body += $@"
                    <li>
                        <strong>שם הספר:</strong> {book.Title}<br/>
                        <strong>סוג העסקה:</strong> {(item.TransactionType == "buy" ? "רכישה" : "השאלה")}<br/>
                        <strong>כמות:</strong> {item.Quantity}<br/>
                        <strong>סכום:</strong> {(item.TransactionType == "buy" ? book.PriceBuy : book.PriceBorrow).ToString("C")}
                    </li>";
                        }
                    }

                    body += $@"
                    </ul>
                    <p><strong>סה&quot;כ לתשלום:</strong> {totalAmount.ToString("C")}</p>
                    <p>תודה,<br/>צוות הספרייה</p>
                 </div>";

                    _emailService.SendEmail(user.Email, subject, body);
                }

                TempData["Success"] = "התשלום הושלם בהצלחה! הספרים נוספו לרשומות שלך.";
                return RedirectToAction("Index", "Books");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"שגיאה בתהליך התשלום: {ex.Message}";
                return RedirectToAction("Index", "CartItems");
            }
        }




    }
}

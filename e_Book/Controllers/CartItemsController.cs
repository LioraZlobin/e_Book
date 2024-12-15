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
    public class CartItemsController : Controller
    {
        private LibraryDbContext db = new LibraryDbContext();
        private readonly EmailService _emailService= new EmailService();

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
        public ActionResult AddToCart(int bookId)
        {
            var userId = GetLoggedInUserId();

            if (userId <= 0)
            {
                TempData["Error"] = "משתמש לא מחובר.";
                return RedirectToAction("Login", "Account");
            }

            var book = db.Books.FirstOrDefault(b => b.BookId == bookId);

            if (book == null)
            {
                TempData["Error"] = "הספר לא נמצא.";
                return RedirectToAction("Index", "Books");
            }

            if (book.AvailableCopies <= 0)
            {
                // הוספת המשתמש לרשימת המתנה אם אין עותקים זמינים
                var waitingUser = new WaitingList
                {
                    UserId = userId,
                    BookId = bookId,
                    AddedDate = DateTime.Now
                };

                db.WaitingLists.Add(waitingUser);
                db.SaveChanges();

                TempData["Error"] = "אין עותקים זמינים לספר. נוספת לרשימת ההמתנה.";
                return RedirectToAction("Index", "Books");
            }

            // בדיקה אם הספר כבר בעגלה
            var existingItem = db.CartItems.FirstOrDefault(c => c.UserId == userId && c.BookId == bookId);

            if (existingItem != null)
            {
                TempData["Error"] = "הספר כבר נמצא בעגלת הקניות שלך.";
                return RedirectToAction("Index", "Books");
            }

            // הוספת הספר לעגלה ללא שינוי במלאי
            var newItem = new CartItem
            {
                UserId = userId,
                BookId = bookId,
                Quantity = 1,
                TransactionType = "buy" // או "borrow" בהתאם לספר
            };

            db.CartItems.Add(newItem);
            db.SaveChanges();

            TempData["Success"] = "הספר נוסף לעגלת הקניות בהצלחה!";
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
                var book = db.Books.FirstOrDefault(b => b.BookId == item.BookId);
                if (book == null)
                {
                    TempData["Error"] = $"לא נמצא ספר עם מזהה {item.BookId}.";
                    continue;
                }

                if (book.AvailableCopies <= 0)
                {
                    TempData["Error"] = $"אין מספיק עותקים זמינים לספר '{book.Title}'.";
                    return RedirectToAction("Index", "CartItems");
                }

                // שמירת השאלה או רכישה
                if (item.TransactionType == "borrow")
                {
                    var borrow = new Borrow
                    {
                        UserId = userId,
                        BookId = item.BookId,
                        BorrowDate = DateTime.Now,
                        DueDate = DateTime.Now.AddDays(30),
                        IsReturned = false
                    };
                    db.Borrows.Add(borrow);
                    totalAmount += book.PriceBorrow * item.Quantity; // סכום כולל להשאלה
                }
                else if (item.TransactionType == "buy")
                {
                    var purchase = new Purchase
                    {
                        UserId = userId,
                        BookId = item.BookId,
                        PurchasePrice = book.PriceBuy,
                        PurchaseDate = DateTime.Now
                    };
                    db.Purchases.Add(purchase);
                    totalAmount += book.PriceBuy * item.Quantity; // סכום כולל לרכישה
                }

                // עדכון מלאי
                book.AvailableCopies -= item.Quantity;
            }

            // מחיקת כל הפריטים מהעגלה
            db.CartItems.RemoveRange(cartItems);
            db.SaveChanges();

            // שליחת מייל למשתמש
            var user = db.Users.FirstOrDefault(u => u.UserId == userId);
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                string subject = "תשלום הושלם בהצלחה";
                StringBuilder body = new StringBuilder();
                body.AppendLine($"שלום {user.Name},<br/><br/>");
                body.AppendLine("תודה על רכישתך/השאלתך בספרייה. להלן פרטי ההזמנה שלך:<br/><br/>");

                foreach (var item in cartItems)
                {
                    var book = db.Books.Find(item.BookId); // שליפת הספר באופן ידני
                    if (book != null)
                    {
                        body.AppendLine($"<strong>שם הספר:</strong> {book.Title}, ");
                        body.AppendLine($"<strong>סוג העסקה:</strong> {(item.TransactionType == "buy" ? "רכישה" : "השאלה")}, ");
                        body.AppendLine($"<strong>כמות:</strong> {item.Quantity}, ");
                        body.AppendLine($"<strong>סכום:</strong> {(item.TransactionType == "buy" ? book.PriceBuy : book.PriceBorrow).ToString("C")}<br/>");
                    }
                    else
                    {
                        body.AppendLine($"<strong>שם הספר:</strong> ספר לא נמצא.<br/>");
                    }
                }


                body.AppendLine($"<br/><strong>סה\"כ לתשלום:</strong> {totalAmount.ToString("C")}<br/><br/>");
                body.AppendLine("תודה,<br/>צוות הספרייה.");

                // שליחת המייל
                _emailService.SendEmail(user.Email, subject, body.ToString());
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
                return RedirectToAction("Index", "CartItems");
            }

            // שמירת המידע על הספר והמשתמש
            var book = cartItem.Book;
            var userId = cartItem.UserId;

            // הסרת הפריט מהעגלה
            db.CartItems.Remove(cartItem);

            // בדיקה אם המשתמש הגיע מרשימת המתנה
            var waitingUser = db.WaitingLists
                                .Where(w => w.BookId == book.BookId)
                                .OrderBy(w => w.Position) // לפי המיקום בתור
                                .FirstOrDefault(w => w.UserId == userId);

            // אם המשתמש קיבל את הספר מתוך רשימת המתנה
            if (waitingUser != null)
            {
                // הסרת המשתמש מרשימת ההמתנה
                db.WaitingLists.Remove(waitingUser);
            }

            db.SaveChanges();

            TempData["Success"] = "הפריט הוסר מהעגלה.";
            return RedirectToAction("Index", "CartItems");
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

                    // עדכון עותקים זמינים
                    book.AvailableCopies -= item.Quantity;

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
                    StringBuilder body = new StringBuilder();
                    body.AppendLine($"שלום {user.Name},<br/><br/>");
                    body.AppendLine("תודה על רכישתך/השאלתך בספרייה. להלן פרטי ההזמנה שלך:<br/><br/>");

                    foreach (var item in cartItems)
                    {
                        var book = db.Books.Find(item.BookId); // שליפת הספר באופן ידני
                        if (book != null)
                        {
                            body.AppendLine($"<strong>שם הספר:</strong> {book.Title}, ");
                            body.AppendLine($"<strong>סוג העסקה:</strong> {(item.TransactionType == "buy" ? "רכישה" : "השאלה")}, ");
                            body.AppendLine($"<strong>כמות:</strong> {item.Quantity}, ");
                            body.AppendLine($"<strong>סכום:</strong> {(item.TransactionType == "buy" ? book.PriceBuy : book.PriceBorrow).ToString("C")}<br/>");
                        }
                        else
                        {
                            body.AppendLine($"<strong>שם הספר:</strong> ספר לא נמצא.<br/>");
                        }
                    }


                    body.AppendLine($"<br/><strong>סה\"כ לתשלום:</strong> {totalAmount.ToString("C")}<br/><br/>");
                    body.AppendLine("תודה,<br/>צוות הספרייה.");

                    // שליחת המייל
                    _emailService.SendEmail(user.Email, subject, body.ToString());
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

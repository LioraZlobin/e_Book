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
    [RequireHttps]
    public class PurchasesController : Controller
    {
        private LibraryDbContext db = new LibraryDbContext();

        // GET: Purchases
        public ActionResult Index()
        {
            var purchases = db.Purchases.Include(p => p.Book).Include(p => p.User);
            return View(purchases.ToList());
        }

        // GET: Purchases/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Purchase purchase = db.Purchases.Find(id);
            if (purchase == null)
            {
                return HttpNotFound();
            }
            return View(purchase);
        }

        // GET: Purchases/Create
        public ActionResult Create()
        {
            ViewBag.BookId = new SelectList(db.Books, "BookId", "Title");
            ViewBag.UserId = new SelectList(db.Users, "UserId", "Name");
            return View();
        }

        // POST: Purchases/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PurchaseId,UserId,BookId,PurchasePrice,PurchaseDate")] Purchase purchase)
        {
            if (ModelState.IsValid)
            {
                db.Purchases.Add(purchase);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.BookId = new SelectList(db.Books, "BookId", "Title", purchase.BookId);
            ViewBag.UserId = new SelectList(db.Users, "UserId", "Name", purchase.UserId);
            return View(purchase);
        }

        // GET: Purchases/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Purchase purchase = db.Purchases.Find(id);
            if (purchase == null)
            {
                return HttpNotFound();
            }
            ViewBag.BookId = new SelectList(db.Books, "BookId", "Title", purchase.BookId);
            ViewBag.UserId = new SelectList(db.Users, "UserId", "Name", purchase.UserId);
            return View(purchase);
        }

        // POST: Purchases/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PurchaseId,UserId,BookId,PurchasePrice,PurchaseDate")] Purchase purchase)
        {
            if (ModelState.IsValid)
            {
                db.Entry(purchase).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.BookId = new SelectList(db.Books, "BookId", "Title", purchase.BookId);
            ViewBag.UserId = new SelectList(db.Users, "UserId", "Name", purchase.UserId);
            return View(purchase);
        }

        // GET: Purchases/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Purchase purchase = db.Purchases.Find(id);
            if (purchase == null)
            {
                return HttpNotFound();
            }
            return View(purchase);
        }

        // POST: Purchases/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Purchase purchase = db.Purchases.Find(id);
            db.Purchases.Remove(purchase);
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
        public ActionResult Purchase(int bookId)
        {
            var userId = GetCurrentUserId();

            // בדיקה אם הספר כבר נרכש
            var isAlreadyPurchased = db.Purchases.Any(p => p.BookId == bookId && p.UserId == userId);
            if (isAlreadyPurchased)
            {
                TempData["Error"] = "הספר כבר נרכש על ידך.";
                return RedirectToAction("Index", "Books");
            }

            // שליפת פרטי הספר
            var book = db.Books.FirstOrDefault(b => b.BookId == bookId);
            if (book == null)
            {
                TempData["Error"] = "הספר לא נמצא.";
                return RedirectToAction("Index", "Books");
            }

            // הוספת פרטי הרכישה
            var purchase = new Purchase
            {
                UserId = userId,
                BookId = bookId,
                PurchasePrice = book.PriceBuy,
                PurchaseDate = DateTime.Now
            };

            db.Purchases.Add(purchase);
            db.SaveChanges();

            TempData["Success"] = "הספר נרכש בהצלחה!";
            return RedirectToAction("Index", "Books");
        }

        private int GetCurrentUserId()
        {
            // בדיקה אם המשתמש מחובר
            if (User.Identity.IsAuthenticated)
            {
                // שליפת מזהה המשתמש מתוך בסיס הנתונים לפי המייל (או שם משתמש)
                var user = db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
                return user?.UserId ?? 0; // מחזיר את מזהה המשתמש או 0 אם לא נמצא
            }

            return 0; // במקרה שהמשתמש אינו מחובר
        }



    }
}

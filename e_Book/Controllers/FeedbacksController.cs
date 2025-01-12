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
    public class FeedbacksController : Controller
    {
        private LibraryDbContext db = new LibraryDbContext();

        // GET: Feedbacks
        public ActionResult Index()
        {
            return View(db.Feedbacks.ToList());
        }

        // GET: Feedbacks/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Feedback feedback = db.Feedbacks.Find(id);
            if (feedback == null)
            {
                return HttpNotFound();
            }
            return View(feedback);
        }

        // GET: Feedbacks/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Feedbacks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "FeedbackId,UserId,BookId,Content,Rating,FeedbackDate")] Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                db.Feedbacks.Add(feedback);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(feedback);
        }

        // GET: Feedbacks/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Feedback feedback = db.Feedbacks.Find(id);
            if (feedback == null)
            {
                return HttpNotFound();
            }
            return View(feedback);
        }

        // POST: Feedbacks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "FeedbackId,UserId,BookId,Content,Rating,FeedbackDate")] Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                db.Entry(feedback).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(feedback);
        }

        // GET: Feedbacks/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Feedback feedback = db.Feedbacks.Find(id);
            if (feedback == null)
            {
                return HttpNotFound();
            }
            return View(feedback);
        }

        // POST: Feedbacks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Feedback feedback = db.Feedbacks.Find(id);
            db.Feedbacks.Remove(feedback);
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

        // GET: Feedbacks/AddFeedback
        public ActionResult AddFeedback(int bookId)
        {
            var book = db.Books.Find(bookId);
            if (book == null)
            {
                TempData["Error"] = "הספר לא נמצא.";
                return RedirectToAction("UserLibrary", "Borrows");
            }

            ViewBag.BookId = bookId;
            return View(new Feedback());
        }

        // POST: Feedbacks/AddFeedback
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddFeedback(Feedback feedback)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.BookId = feedback.BookId;
                return View(feedback);
            }

            // ודא ש-Session["UserId"] לא null
            if (Session["UserId"] == null)
            {
                TempData["Error"] = "חובה להתחבר כדי להוסיף חוות דעת.";
                return RedirectToAction("Login", "Account");
            }

            feedback.FeedbackDate = DateTime.Now;
            feedback.UserId = int.Parse(Session["UserId"].ToString()); // משתמש מחובר
            db.Feedbacks.Add(feedback);
            db.SaveChanges();

            TempData["Success"] = "חוות הדעת נוספה בהצלחה!";
            return RedirectToAction("UserLibrary", "Borrows");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitPurchaseFeedback(Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                feedback.UserId = int.Parse(Session["UserId"].ToString()); // מזהה המשתמש המחובר
                feedback.FeedbackDate = DateTime.Now; // תאריך הפידבק
                feedback.IsPurchaseFeedback = true; // סימון כחוות דעת על רכישה

                db.Feedbacks.Add(feedback);
                db.SaveChanges();

                TempData["Success"] = "תודה על חוות הדעת שלך על הרכישה!";
                return RedirectToAction("UserLibrary", "Borrows");
            }

            TempData["Error"] = "אירעה שגיאה, אנא נסה שנית.";
            return View("FeedbackForm", feedback);
        }

        [HttpGet]
        public ActionResult AddPurchaseFeedback(int bookId)
        {
            var feedback = new Feedback
            {
                BookId = bookId,
                IsPurchaseFeedback = true
            };
            return View(feedback);
        }

    }
}


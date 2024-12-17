using System.Linq;
using System.Web.Mvc;
using e_Book.Models;
using e_Book.Services;
using eBookLibrary.Models;

public class HomeController : Controller
{
    private LibraryDbContext db = new LibraryDbContext();

    public ActionResult About()
    {
        ViewBag.TotalBooks = db.Books.Count();
        ViewBag.ActiveUsers = db.Users.Count();
        ViewBag.TotalTransactions = db.Borrows.Count() + db.Purchases.Count();
        return View();
    }

    public ActionResult Contact()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult SendMessage(string fullName, string email, string message)
    {
        if (!string.IsNullOrEmpty(fullName) && !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(message))
        {
            EmailService emailService = new EmailService();
            string subject = "הודעה חדשה מאתר הספרייה";

            // עיצוב HTML למייל עם תמיכה ב-RTL
            string body = $@"
            <div dir='rtl' style='text-align:right; font-family:Arial, sans-serif;'>
                <h2>הודעה חדשה מהאתר</h2>
                <p><strong>שם מלא:</strong> {fullName}</p>
                <p><strong>אימייל:</strong> {email}</p>
                <p><strong>הודעה:</strong></p>
                <p>{message}</p>
                <br/>
                <p>תודה,<br/>צוות ספריית eBook Library</p>
            </div>";

            emailService.SendEmail("admin@example.com", subject, body);

            TempData["Success"] = "ההודעה נשלחה בהצלחה! נחזור אליך בהקדם.";
        }
        else
        {
            TempData["Error"] = "נא למלא את כל השדות בטופס.";
        }
        return RedirectToAction("Contact");
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult SendFeedback(string Feedback)
    {
        if (!string.IsNullOrEmpty(Feedback))
        {
            EmailService emailService = new EmailService();
            string subject = "משוב חדש מהאתר";

            string body = $@"
            <div dir='rtl' style='text-align:right; font-family:Arial, sans-serif;'>
                <h2>משוב מהמשתמש</h2>
                <p>{Feedback}</p>
                <br/>
                <p>תודה,<br/>צוות ספריית eBook Library</p>
            </div>";

            emailService.SendEmail("admin@example.com", subject, body);

            TempData["Success"] = "תודה על המשוב!";
        }
        else
        {
            TempData["Error"] = "נא למלא את שדה המשוב.";
        }
        return RedirectToAction("Contact");
    }

}

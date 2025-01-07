using System.IO;
using System.Linq;
using System.Web.Mvc;
using iTextSharp.text;
using iTextSharp.text.pdf;
using eBookLibrary.Models;

namespace e_Book.Controllers
{
    public class DownloadController : Controller
    {
        private LibraryDbContext db = new LibraryDbContext();

        [HttpGet]
        public ActionResult DownloadFile(int bookId, string format)
        {
            // בדיקת משתמש מחובר
            var userId = GetLoggedInUserId();
            if (userId <= 0)
            {
                TempData["Error"] = "עליך להתחבר כדי להוריד את הספר.";
                return RedirectToAction("Login", "Account");
            }

            // בדיקה אם המשתמש רכש או השאיל את הספר
            bool hasAccess = db.Borrows.Any(b => b.UserId == userId && b.BookId == bookId && !b.IsReturned) ||
                             db.Purchases.Any(p => p.UserId == userId && p.BookId == bookId);

            if (!hasAccess)
            {
                TempData["Error"] = "רק משתמשים שרכשו או השאילו את הספר יכולים להוריד אותו.";
                return RedirectToAction("Details", "Books", new { id = bookId });
            }

            byte[] fileBytes;
            string contentType;
            string fileName;

            switch (format.ToLower())
            {
                case "pdf":
                    fileBytes = GenerateEmptyPdf();
                    contentType = "application/pdf";
                    fileName = "EmptyDocument.pdf";
                    break;

                case "epub":
                    fileBytes = GenerateEmptyEpub();
                    contentType = "application/epub+zip";
                    fileName = "EmptyDocument.epub";
                    break;

                case "fb2":
                    fileBytes = GenerateEmptyFb2();
                    contentType = "application/x-fictionbook+xml";
                    fileName = "EmptyDocument.fb2";
                    break;

                case "mobi":
                    fileBytes = GenerateEmptyMobi();
                    contentType = "application/x-mobipocket-ebook";
                    fileName = "EmptyDocument.mobi";
                    break;

                default:
                    return new HttpStatusCodeResult(400, "Unsupported format.");
            }

            return File(fileBytes, contentType, fileName);
        }

        private int GetLoggedInUserId()
        {
            var userEmail = User.Identity.Name; // משתמש מחובר לפי ה-Email
            var user = db.Users.FirstOrDefault(u => u.Email == userEmail);
            return user?.UserId ?? 0; // אם המשתמש לא נמצא, נחזיר 0
        }

        private byte[] GenerateEmptyPdf()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                iTextSharp.text.Document pdfDoc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4);
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                pdfDoc.Open();
                pdfDoc.Add(new iTextSharp.text.Paragraph("ספר PDF בפורמט "));
                pdfDoc.Close();
                writer.Close();
                return stream.ToArray();
            }
        }

        private byte[] GenerateEmptyEpub()
        {
            string content = "EPUB <?xml version=\"1.0\" encoding=\"utf-8\"?><package xmlns=\"http://www.idpf.org/2007/opf\" unique-identifier=\"bookid\"><metadata xmlns:dc=\"http://purl.org/dc/elements/1.1/\"></metadata><manifest></manifest><spine></spine></package>";
            return System.Text.Encoding.UTF8.GetBytes(content);
        }

        private byte[] GenerateEmptyFb2()
        {
            string content = " FB2 <?xml version=\"1.0\" encoding=\"utf-8\"?><FictionBook xmlns=\"http://www.gribuser.ru/xml/fictionbook/2.0\"><description></description><body></body></FictionBook>";
            return System.Text.Encoding.UTF8.GetBytes(content);
        }

        private byte[] GenerateEmptyMobi()
        {
            string content = "MOBI";
            return System.Text.Encoding.UTF8.GetBytes(content);
        }
    }
}

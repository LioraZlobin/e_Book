using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Mail;

namespace e_Book.Services
{
    public class EmailService
    {
        private readonly string _smtpHost = "sandbox.smtp.mailtrap.io"; // כתובת ה-SMTP של Mailtrap
        private readonly int _smtpPort = 2525; // פורט של Mailtrap
        private readonly string _smtpUser = "bc04508b2cc5e9"; // שם המשתמש שלך ב-Mailtrap
        private readonly string _smtpPass = "da9adda60c17f0"; // סיסמת ה-API שלך ב-Mailtrap
        private readonly string _fromEmail = "admin@example.com"; // כתובת דוא"ל לשולח (Fake Email)

        public void SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                using (var smtpClient = new SmtpClient(_smtpHost, _smtpPort))
                {
                    smtpClient.Credentials = new NetworkCredential(_smtpUser, _smtpPass);
                    smtpClient.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_fromEmail),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(toEmail);

                    smtpClient.Send(mailMessage);
                }
            }
            catch (Exception ex)
            {
                // ניתן להוסיף לוגים כדי לתעד את השגיאה
                throw new Exception($"שגיאה בשליחת המייל: {ex.Message}");
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;
using System.Threading.Tasks;
using e_Book.Services; // ודאי שה-namespace מתאים ל-Service שלך

public class EmailReminderJob : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        // קריאה למחלקת BorrowsController לצורך שליחת תזכורות
        var borrowController = new e_Book.Controllers.BorrowsController();

        // קריאה לפונקציה לשליחת מיילים ומחיקת ספרים מושאלים שפג תוקפם
        borrowController.SendReminderEmails();
        borrowController.ProcessExpiredBorrows();

        return Task.CompletedTask;
    }
}

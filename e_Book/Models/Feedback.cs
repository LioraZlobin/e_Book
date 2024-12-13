using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace e_Book.Models
{
    public class Feedback
    {
        public int FeedbackId { get; set; } // מזהה ייחודי לפידבק
        public int UserId { get; set; } // מזהה המשתמש
        public int? BookId { get; set; } // מזהה הספר (אופציונלי, במידה והפידבק לספר)
        public string Content { get; set; } // תוכן הפידבק
        public int Rating { get; set; } // דירוג (1-5)
        public DateTime FeedbackDate { get; set; } // תאריך הפידבק

        public bool IsPurchaseFeedback { get; set; }
        public virtual Book Book { get; set; }

        public virtual User User { get; set; }
    }
}
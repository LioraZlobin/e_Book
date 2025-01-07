using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace e_Book.Models
{
    public class WaitingList
    {
        public int WaitingListId { get; set; } // מזהה ייחודי לרשימה
        public int UserId { get; set; } // מזהה המשתמש הממתין
        public int BookId { get; set; } // מזהה הספר שאליו המשתמש ממתין
        public DateTime AddedDate { get; set; } // תאריך ההצטרפות לרשימה
        public int Position { get; set; }//מיקום בתור

        public DateTime? ExpirationTime { get; set; }//הזמן שמוקצב ברשימת המתנה ל3 הראשונים כשספר חוזר למלאי
    }
}
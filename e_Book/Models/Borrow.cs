using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace e_Book.Models
{
    public class Borrow
    {
        public int BorrowId { get; set; } // מזהה ייחודי להשאלה
        public int UserId { get; set; } // מזהה המשתמש שמשאיל
        public int BookId { get; set; } // מזהה הספר המושאל
        public DateTime BorrowDate { get; set; } // תאריך ההשאלה
        public DateTime DueDate { get; set; } // תאריך החזרה

        public bool IsReturned { get; set; } // האם הספר הוחזר
        public virtual Book Book { get; set; }
        public virtual User User { get; set; }
    }
}
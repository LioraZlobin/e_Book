using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace e_Book.Models
{
    public class Purchase
    {
        public int PurchaseId { get; set; } // מזהה ייחודי לקנייה
        public int UserId { get; set; } // מזהה המשתמש שרכש
        public int BookId { get; set; } // מזהה הספר שנרכש
        public decimal PurchasePrice { get; set; } // מחיר הקנייה
        public DateTime PurchaseDate { get; set; } // תאריך הרכישה

        public virtual User User { get; set; }
        public virtual Book Book { get; set; }
    }
}
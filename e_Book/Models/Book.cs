using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace e_Book.Models
{
    public class Book
    {
        public int BookId { get; set; } // מזהה ייחודי לספר
        public string Title { get; set; } // שם הספר
        public string Author { get; set; } // שם המחבר
        public string Publisher { get; set; } // הוצאה לאור
        public string Genre { get; set; } // ז'אנר
        public int Popularity { get; set; } // פופולריות (1-5)
        public decimal PriceBuy { get; set; } // מחיר רכישה
        public decimal? PreviousPrice { get; set; } // מחיר קודם
        public DateTime? DiscountEndDate { get; set; } // תאריך סיום ההנחה
        public decimal PriceBorrow { get; set; } // מחיר השאלה
        public int AvailableCopies { get; set; } // מספר עותקים זמינים להשאלה
        public string Format { get; set; } // פורמט הספר (epub, mobi, PDF)
        public int YearPublished { get; set; } // שנת פרסום
        public bool IsBorrowable { get; set; } // האם הספר ניתן להשאלה
        public string AgeRestriction { get; set; } = "All Ages"; //ברירת מחדל

        public string Synopsis { get; set; } // תקציר הספר
        public string ImageUrl { get; set; } // כתובת התמונה
        public string DownloadLink { get; set; } // קישור להורדה
    }
}
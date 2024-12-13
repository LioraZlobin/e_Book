using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace e_Book.Models
{
    public class UserLibraryItem
    {
        public int BookId { get; set; } // מזהה ייחודי של הספר
        public int? BorrowId { get; set; } // מזהה ייחודי של ההשאלה (אם הושאל)
        public int? PurchaseId { get; set; }// מזהה ייחודי של רכישה 
        public string Title { get; set; } // שם הספר
        public DateTime? BorrowDate { get; set; } // תאריך השאלה (אם הושאל)
        public DateTime? DueDate { get; set; } // תאריך החזרה (אם הושאל)
        public DateTime? PurchaseDate { get; set; } // תאריך רכישה (אם נרכש)
        public bool IsPurchased { get; set; } // האם הספר נרכש
        public bool IsBorrowed { get; set; } // האם הספר מושאל
        public string DownloadLink { get; set; } // קישור להורדה, אם קיים
    }

}
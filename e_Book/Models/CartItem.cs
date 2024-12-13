using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;


namespace e_Book.Models
{
    public class CartItem
    {
        public int CartItemId { get; set; } // מזהה ייחודי לפריט בעגלה
        public int UserId { get; set; } // מזהה המשתמש
        public int BookId { get; set; } // מזהה הספר
        public int Quantity { get; set; } // כמות
        public string TransactionType { get; set; } // קנייה או השאלה

        // קשרים
        public virtual Book Book { get; set; } // ספר
        public virtual User User { get; set; } // משתמש
    }
}
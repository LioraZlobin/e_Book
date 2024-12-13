using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace e_Book.Models
{
    public class Payment
    {
        public int PaymentId { get; set; } // מזהה ייחודי לתשלום
        public int UserId { get; set; } // מזהה המשתמש ששילם
        public int BookId { get; set; } // מזהה הספר (אם רלוונטי)
        public decimal Amount { get; set; } // סכום התשלום
        public string PaymentMethod { get; set; } // אמצעי תשלום פייפל או כרטיס אשראי
        public DateTime PaymentDate { get; set; } // תאריך התשלום

        public virtual Book Book { get; set; }
        public virtual User User { get; set; }
    }
}
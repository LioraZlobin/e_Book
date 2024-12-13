using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace e_Book.Models
{
    public class User
    {
        public int UserId { get; set; } // מזהה ייחודי למשתמש
        public string Name { get; set; } // שם המשתמש
        public string Email { get; set; } // כתובת אימייל
        public string Password { get; set; } // סיסמה
        public string Role { get; set; } // תפקיד (Admin או User)
        public string IdentityCard { get; set; } // מספר תעודת זהות
        public int Age { get; set; } // גיל המשתמש
    }
}
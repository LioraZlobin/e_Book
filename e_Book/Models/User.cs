// Models/User.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace e_Book.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; } // hashed with SHA-256

        [Required]
        public string Role { get; set; } // "Admin" or "User"

        // 🆕 Credit Card Info

        [RegularExpression("^[0-9]{16}$", ErrorMessage = "Card must be 16 digits")]
        public string CreditCardNumber { get; set; }

        [RegularExpression("^(0[1-9]|1[0-2])/([0-9]{2})$", ErrorMessage = "Expiry must be in MM/YY format")]
        public string ExpiryDate { get; set; }

        
        [RegularExpression("^[0-9]{3}$", ErrorMessage = "CVC must be 3 digits")]
        public string CVC { get; set; }

        // אפשר להוסיף גם Age, LibraryItems וכו׳ לפי הצורך
        public int Age { get; set; }
    }
}

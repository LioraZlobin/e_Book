using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace e_Book.Models
{
    public class CreditCardInfo
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string PersonalId { get; set; }

        [Required]
        [Display(Name = "Card Number")]
        public string CardNumber { get; set; }

        [Required]
        [Display(Name = "Expiry Date")]
        public string ExpiryDate { get; set; }

        [Required]
        public string CVC { get; set; }
    }
}

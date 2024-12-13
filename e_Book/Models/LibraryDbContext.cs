using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using e_Book.Models;

namespace eBookLibrary.Models
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext() : base("DefaultConnection") { }

        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Borrow> Borrows { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<WaitingList> WaitingLists { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        public System.Data.Entity.DbSet<e_Book.Models.Payment> Payments { get; set; }
    }
}

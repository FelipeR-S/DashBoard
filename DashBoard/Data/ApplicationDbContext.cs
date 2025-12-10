using DashBoard.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DashBoard.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Cliente>().HasIndex(c => c.Email).IsUnique();
            builder.Entity<NewsLetter>();
            builder.Entity<User>().HasIndex(u => u.Usuario).IsUnique();
            builder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            builder.Entity<Role>();
            builder.Entity<Permissoes>();
            builder.Entity<Estado>();
            builder.Entity<Cidade>();
        }
    }
}
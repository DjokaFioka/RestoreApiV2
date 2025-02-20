using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RestoreApiV2.Entities;

namespace RestoreApiV2.Data
{
    public class StoreContext(DbContextOptions options) : IdentityDbContext<User>(options)
    {
        public required DbSet<Product> Products { get; set; }
        public required DbSet<Basket> Baskets { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityRole>()
                .HasData(
                    new IdentityRole { Id = "90825fbe-a7d0-4ab7-a731-db3874f99eb6", Name = "Member", NormalizedName = "MEMBER" },
                    new IdentityRole { Id = "f0223b23-d08c-4b7a-89a1-0073975cfb48", Name = "Admin", NormalizedName = "ADMIN" }
                );
        }
    }
}

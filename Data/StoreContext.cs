using Microsoft.EntityFrameworkCore;
using RestoreApiV2.Entities;

namespace RestoreApiV2.Data
{
    public class StoreContext(DbContextOptions options) : DbContext(options)
    {
        public required DbSet<Product> Products { get; set; }
        public required DbSet<Basket> Baskets { get; set; }
    }
}

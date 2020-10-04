using System.Data.Entity;

namespace SendPics
{
    class TgContext : DbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Image> Images { get; set; }
    }
}

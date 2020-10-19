using System.Data.Entity;

namespace PrivatCurrency
{
    public class TgContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Interval> Intervals { get; set; }
    }
}

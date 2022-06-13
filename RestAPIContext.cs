using Microsoft.EntityFrameworkCore;
using RestAPI.Model;

namespace RestAPI
{
    public class RestAPIContext : DbContext
    {
        public RestAPIContext(DbContextOptions<RestAPIContext> options) : base(options)
        {

        }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDet> OrderDet { get; set; }
        public DbSet<Releases> Releases { get; set; }
        public DbSet<Materials> Materials { get; set; }
        public DbSet<PODet> PODet { get; set; }
        public DbSet<PO> PO { get; set; }

    }
}

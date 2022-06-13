using Microsoft.EntityFrameworkCore;
using RestAPI.Model;

namespace RestAPI
{
    public class CadmanContext : DbContext
    {
        public CadmanContext(DbContextOptions<CadmanContext> options) : base(options)
        {

        }
        public DbSet<PartTemplateRevision> PartTemplateRevision { get; set; }
       

    }
}

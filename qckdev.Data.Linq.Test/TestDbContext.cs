using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace qckdev.Data.Linq.Test
{
    sealed class TestDbContext : DbContext
    {

        public TestDbContext(DbContextOptions options)
            : base(options) { }


        public DbSet<Entities.TestHeader> TestHeaders { get; set; }
        public DbSet<Entities.TestLine> TestLines { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

    }
}

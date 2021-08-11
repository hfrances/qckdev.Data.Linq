using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace qckdev.Data.Linq.Test
{
    static class Helpers
    {

        public static TDbContext CreateDbContext<TDbContext>(Func<DbContextOptionsBuilder<TDbContext>, DbContextOptionsBuilder<TDbContext>> builder) where TDbContext : DbContext
        {
            var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
            var options = builder(optionsBuilder).Options;

            return (TDbContext)Activator.CreateInstance(typeof(TDbContext), options);
        }

        public static IEnumerable<Entities.TestHeader> GetSampleData()
        {
            return new[]{
                new Entities.TestHeader()
                {
                    Name = "Hello world",
                    Lines = new[]
                    {
                        new Entities.TestLine { Description = "First line" },
                        new Entities.TestLine { Description = "Second line" }
                    }
                },
                new Entities.TestHeader()
                {
                    Name = "Bye!",
                    Lines = new[]
                    {
                        new Entities.TestLine { Description = "First line" },
                        new Entities.TestLine { Description = "Last line", Disabled = true }
                    }
                },
                new Entities.TestHeader()
                {
                    Name = "First line",
                    Lines = new[]
                    {
                        new Entities.TestLine { Description = "Line A", Disabled = true },
                        new Entities.TestLine { Description = "Line B" }
                    }
                },
                new Entities.TestHeader()
                {
                    Name = "One more line",
                    Lines = new[]
                    {
                        new Entities.TestLine { Description = "Line X" },
                        new Entities.TestLine { Description = "Line Y", Disabled = true }
                    }
                },
            };
        }

    }
}

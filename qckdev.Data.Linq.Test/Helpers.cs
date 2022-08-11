using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace qckdev.Data.Linq.Test
{
    static class Helpers
    {

        // https://www.entityframeworktutorial.net/efcore/logging-in-entityframework-core.aspx
        static readonly ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddConsole()
                .AddDebug()
                .AddFilter((category, level) =>
                {
                    return 
                        category.Equals("Microsoft.EntityFrameworkCore.Database.Command") && 
                        level >= LogLevel.Information;
                });
        });

        public static EnumerableComparable<TEntity> ToComparable<TEntity>(this IEnumerable<TEntity> collection)
        {
            return new EnumerableComparable<TEntity>(collection);
        }

        public static TDbContext CreateDbContext<TDbContext>(Func<DbContextOptionsBuilder<TDbContext>, DbContextOptionsBuilder<TDbContext>> builder) where TDbContext : DbContext
        {
            var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
            DbContextOptions<TDbContext> options;

            loggerFactory.CreateLogger<TDbContext>().LogInformation("Creating DbContext...");
            optionsBuilder =
                optionsBuilder
                    .EnableSensitiveDataLogging()
                    .UseLoggerFactory(loggerFactory);
            optionsBuilder = builder(optionsBuilder);
            options = optionsBuilder.Options;
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

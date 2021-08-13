using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace qckdev.Data.Linq.Test
{
    [TestClass]
    public class QueryableTests
    {

        /// <summary>
        /// Check if InMemory DbContext works properly.
        /// </summary>
        [TestMethod]
        public void TestDbContext()
        {
            using var context = Helpers.CreateDbContext<TestDbContext>(
                builder => builder.UseSqlite($"Data Source={Guid.NewGuid()}.db")
            );
            var headerId = Guid.NewGuid();
            int rowcount;

            context.Database.EnsureCreated();
            context.TestHeaders.Add(new Entities.TestHeader()
            {
                TestHeaderId = headerId,
                Name = "Hola Mundo"
            });
            context.TestLines.Add(new Entities.TestLine()
            {
                TestHeaderId = headerId,
                Description = "First line"
            });
            rowcount = context.SaveChanges();
            Assert.AreEqual(2, rowcount);
        }

        /// <summary>
        /// Check if <see cref="Queryable.WhereString"/> is working properly.
        /// </summary>
        [TestMethod]
        public void TestWhereString()
        {
            using var context = Helpers.CreateDbContext<TestDbContext>(
                builder => builder.UseSqlite($"Data Source={Guid.NewGuid()}.db")
            );

            context.Database.EnsureCreated();
            context.TestHeaders.AddRange(Helpers.GetSampleData());
            context.SaveChanges();

            var rdo =
                context.TestHeaders
                    .Include(x => x.Lines)
                    .WhereString("First line",
                        x => x.Name,
                        x => x.Lines.Select(x => x.Description)
                    );
            Assert.AreEqual(3, rdo.Count());
        }

        /// <summary>
        /// Checks if <see cref="Queryable.WhereIn"/> works proerly.
        /// </summary>
        [TestMethod]
        public void TestWhereIn()
        {
            var descriptions = new[] { "Line A", "Line X" };
            using var context = Helpers.CreateDbContext<TestDbContext>(
                builder => builder.UseSqlite($"Data Source={Guid.NewGuid()}.db")
            );

            context.Database.EnsureCreated();
            context.TestHeaders.AddRange(Helpers.GetSampleData());
            context.SaveChanges();

            var rdo =
                context.TestLines
                    .Include(x => x.Header)
                    .WhereIn(descriptions, x => x.Description);
            Assert.AreEqual(2, rdo.Count());
        }

        /// <summary>
        /// Checks if <see cref="Queryable.WhereIn"/> works proerly.
        /// </summary>
        [TestMethod]
        public void TestWhereAnd()
        {
            using var context = Helpers.CreateDbContext<TestDbContext>(
                builder => builder.UseSqlite($"Data Source={Guid.NewGuid()}.db")
            );

            context.Database.EnsureCreated();
            context.TestHeaders.AddRange(Helpers.GetSampleData());
            context.SaveChanges();

            var rdo =
                context.TestHeaders
                    .Include(x => x.Lines)
                    .WhereAnd(
                        x => x.Name == "Hello world", 
                        x => x.Lines.Any(y => y.Description == "First line")
                    );
            Assert.AreEqual(1, rdo.Count());
        }

        /// <summary>
        /// Checks if <see cref="Queryable.WhereIn"/> works proerly.
        /// </summary>
        [TestMethod]
        public void TestWhereOr()
        {
            using var context = Helpers.CreateDbContext<TestDbContext>(
                builder => builder.UseSqlite($"Data Source={Guid.NewGuid()}.db")
            );

            context.Database.EnsureCreated();
            context.TestHeaders.AddRange(Helpers.GetSampleData());
            context.SaveChanges();

            var rdo =
                context.TestHeaders
                    .Include(x => x.Lines)
                    .WhereOr(
                        x => x.Name == "Hello world",
                        x => x.Lines.Any(y => y.Description == "Line X")
                    );
            Assert.AreEqual(2, rdo.Count());
        }

        /// <summary>
        /// Checks if <see cref="Queryable.GetPaged"/> works properly.
        /// </summary>
        [TestMethod]
        public void TestPagination()
        {
            using var context = Helpers.CreateDbContext<TestDbContext>(
                builder => builder.UseSqlite($"Data Source={Guid.NewGuid()}.db")
            );

            context.Database.EnsureCreated();
            for (int i = 0; i < 100; i++)
            {
                context.Add(new Entities.TestHeader
                {
                    Name = $"Header number {i}"
                });
            }
            context.SaveChanges();

            var rdo = context.TestHeaders.GetPaged(1, 5);
            Assert.AreEqual(
                new { Current = 1, Pages = 20, Total = 100, Count = 5 },
                new { rdo.Current, rdo.Pages, rdo.Total, Count = rdo.Items.Count() }
            );
        }

    }
}

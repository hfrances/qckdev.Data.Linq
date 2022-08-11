using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;

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
        /// Checks if <see cref="Queryable.WhereIn"/> works proerly.
        /// </summary>
        [TestMethod]
        public void TestWhereOr_Empty()
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
                    .WhereOr();
            Assert.AreEqual(context.TestHeaders.Count(), rdo.Count());
        }

        /// <summary>
        /// Checks if <see cref="Queryable.WhereIn"/> works proerly.
        /// </summary>
        [TestMethod]
        public void TestWhereOr_One()
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
                    .WhereOr(x => x.Name == "Hello world");
            Assert.AreEqual(1, rdo.Count());
        }

        /// <summary>
        /// Checks if <see cref="Queryable.GetPaged"/> works properly.
        /// </summary>
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)] 
        [DataRow(2)]
        [DataRow(5)]
        [DataRow(20)]
        [DataRow(21)]
        public void TestPagination(int page)
        {
            using var context = Helpers.CreateDbContext<TestDbContext>(
                builder => builder.UseSqlite($"Data Source={Guid.NewGuid()}.db")
            );

            context.Database.EnsureCreated();
            for (int i = 0; i < 100; i++)
            {
                context.Add(new Entities.TestHeader
                {
                    Name = $"Header number {i:000}"
                });
            }
            context.SaveChanges();

            var rdo = context.TestHeaders.GetPaged(page, 5);
            var expected = context.TestHeaders.Skip((page-1) * 5).Take(5);
            Assert.AreEqual(
                new { Current = page, Pages = 20, Total = 100, Items = expected.ToComparable() },
                new { rdo.Current, rdo.Pages, rdo.Total, Items = rdo.Items.ToComparable() }
            );
        }

        /// <summary>
        /// Checks if <see cref="Queryable.GetPaged"/> works properly.
        /// </summary>
        [TestMethod]
        public void TestPagination_No_Results()
        {
            using var context = Helpers.CreateDbContext<TestDbContext>(
                builder => builder.UseSqlite($"Data Source={Guid.NewGuid()}.db")
            );

            context.Database.EnsureCreated();
            context.SaveChanges();

            var rdo = context.TestHeaders.GetPaged(1, 5);
            var expected = context.TestHeaders.Skip(5).Take(5);
            Assert.AreEqual(
                new { Current = 1, Pages = 0, Total = 0, Items = expected.ToComparable() },
                new { rdo.Current, rdo.Pages, rdo.Total, Items = rdo.Items.ToComparable() }
            );
        }

    }
}

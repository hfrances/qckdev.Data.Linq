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
                builder => builder.UseInMemoryDatabase($"memory{Guid.NewGuid()}")
            );
            var headerId = Guid.NewGuid();
            int rowcount;

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
                builder => builder.UseInMemoryDatabase($"memory{Guid.NewGuid()}")
            );

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
        public void TestWhereIn_Enumerable()
        {
            var descriptions = new[] { "Line A", "Line X" };
            using var context = Helpers.CreateDbContext<TestDbContext>(
                builder => builder.UseInMemoryDatabase($"memory{Guid.NewGuid()}")
            );

            context.TestHeaders.AddRange(Helpers.GetSampleData());
            context.SaveChanges();

            var rdo =
                context.TestLines
                    .Include(x => x.Header)
                    .WhereIn(descriptions, x => x.Description);
            Assert.AreEqual(2, rdo.Count());
        }

        /// <summary>
        /// Checks if <see cref="Queryable.WhereIn{TEntity, TValue}(IQueryable{TEntity}, IQueryable{TValue}, System.Linq.Expressions.Expression{Func{TEntity, TValue}})"/> works properly.
        /// </summary>
        [TestMethod]
        public void TestWhereIn_Queryable()
        {
            using var context = Helpers.CreateDbContext<TestDbContext>(
                builder => builder.UseInMemoryDatabase($"memory{Guid.NewGuid()}")
            );

            context.TestHeaders.AddRange(Helpers.GetSampleData());
            context.SaveChanges();

            var lineIds = context.TestLines.Where(x => !x.Disabled).Select(x => x.TestLineId);
            var rdo = context.TestLines.WhereIn(lineIds, x => x.TestLineId);
            Assert.AreEqual(5, rdo.Count());
        }

        /// <summary>
        /// Checks if <see cref="Queryable.GetPaged"/> works properly.
        /// </summary>
        [TestMethod]
        public void TestPagination()
        {
            using var context = Helpers.CreateDbContext<TestDbContext>(
                builder => builder.UseInMemoryDatabase($"memory{Guid.NewGuid()}")
            );

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
                new { CurrentPage = 1, Pages = 20, Total = 100, Count = 5 },
                new { rdo.CurrentPage, rdo.Pages, rdo.Total, Count = rdo.Items.Count() }
            );
        }

    }
}

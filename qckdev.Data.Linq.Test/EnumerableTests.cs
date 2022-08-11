using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;

namespace qckdev.Data.Linq.Test
{
    [TestClass]
    public class EnumerableTests
    {

        /// <summary>
        /// <summary>
        /// Check if <see cref="Queryable.WhereString"/> is working properly.
        /// </summary>
        [TestMethod]
        public void TestWhereString()
        {
            var testHeaders = new List<Entities.TestHeader>(Helpers.GetSampleData());

            var rdo =
                testHeaders
                    .WhereString("First line",
                        x => x.Name,
                        x => x.Lines.Select(x => x.Description)
                    );
            Assert.AreEqual(3, rdo.Count());
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
            var testHeaders = new List<Entities.TestHeader>();

            for (int i = 0; i < 100; i++)
            {
                testHeaders.Add(new Entities.TestHeader
                {
                    Name = $"Header number {i:000}"
                });
            }

            var rdo = testHeaders.GetPaged(page, 5);
            var expected = testHeaders.Skip((page-1) * 5).Take(5);
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
            var testHeaders = new List<Entities.TestHeader>();

            var rdo = testHeaders.GetPaged(1, 5);
            var expected = testHeaders.Skip(5).Take(5);
            Assert.AreEqual(
                new { Current = 1, Pages = 0, Total = 0, Items = expected.ToComparable() },
                new { rdo.Current, rdo.Pages, rdo.Total, Items = rdo.Items.ToComparable() }
            );
        }

    }
}

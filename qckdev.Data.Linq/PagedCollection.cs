using System.Collections.Generic;

namespace qckdev.Data.Linq
{

    /// <summary>
    /// Represents a paged results.
    /// </summary>
    /// <typeparam name="TSource">Type of the page results.</typeparam>
    public class PagedCollection<TSource> : IDataCollection<TSource>
    {

        /// <summary>
        /// Gets the current page.
        /// </summary>
        public int Current { get; }

        /// <summary>
        /// Gets the total pages.
        /// </summary>
        public int Pages { get;}

        /// <summary>
        /// Gets the total of elements returned for the query.
        /// </summary>
        public int Total { get;  }

        /// <summary>
        /// Gets the page elements.
        /// </summary>
        public IEnumerable<TSource> Items { get;}

        /// <summary>
        /// Creates a new instance of <see cref="PagedCollection{TSource}"/>
        /// </summary>
        /// <param name="current">The current page.</param>
        /// <param name="pages">Total of pages.</param>
        /// <param name="total">Total of query elements.</param>
        /// <param name="items">Page elements.</param>
        internal protected PagedCollection(int current, int pages, int total, IEnumerable<TSource> items)
        {
            this.Current = current;
            this.Pages = pages;
            this.Total = total;
            this.Items = items;
        }

    }
}

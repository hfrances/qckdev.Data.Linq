using System;
using System.Collections.Generic;
using System.Linq;

namespace qckdev.Data.Linq
{

    /// <summary>
    /// Provides a set of static (Shared in Visual Basic) methods for querying data structures that implement <see cref="IEnumerable{T}"/>.
    /// </summary>
    public static class Enumerable
    {

        /// <summary>
        /// Adds pagination to <see cref="IEnumerable{TSource}"/>.
        /// </summary>
        /// <typeparam name="TSource">Kind of <see cref="IEnumerable{TSource}"/> to process</typeparam>
        /// <param name="query"><see cref="IEnumerable{TSource}"/> to paginate on</param>
        /// <param name="page">From where to catch</param>
        /// <param name="take">How much to take</param>
        /// <returns><see cref="PagedCollection{TSource}"/></returns>
        public static PagedCollection<TSource> GetPaged<TSource>(this IEnumerable<TSource> query, int page, int take)
        {
            return GetPaged(query, page, take, x => x);
        }

        /// <summary>
        /// Adds pagination to <see cref="IEnumerable{TSource}"/>.
        /// </summary>
        /// <typeparam name="TSource">Kind of <see cref="IEnumerable{TSource}"/> to process</typeparam>
        /// <typeparam name="TResult">Kind of IEnumerable to return</typeparam>
        /// <param name="query"><see cref="IEnumerable{TSource}"/> to paginate on</param>
        /// <param name="page">From where to catch</param>
        /// <param name="take">How much to take</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>DataCollection</returns>
        public static PagedCollection<TResult> GetPaged<TSource, TResult>(this IEnumerable<TSource> query, int page, int take, Func<TSource, TResult> selector)
        {
            var originalPage = page;
            page--;

            if (page > 0)
            {
                page *= take;
            }

            var queryCount = query.Count();
            var result = new PagedCollection<TResult>(
                current: originalPage,
                pages: queryCount == 0 ? 0 : Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(queryCount) / take)),
                items: query.Skip(page).Take(take).Select(selector),
                total: queryCount
            );

            return result;
        }

        /// <summary>
        /// Filters a sequence of items which contains some of the values specified in the predicates.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="collection">An <see cref="IEnumerable{TSource}"/>.</param>
        /// <param name="value">The string value to search.</param>
        /// <param name="predicates">A list of functions to test each element for a condition.</param>
        /// <returns>
        /// An <see cref="IEnumerable{TSource}"/> that contains elements from the input sequence 
        /// that contains contains some of the values specified in the predicates.
        /// </returns>
        public static IEnumerable<TSource> WhereString<TSource>(this IEnumerable<TSource> collection, string value,
            params Func<TSource, object>[] predicates)
        {

            return collection.Where(x =>
            {
                return predicates.Any(y =>
                {
                    var val = y(x);

                    if (val is IEnumerable<string> text)
                    {
                        return text.Any(z => (z ?? "").Contains(value));
                    }
                    else
                    {
                        return (val?.ToString() ?? "").Contains(value);
                    }
                });
            });
        }

    }
}

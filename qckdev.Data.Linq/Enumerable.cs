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
        /// <param name="query"><see cref="IEnumerable{TSource}"/> to paginate on</param>
        /// <param name="page">From where to catch</param>
        /// <param name="take">How much to take</param>
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

        public static IEnumerable<TEntity> WhereFilter<TEntity>(this IEnumerable<TEntity> source, string value,
            params Func<TEntity, object>[] predicates)
        {

            return source.Where(x =>
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

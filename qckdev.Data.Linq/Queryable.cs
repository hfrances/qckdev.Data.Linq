using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace qckdev.Data.Linq
{

    /// <summary>
    /// Provides a set of static (Shared in Visual Basic) methods for querying data structures that implement <see cref="IQueryable{T}"/>.
    /// </summary>
    public static class Queryable
    {

        /// <summary>
        /// Adds pagination to <see cref="IQueryable{TSource}"/>.
        /// </summary>
        /// <typeparam name="TSource">Kind of IQueryable to process</typeparam>
        /// <param name="query">IQueryable to paginate on</param>
        /// <param name="page">From where to catch</param>
        /// <param name="take">How much to take</param>
        /// <returns><see cref="PagedCollection{TSource}"/></returns>
        public static PagedCollection<TSource> GetPaged<TSource>(this IQueryable<TSource> query, int page, int take)
        {
            return GetPaged(query, page, take, x => x);
        }

        /// <summary>
        /// Adds pagination to <see cref="IQueryable{TSource}"/>.
        /// </summary>
        /// <typeparam name="T">Kind of IQueryable to process</typeparam>
        /// <param name="query">IQueryable to paginate on</param>
        /// <param name="page">From where to catch</param>
        /// <param name="take">How much to take</param>
        /// <returns><see cref="PagedCollection{TSource}"/></returns>
        public static PagedCollection<TResult> GetPaged<TSource, TResult>(this IQueryable<TSource> query, int page, int take, Func<TSource, TResult> selector)
        {
            var originalPage = page;

            page--;
            if (page > 0)
            {
                page *= take;
            }

            var result = new PagedCollection<TResult>
            {
                Items = query.Skip(page).Take(take).Select(selector),
                Total = query.Count(),
                CurrentPage = originalPage
            };

            if (result.Total > 0)
            {
                result.Pages = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(result.Total) / take));
            }
            return result;
        }

        /// <summary>
        /// Adds pagination to <see cref="IQueryable{TSource}"/>.
        /// </summary>
        /// <typeparam name="TSource">Kind of IQueryable to process</typeparam>
        /// <param name="query">IQueryable to paginate on</param>
        /// <param name="page">From where to catch</param>
        /// <param name="take">How much to take</param>
        /// <returns><see cref="PagedCollection{TSource}"/></returns>
        public static async Task<PagedCollection<TSource>> GetPagedAsync<TSource>(this IQueryable<TSource> query, int page, int take)
        {
            return await GetPagedAsync(query, page, take, x => x);
        }

        /// <summary>
        /// Adds pagination to <see cref="IQueryable{TSource}"/>.
        /// </summary>
        /// <typeparam name="T">Kind of IQueryable to process</typeparam>
        /// <param name="query">IQueryable to paginate on</param>
        /// <param name="page">From where to catch</param>
        /// <param name="take">How much to take</param>
        /// <returns><see cref="PagedCollection{TSource}"/></returns>
        public static async Task<PagedCollection<TResult>> GetPagedAsync<TSource, TResult>(this IQueryable<TSource> query, int page, int take, Func<TSource, TResult> selector)
        {
            var originalPage = page;

            page--;
            if (page > 0)
            {
                page *= take;
            }

            var result = new PagedCollection<TResult>
            {
                Items = (await Task.FromResult(query.Skip(page).Take(take).ToArray())).Select(selector),
                Total = await Task.FromResult(query.Count()),
                CurrentPage = originalPage
            };

            if (result.Total > 0)
            {
                result.Pages = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(result.Total) / take));
            }
            return result;
        }


        public static IQueryable<TEntity> WhereString<TEntity>(this IQueryable<TEntity> source, string value,
            params Expression<Func<TEntity, object>>[] predicates)
        {
            var valueAux = value ?? "";
            var contains = typeof(string).GetMethod(nameof(string.Contains), new Type[] { typeof(string) });
            var any = typeof(System.Linq.Enumerable)
                .GetMethods()
                .Where(x => x.Name == nameof(System.Linq.Queryable.Any))
                .First(x => x.GetParameters().Count() == 2);
            var anyGen = any.MakeGenericMethod(typeof(string));
            Expression<Func<string, bool>> anyExpr = (p => p.Contains(valueAux));

            var parameterExpr = Expression.Parameter(typeof(TEntity), "p");
            var valueExpr = Expression.Constant(valueAux);
            var completeExpression = (Expression)null;
            var etor = predicates.AsEnumerable().GetEnumerator();

            while (etor.MoveNext())
            {
                var expression = etor.Current.Body;

                expression = expression.ReplaceParameter(parameterExpr);

                // Call to "Contains" function and concat with other conditions using "OrElse" method.
                if (typeof(IEnumerable<string>).IsAssignableFrom(expression.Type))
                {
                    // Enumerable: add Any with contains.
                    expression = Expression.Call(anyGen, expression, anyExpr);
                }
                else
                {
                    // Single: add contains.
                    expression = Expression.Call(expression, contains, valueExpr);
                }

                if (completeExpression == null)
                {
                    completeExpression = expression;
                }
                else
                {
                    completeExpression = Expression.OrElse(completeExpression, expression);
                }
            }

            var lambda = Expression.Lambda<Func<TEntity, bool>>(completeExpression, false, parameterExpr);
            return source.Where(lambda);
        }

        public static IQueryable<TEntity> WhereIn<TEntity, TValue>(this IQueryable<TEntity> source, IEnumerable<TValue> values, Expression<Func<TEntity, TValue>> predicate)
        {
            return WhereIn(source, values, predicate, typeof(System.Linq.Enumerable), nameof(System.Linq.Enumerable.Contains));
        }

        public static IQueryable<TEntity> WhereIn<TEntity, TValue>(this IQueryable<TEntity> source, IQueryable<TValue> values, Expression<Func<TEntity, TValue>> predicate)
        {
            return WhereIn(source, values, predicate, typeof(System.Linq.Queryable), nameof(System.Linq.Queryable.Contains));
        }

        private static IQueryable<TEntity> WhereIn<TEntity, TValue>(this IQueryable<TEntity> source, IEnumerable<TValue> values, Expression<Func<TEntity, TValue>> predicate, Type type, string containsName)
        {
            Expression<Func<TEntity, bool>> emptyExpression = (p => values == null || !values.Any());
            Expression definitiveExpression;
            var parameterExpr = Expression.Parameter(typeof(TEntity), "p");

            definitiveExpression = emptyExpression.Body;
            if (values != null)
            {
                var contains = type.GetMethod(containsName, typeof(TValue), values.GetType(), typeof(TValue));
                var valueExpr = Expression.Constant(values);
                var predicateExpression = predicate.Body.ReplaceParameter(parameterExpr);

                predicateExpression = Expression.Call(null, contains, valueExpr, predicateExpression);
                definitiveExpression = Expression.OrElse(definitiveExpression, predicateExpression);
            }

            var lambda = Expression.Lambda<Func<TEntity, bool>>(definitiveExpression, false, parameterExpr);
            return source.Where(lambda);
        }

    }

}

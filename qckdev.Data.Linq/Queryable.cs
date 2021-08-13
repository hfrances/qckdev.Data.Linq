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

        enum ExpressionOperator
        {
            And, Or
        }

        /// <summary>
        /// Adds pagination to <see cref="IQueryable{TSource}"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
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
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TResult">Kind of IEnumerable to return</typeparam>
        /// <param name="query">IQueryable to paginate on</param>
        /// <param name="page">From where to catch</param>
        /// <param name="take">How much to take</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns><see cref="PagedCollection{TSource}"/></returns>
        public static PagedCollection<TResult> GetPaged<TSource, TResult>(this IQueryable<TSource> query, int page, int take, Func<TSource, TResult> selector)
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
        /// <param name="collection">An <see cref="IQueryable{TSource}"/>.</param>
        /// <param name="value">The string value to search.</param>
        /// <param name="predicates">A list of functions to test each element for a condition.</param>
        /// <returns>
        /// An <see cref="IQueryable{TSource}"/> that contains elements from the input sequence 
        /// that contains contains some of the values specified in the predicates.
        /// </returns>
        public static IQueryable<TSource> WhereString<TSource>(this IQueryable<TSource> collection, string value,
            params Expression<Func<TSource, object>>[] predicates)
        {
            var valueAux = value ?? "";
            var contains = typeof(string).GetMethod(nameof(string.Contains), new Type[] { typeof(string) });
            var any = typeof(System.Linq.Enumerable)
                .GetMethods()
                .Where(x => x.Name == nameof(System.Linq.Queryable.Any))
                .First(x => x.GetParameters().Count() == 2);
            var anyGen = any.MakeGenericMethod(typeof(string));
            Expression<Func<string, bool>> anyExpr = (p => p.Contains(valueAux));

            var parameterExpr = Expression.Parameter(typeof(TSource), "p");
            var valueExpr = Expression.Constant(valueAux);
            var completeExpression = (Expression)null;
            var etor = predicates.AsEnumerable().GetEnumerator();

            while (etor.MoveNext())
            {
                var parameter = etor.Current.Parameters[0];
                var expression = etor.Current.Body;

                expression = expression.ReplaceParameter(parameter, parameterExpr);

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

            var lambda = Expression.Lambda<Func<TSource, bool>>(completeExpression, false, parameterExpr);
            return collection.Where(lambda);
        }

        /// <summary>
        /// Filters a sequence of items which contains some of the specified values.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TValue">The type of the values to compare</typeparam>
        /// <param name="collection">An <see cref="IQueryable{TSource}"/>.</param>
        /// <param name="values">A list of <typeparamref name="TValue"/> in which search the value.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>
        /// An <see cref="IQueryable{TSource}"/> that contains elements from the input sequence that contains some of the specified values.
        /// </returns>
        public static IQueryable<TSource> WhereIn<TSource, TValue>(this IQueryable<TSource> collection, IEnumerable<TValue> values, Expression<Func<TSource, TValue>> predicate)
        {
            return WhereIn(collection, values, predicate, typeof(System.Linq.Enumerable), nameof(System.Linq.Enumerable.Contains));
        }

        private static IQueryable<TSource> WhereIn<TSource, TValue>(this IQueryable<TSource> collection, IEnumerable<TValue> values, Expression<Func<TSource, TValue>> predicate, Type type, string methodName)
        {
            Expression<Func<TSource, bool>> emptyExpression = (p => values == null || !values.Any());
            Expression definitiveExpression;
            var parameterExpr = Expression.Parameter(typeof(TSource), "p");

            definitiveExpression = emptyExpression.Body;
            if (values != null)
            {
                var method = type.GetMethod(methodName, typeof(TValue), values.GetType(), typeof(TValue));

                if (method == null)
                {
                    throw new ArgumentException($"Method not found '{methodName}' in type '{type.FullName}' with necessary parameters.");
                }
                else
                {
                    var valueExpr = Expression.Constant(values);
                    var predicateExpression = predicate.Body.ReplaceParameter(predicate.Parameters[0], parameterExpr);

                    predicateExpression = Expression.Call(null, method, valueExpr, predicateExpression);
                    definitiveExpression = Expression.OrElse(definitiveExpression, predicateExpression);
                }
            }

            var lambda = Expression.Lambda<Func<TSource, bool>>(definitiveExpression, false, parameterExpr);
            return collection.Where(lambda);
        }

        /// <summary>
        /// Filters a sequence of values which satisfy all <paramref name="predicates"/> (AND operator).
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="collection">An <see cref="IQueryable{TSource}"/>.</param>
        /// <param name="predicates">A list of functions to test each element for a condition.</param>
        /// <returns>
        /// An <see cref="IQueryable{TSource}"/> that contains elements from the input sequence that satisfy the condition specified by the predicates.
        /// </returns>
        public static IQueryable<TSource> WhereAnd<TSource>(this IQueryable<TSource> collection, params Expression<Func<TSource, bool>>[] predicates)
        {
            return Where(collection, ExpressionOperator.And, predicates);
        }

        /// <summary>
        /// Filters a sequence of values which satisfy one of the <paramref name="predicates"/> (OR operator).
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="collection">An <see cref="IQueryable{TSource}"/>.</param>
        /// <param name="predicates">A list of functions to test each element for a condition.</param>
        /// <returns>
        /// An <see cref="IQueryable{TSource}"/> that contains elements from the input sequence that satisfy the condition specified by the predicates.
        /// </returns>
        public static IQueryable<TSource> WhereOr<TSource>(this IQueryable<TSource> collection, params Expression<Func<TSource, bool>>[] predicates)
        {
            return Where(collection, ExpressionOperator.Or, predicates);
        }

        private static IQueryable<TSource> Where<TSource>(IQueryable<TSource> collection, ExpressionOperator @operator, params Expression<Func<TSource, bool>>[] predicates)
        {
            IQueryable<TSource> rdo;
            var count = predicates.Length;

            switch (count)
            {
                case 0:
                    rdo = collection;
                    break;

                case 1:
                    rdo = collection.Where(predicates[0]);
                    break;

                default:
                    var parameter = Expression.Parameter(typeof(TSource), "x");
                    Expression finalPredicate = null;

                    foreach (var predicate in predicates)
                    {
                        var tmp = predicate.Body.ReplaceParameter(predicate.Parameters[0], parameter);
                        if (finalPredicate == null)
                        {
                            finalPredicate = tmp;
                        }
                        else
                        {
                            switch (@operator)
                            {
                                case ExpressionOperator.And:
                                    finalPredicate = Expression.AndAlso(finalPredicate, tmp);
                                    break;
                                case ExpressionOperator.Or:
                                    finalPredicate = Expression.OrElse(finalPredicate, tmp);
                                    break;
                                default:
                                    throw new ArgumentException($"Invalid operator {@operator}", nameof(@operator));
                            }
                        }
                    }
                    rdo = collection.Where(Expression.Lambda<Func<TSource, bool>>(finalPredicate, parameter));
                    break;
            }
            return rdo;
        }


    }

}

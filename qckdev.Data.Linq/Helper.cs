using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace qckdev.Data.Linq
{
    static class Helper
    {

        public static Expression ReplaceParameter(this Expression expression, ParameterExpression newParameterExpression)
        {
            var expressionList = new List<Expression>();
            var expressionPart = expression;

            // Get expression list.
            do
            {
                if (expressionPart is MemberExpression memberExpression)
                {
                    expressionList.Insert(0, memberExpression);
                    expressionPart = memberExpression.Expression;
                }
                else if (expressionPart is MethodCallExpression methodCallExpression)
                {
                    expressionList.Insert(0, methodCallExpression);
                    expressionPart = methodCallExpression.Object;
                }
                else if (expressionPart is ParameterExpression _)
                {
                    expressionList.Insert(0, newParameterExpression);
                    expressionPart = null;
                }
                else if (expressionPart is LambdaExpression lambdaExpression)
                {
                    expressionList.Insert(0, lambdaExpression);
                    expressionPart = null;
                }
                else if (expressionPart is UnaryExpression unaryExpresion)
                {
                    expressionList.Insert(0, unaryExpresion);
                    expressionPart = unaryExpresion.Operand;
                }
                else
                {
                    throw new NotSupportedException($"WhereAny {expressionPart}");
                }
            } while (expressionPart != null);


            // Build expression list with the new parameter expression.
            expressionPart = null;
            foreach (var subexpression in expressionList)
            {
                if (subexpression is MemberExpression memberExpr)
                {
                    expressionPart = Expression.MakeMemberAccess(expressionPart, memberExpr.Member);
                }
                else if (subexpression is MethodCallExpression methodExpr)
                {
                    var arguments = methodExpr.Arguments.Select(x =>
                        x.ReplaceParameter(newParameterExpression)
                    );
                    expressionPart = Expression.Call(expressionPart, methodExpr.Method, arguments);
                }
                else if (subexpression is ParameterExpression parameterExpr)
                {
                    expressionPart = parameterExpr;
                }
                else if (subexpression is LambdaExpression lambdaExpression)
                {
                    expressionPart = lambdaExpression;
                }
                else if (subexpression is UnaryExpression unaryExpression)
                {
                    expressionPart = Expression.Convert(expressionPart, unaryExpression.Type);
                }
                else
                {
                    throw new NotSupportedException($"WhereAny {expressionPart}");
                }
            }
            return expressionPart;
        }

        public static MethodInfo GetMethod(this Type type, string name, params Type[] types)
        {
            var query = type
                .GetMethods()
                .Where(x => x.Name == name)
                .Select(x => GetEqualityLevel(x, types))
                .OrderBy(x => x.EqualityLevel)
                .FirstOrDefault(x => x.EqualityLevel > 0);

            return query?.Method;
        }

        static dynamic GetEqualityLevelParameters(Type[] parameterTypes, Type[] types)
        {
            int equalityLevel = 1;
            var matchedTypeList = new List<Type>();

            for (int i = 0; i < parameterTypes.Length && equalityLevel > 0; i++)
            {
                Type type = types[i], parameterType = parameterTypes[i];

                if (type == parameterType)
                {
                    equalityLevel = 1;
                }
                else if (parameterType.IsAssignableFrom(type))
                {
                    // TODO: Calculate equality level according to the inheritance level.
                    equalityLevel = 1024;
                }
                else if (parameterType.IsGenericMethodParameter() && (parameterType.BaseType.IsAssignableFrom(type)))
                {
                    // TODO: Calculate equality level according to the inheritance level.
                    equalityLevel = 1;
                }
                else
                {
                    equalityLevel = 0;
                }

                if (equalityLevel > 0)
                {
                    matchedTypeList.Add(parameterType);
                }
            }
            return new
            {
                EqualityLevel = equalityLevel,
                MatchedTypes = matchedTypeList.ToArray()
            };
        }

        static bool IsGenericMethodParameter(this Type type)
        {
            return type.IsGenericParameter && type.DeclaringMethod != null;
        }

        static dynamic GetEqualityLevel(MethodInfo method, Type[] types)
        {
            int equalityLevel = 0;
            var genericTypes = method.GetGenericArguments();
            var parameters = method.GetParameters();
            dynamic equalityResult;

            if (genericTypes.Length + parameters.Length == types.Length)
            {

                if (genericTypes.Any())
                {
                    equalityResult = GetEqualityLevelParameters(genericTypes, types);
                    if (equalityResult.EqualityLevel > 0)
                    {
                        Type[] matchedTypes = equalityResult.MatchedTypes;

                        method = method.MakeGenericMethod(
                            types.Take(matchedTypes.Length).ToArray());
                        parameters = method.GetParameters();
                        equalityLevel = equalityResult.EqualityLevel;
                    }
                }
                else
                {
                    equalityLevel = 1;
                }

                equalityResult = GetEqualityLevelParameters(
                    parameters.Select(x => x.ParameterType).ToArray(),
                    types.Reverse().Take(parameters.Length).Reverse().ToArray()
                );
                equalityLevel *= equalityResult.EqualityLevel;

            }
            return new
            {
                Method = method,
                EqualityLevel = equalityLevel
            };
        }

    }
}

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

        /// <remarks>
        /// https://stackoverflow.com/questions/36650934/combine-two-lambda-expressions-with-inner-expression/36651409#36651409
        /// </remarks>
        public static Expression ReplaceParameter(this Expression expression, ParameterExpression source, Expression target)
        {
            return new ParameterReplacer { Source = source, Target = target }.Visit(expression);
        }

        class ParameterReplacer : ExpressionVisitor
        {
            public ParameterExpression Source;
            public Expression Target;
            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == Source ? Target : base.VisitParameter(node);
            }
        }

    }
}

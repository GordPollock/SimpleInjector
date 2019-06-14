using System;
using System.Linq.Expressions;
using System.Reflection;

namespace SimpleInjector.CodeSamples
{
    public static class CustomExpressionContainerExtensions
    {
        public static void HandleCustomExpressions(this Container container)
        {
            container.ExpressionBuilding += HandleExpressionBuildingEvent;
        }

        private static void HandleExpressionBuildingEvent(object sender, ExpressionBuildingEventArgs e)
        {
            if (IsExpressionCustomExpressionPlaceholder(e.Expression, out var customExpression))
            {
                e.Expression = customExpression.Expression;
            }
        }

        private static bool IsExpressionCustomExpressionPlaceholder(Expression expression,
            out CustomExpression customExpression)
        {
            if (expression is ConstantExpression constantExpression &&
                constantExpression.Value is Delegate d && d.Target is CustomExpression c)
            {
                customExpression = c;
                return true;
            }

            customExpression = null;
            return false;
        }

        public static void RegisterExpression<TSubject>(this Container container, Expression<TSubject> e)
            where TSubject : class
        {
            var customExpression = new CustomExpression(e);
            container.Register(customExpression.PlaceholderFunc<TSubject>);
        }

        public static void RegisterExpression<TSubject>(this Container container, Expression<TSubject> e,
            Lifestyle lifestyle)
            where TSubject : class
        {
            var customExpression = new CustomExpression(e);
            container.Register(customExpression.PlaceholderFunc<TSubject>, lifestyle);
        }
    }

    public class CustomExpression
    {
        public Expression Expression { get; }

        public CustomExpression(Expression expression)
        {
            this.Expression = expression;
        }

        public TSubject PlaceholderFunc<TSubject>() => throw new NotImplementedException();
    }
    
    public class CustomExpressionExtensions
    {
        
    }
}
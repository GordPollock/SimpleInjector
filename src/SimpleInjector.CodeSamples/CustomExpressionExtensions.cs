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
            out ICustomExpression customExpression)
        {
            if (expression is ConstantExpression constantExpression &&
                constantExpression.Value is Delegate d &&
                d.Target is ICustomExpression c)
            {
                customExpression = c;
                return true;
            }

            customExpression = null;
            return false;
        }

        public static void RegisterExpression<TSubject>(this Container container, Expression<Func<ExpressionHelper, TSubject>> e)
            where TSubject : class
        {
            var customExpression = new CustomExpression<TSubject>(e);
            container.Register(customExpression.PlaceholderFunc);
        }

        public static void RegisterExpression<TSubject>(this Container container, Expression<Func<ExpressionHelper, TSubject>> e,
            Lifestyle lifestyle)
            where TSubject : class
        {
            var customExpression = new CustomExpression<TSubject>(e);
            container.Register(customExpression.PlaceholderFunc, lifestyle);
        }
    }

    public class ExpressionHelper
    {
        public T Resolve<T>() => throw new NotImplementedException();

        public T Resolve<T>(string key) => throw new NotImplementedException();
    }

    public interface ICustomExpression
    {
        Expression Expression { get; }
    }

    public class CustomExpression<TSubject> : ICustomExpression
    {
        Expression ICustomExpression.Expression => this.DelegateExpression;

        public Expression<Func<ExpressionHelper, TSubject>> DelegateExpression { get; }

        public CustomExpression(Expression<Func<ExpressionHelper, TSubject>> delegateExpression)
        {
            this.DelegateExpression = delegateExpression;
        }

        public TSubject PlaceholderFunc() => throw new NotImplementedException();
    }
    
    public class CustomExpressionExtensions
    {
        
    }
}
using System;
using System.Linq.Expressions;

namespace agilex.fabrication
{
    public class PostFabrication<T> where T : class
    {
        private readonly T _target;

        public PostFabrication(T target)
        {
            _target = target;
        }

        public PostFabrication<T> SetProperty(Expression<Func<T, object>> action, object overrideInstance)
        {
            var expression = GetMemberInfo(action);
            var propertyName = expression.Member.Name;
            typeof(T).GetProperty(propertyName).SetValue(_target, overrideInstance, new object[] { });
            return this;
        }

        private static MemberExpression GetMemberInfo(Expression method)
        {
            var lambda = method as LambdaExpression;
            if (lambda == null)
                throw new ArgumentNullException("method");

            MemberExpression memberExpr = null;

            if (lambda.Body.NodeType == ExpressionType.Convert)
            {
                memberExpr =
                    ((UnaryExpression)lambda.Body).Operand as MemberExpression;
            }
            else if (lambda.Body.NodeType == ExpressionType.MemberAccess)
            {
                memberExpr = lambda.Body as MemberExpression;
            }

            if (memberExpr == null)
                throw new ArgumentException("method");

            return memberExpr;
        }

        public T Instance()
        {
            return _target;
        }
    }
}
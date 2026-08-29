using System.Linq.Expressions;
using System.Reflection;

namespace Auturge.Validation;

public static class ExpressionExtensions
{
    public static MemberExpression? GetMemberExpression(this Expression? expression)
    {
        if (expression is UnaryExpression unaryExpression)
        {
            return unaryExpression.Operand as MemberExpression;
        }

        return expression as MemberExpression;
    }

    /// <summary> Gets a MemberInfo from a member expression. </summary>
    public static MemberInfo? GetMember<TInput, TProperty>(this Expression<Func<TInput, TProperty>> expression)
    {
        var memberExp = RemoveUnary(expression.Body) as MemberExpression;
        if (memberExp == null) {
            return null;
        }

        Expression? currentExpr = memberExp.Expression;
        while (true) {
            currentExpr = RemoveUnary(currentExpr);

            if (currentExpr is { NodeType: ExpressionType.MemberAccess }) {
                currentExpr = ((MemberExpression)currentExpr).Expression;
            } else {
                break;
            }
        }
        return currentExpr is not { NodeType: ExpressionType.Parameter } ? null : memberExp.Member;
    }
    
    /// <summary>
    /// Checks if the expression is a parameter expression
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    internal static bool IsParameterExpression(this LambdaExpression expression) 
        => expression.Body.NodeType == ExpressionType.Parameter;

    private static Expression? RemoveUnary(Expression? toUnwrap) {
        if (toUnwrap is UnaryExpression expression) {
            return expression.Operand;
        }

        return toUnwrap;
    }
}

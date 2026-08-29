using System.Linq.Expressions;
using System.Reflection;

namespace Auturge.Validation;

internal static class Properties
{
    internal static string ResolvePropertyName(MemberInfo? memberInfo, LambdaExpression? expression) {
        if (expression != null) {
            var chain = PropertyChain.FromExpression(expression);
            if (chain.Count > 0) return chain.ToString();
        }

        return memberInfo?.Name ?? string.Empty;
    }
}

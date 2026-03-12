namespace ActivityLog.Internal;

/// <summary>
/// Extension methods for MethodInfo
/// </summary>
public static class MethodInfoExtensions
{
    public static bool IsAsyncMethod(this System.Reflection.MethodInfo method)
    {
        var returnType = method.ReturnType;
        return returnType == typeof(Task) || 
               (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>)) ||
               returnType == typeof(ValueTask) ||
               (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ValueTask<>));
    }
}
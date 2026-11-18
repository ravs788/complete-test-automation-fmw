using System.Reflection;

namespace Tests.Unit.TestDoubles;

/// <summary>Minimal interface double for exercising vendor cleanup paths without a live broker.</summary>
public class InterfaceStub : DispatchProxy
{
    private Func<MethodInfo, object?[]?, object?> _invoke = null!;

    public static T Create<T>(Func<MethodInfo, object?[]?, object?> invoke) where T : class
    {
        var instance = Create<T, InterfaceStub>();
        ((InterfaceStub)(object)instance)._invoke = invoke;
        return instance;
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => _invoke(targetMethod!, args);

    public static object? Completed(Type returnType, object? value = null)
    {
        if (returnType == typeof(void)) return null;
        if (returnType == typeof(Task)) return Task.CompletedTask;
        if (returnType == typeof(ValueTask)) return ValueTask.CompletedTask;
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            return typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(returnType.GenericTypeArguments[0]).Invoke(null, [value]);
        }
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
        {
            return Activator.CreateInstance(returnType, [value]);
        }
        throw new NotSupportedException(returnType.FullName);
    }
}

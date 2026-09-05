using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;

namespace ERP_Government.Application.UnitTests.Accounting;

internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object? Execute(Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        // EF Core 10 passes reduced expressions rooted at plain enumerables
        // (e.g. List<T>.Any(...)). EnumerableQuery.Execute rejects non-queryable
        // roots, so try to compile and invoke directly; if it fails (provider-
        // rooted expression), fall back to the inner provider.
        var syncExpression = AsyncToSyncConverter.ConvertAsync(expression);
        try
        {
            var result = Expression.Lambda(syncExpression).Compile().DynamicInvoke();
            return WrapResult<TResult>(result);
        }
        catch
        {
            // Expression needs the inner provider (e.g. IQueryable-backed queries)
            return _inner.Execute<TResult>(syncExpression);
        }
    }

    private static TResult WrapResult<TResult>(object? result)
    {
        if (result is TResult typed)
        {
            return typed;
        }

        var taskType = typeof(TResult);
        if (result is not null && taskType.IsGenericType && taskType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var elementType = taskType.GetGenericArguments()[0];
            var converted = Convert.ChangeType(result, elementType);
            return (TResult)typeof(Task)
                .GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(elementType)
                .Invoke(null, new[] { converted })!;
        }

        throw new InvalidOperationException(
            $"Cannot convert sync result of type {result?.GetType().Name ?? "null"} to {typeof(TResult).Name}");
    }
}

/// <summary>
/// Converts EF Core async method calls (AnyAsync, FirstOrDefaultAsync, ToListAsync, etc.)
/// to their synchronous equivalents so they can be executed against EnumerableQuery.
/// </summary>
internal class AsyncToSyncConverter : ExpressionVisitor
{
    private static readonly Dictionary<string, string> AsyncToSyncMap = new()
    {
        ["AnyAsync"] = "Any",
        ["AllAsync"] = "All",
        ["FirstAsync"] = "First",
        ["FirstOrDefaultAsync"] = "FirstOrDefault",
        ["SingleAsync"] = "Single",
        ["SingleOrDefaultAsync"] = "SingleOrDefault",
        ["CountAsync"] = "Count",
        ["LongCountAsync"] = "LongCount",
        ["SumAsync"] = "Sum",
        ["MinAsync"] = "Min",
        ["MaxAsync"] = "Max",
        ["AverageAsync"] = "Average",
        ["ContainsAsync"] = "Contains",
        ["ToListAsync"] = "ToList",
        ["ToArrayAsync"] = "ToArray",
        ["ToDictionaryAsync"] = "ToDictionary",
        ["ToHashSetAsync"] = "ToHashSet",
        ["LoadAsync"] = "Load",
        ["ForEachAsync"] = "ForEach",
    };

    public static Expression ConvertAsync(Expression expression)
    {
        return new AsyncToSyncConverter().Visit(expression);
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        // First, recurse into arguments to handle nested async calls
        var visitedArgs = node.Arguments.Select(Visit).Where(e => e != null).Cast<Expression>().ToList();

        // Check if this is an async method from EF or LINQ
        var methodName = node.Method.Name;
        if (AsyncToSyncMap.TryGetValue(methodName, out var syncMethodName))
        {
            // Find the sync method with the same name on the same type
            var asyncDeclaringType = node.Method.DeclaringType;
            if (asyncDeclaringType != null)
            {
                // Try to find the sync method on the same type first (for extension methods)
                var syncMethod = FindSyncMethod(asyncDeclaringType, syncMethodName, node.Method, visitedArgs);
                if (syncMethod != null)
                {
                    // If sync method has fewer params (no CancellationToken), strip last arg
                    var callArgs = syncMethod.GetParameters().Length < visitedArgs.Count
                        ? visitedArgs.Take(syncMethod.GetParameters().Length).ToList()
                        : visitedArgs;
                    return Expression.Call(node.Object, syncMethod, callArgs);
                }

                // For EF extension methods, try the System.Linq.Queryable type
                if (asyncDeclaringType.FullName?.StartsWith("Microsoft.EntityFrameworkCore") == true)
                {
                    var queryableMethod = FindSyncMethod(typeof(Queryable), syncMethodName, node.Method, visitedArgs);
                    if (queryableMethod != null)
                    {
                        // If sync method has fewer params (no CancellationToken), strip last arg
                        var callArgs = queryableMethod.GetParameters().Length < visitedArgs.Count
                            ? visitedArgs.Take(queryableMethod.GetParameters().Length).ToList()
                            : visitedArgs;
                        return Expression.Call(queryableMethod, callArgs);
                    }
                }
            }
        }

        // Handle the ExecuteAsync case (EF Core wraps async calls)
        if (node.Method.Name == "ExecuteAsync" && node.Method.DeclaringType != null
            && node.Method.DeclaringType.FullName?.StartsWith("Microsoft.EntityFrameworkCore") == true)
        {
            // The first argument is the inner expression — unwrap and convert it
            if (visitedArgs.Count > 0)
            {
                return Visit(visitedArgs[0]);
            }
        }

        return Expression.Call(node.Object, node.Method, visitedArgs);
    }

    private static MethodInfo? FindSyncMethod(Type declaringType, string syncName, MethodInfo asyncMethod, List<Expression> args)
    {
        var methods = declaringType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        var asyncParamCount = asyncMethod.GetParameters().Length;

        return methods.FirstOrDefault(m =>
            m.Name == syncName
            && m.GetGenericArguments().Length == asyncMethod.GetGenericArguments().Length
            && (m.GetParameters().Length == asyncParamCount
                || m.GetParameters().Length == asyncParamCount - 1)); // Allow -1 for removed CancellationToken
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable
{
    private readonly IQueryProvider _asyncProvider;

    public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable)
    {
        _asyncProvider = new TestAsyncQueryProvider<T>(((IQueryable)this).Provider);
    }

    public TestAsyncEnumerable(Expression expression) : base(expression)
    {
        _asyncProvider = new TestAsyncQueryProvider<T>(((IQueryable)this).Provider);
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => _asyncProvider;
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> MoveNextAsync()
    {
        return ValueTask.FromResult(_inner.MoveNext());
    }

    public T Current => _inner.Current;
}

internal static class DbSetMockExtensions
{
    public static Mock<DbSet<T>> BuildMockForAsync<T>(this IQueryable<T> queryable) where T : class
    {
        var mockSet = new Mock<DbSet<T>>();

        mockSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));

        mockSet.As<IQueryable<T>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));

        mockSet.As<IQueryable<T>>()
            .Setup(m => m.Expression)
            .Returns(queryable.Expression);

        mockSet.As<IQueryable<T>>()
            .Setup(m => m.ElementType)
            .Returns(queryable.ElementType);

        mockSet.As<IQueryable<T>>()
            .Setup(m => m.GetEnumerator())
            .Returns(queryable.GetEnumerator());

        return mockSet;
    }
}

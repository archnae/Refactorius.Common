using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Refactorius;

/// <summary>
/// Extension method for   rethrowing the inner exception of the <see cref="AggregateException"/>.
/// </summary>
[PublicAPI]
public static class UnwrapAsyncExtensions
{
    /// <summary>
    /// Waits for the task completion and rethrows the inner exception if the task failed. 
    /// </summary>
    /// <param name="task">The task to wait for.</param>
    public static void UnwrapTaskResult(this Task task)
    {
        try
        {
            task.Wait();
        }
        catch (AggregateException ex)
        {
            ExceptionDispatchInfo.Capture(ex.ExtractInnerException()).Throw();
            throw; // to make compiler happy
        }
    }

    /// <summary>
    /// Waits for the task result and rethrows the inner exception if the task failed. 
    /// </summary>
    /// <param name="task">The task to wait for.</param>
    /// <returns>The result of the <paramref name="task"/>.</returns>
    public static T UnwrapTaskResult<T>(this Task<T> task)
    {
        try
        {
            return task.Result;
        }
        catch (AggregateException ex)
        {
            ExceptionDispatchInfo.Capture(ex.ExtractInnerException()).Throw();
            throw; // to make compiler happy
        }
    }
}
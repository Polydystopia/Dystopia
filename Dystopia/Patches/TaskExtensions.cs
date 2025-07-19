namespace Dystopia.Patches;

public static class TaskExtensions
{
    public static async Task<TResult> Then<TSource, TResult>(this Task<TSource> task, Func<TSource, Task<TResult>> then)
    {
        var result = await task;
        return await then(result);
    }
    public static async Task<TResult> Then<TSource, TResult>(this Task<TSource> task, Func<TSource, TResult> then)
    {
        var result = await task;
        return then(result);
    }
}
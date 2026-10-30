using System.Runtime.CompilerServices;

namespace Phrase.Acceptance.Tests.Support;

/// <summary>
/// Many acceptance tests block pool threads while they wait for child processes. The pool then adds threads slowly, and a harness
/// continuation (the child's "Now listening on:" line reaching a waiter) can sit in the queue past a 3 to 20 second startup window
/// although the line was already read. A higher floor keeps those continuations prompt under the parallel full run.
/// </summary>
internal static class ThreadPoolFloor
{
    [ModuleInitializer]
    internal static void Raise()
    {
        ThreadPool.GetMinThreads(out var worker, out var io);
        ThreadPool.SetMinThreads(Math.Max(worker, 128), Math.Max(io, 128));
    }
}

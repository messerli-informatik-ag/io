#if !NET_7_0_OR_GREATER
using System.Runtime.InteropServices;
#endif
using Funcky.RetryPolicies;

namespace Messerli.IO;

public static class Retrier
{
    // The HResult that corresponds with the Win32 ERROR_SHARING_VIOLATION (0x20) code.
    // (HResults include some additional information)
    private const int SharingViolationHResult = unchecked((int)0x80070020);

    // The EWOULDBLOCK error code from POSIX.
    // ReSharper disable once InconsistentNaming
    private const int EWOULDBLOCK = 0xB;

    private static readonly int FileInUseHResult = IsWindows()
        ? SharingViolationHResult
        : EWOULDBLOCK;

    /// <summary><para>Retries an action if a «file is still in use» exception is thrown.</para>
    /// <para>On Windows, this is an <see cref="IOException"/> with the <c>ERROR_SHARING_VIOLATION</c> code
    /// on non-Windows systems, this is an <see cref="IOException"/> with the <c>EWOULDBLOCK</c> code.</para></summary>
    /// <exception cref="IOException">The exception thrown by the action is re-thrown when the retries are exhausted.</exception>
    /// <example>
    /// Appends some text to a file and retries if the file is busy.
    /// <code><![CDATA[
    /// using static Messerli.IO.Retrier;
    ///
    /// var policy = new ConstantDelayPolicy(maxRetries: 10, TimeSpan.FromMilliseconds(20));
    /// await RetryWhileFileIsInUseAsync(policy, () => File.AppendAllLines("busy.txt", contents: Sequence.Return("hello world")));
    /// ]]></code>
    /// </example>
    public static Task RetryWhileFileIsInUseAsync(IRetryPolicy policy, Action action)
        => RetryAsync(ActionToUnit(action), ShouldRetry, policy);

    /// <inheritdoc cref="RetryWhileFileIsInUseAsync"/>
    public static Task<TResult> RetryWhileFileIsInUseAsync<TResult>(IRetryPolicy policy, Func<TResult> func)
        => RetryAsync(func, ShouldRetry, policy);

    private static bool ShouldRetry(Exception exception)
        => exception is IOException && exception.HResult == FileInUseHResult;

    /// <summary>Retries a producer as long as an exception matching the <paramref name="shouldRetry"/> predicate is thrown.
    /// When all retries are exhausted, the exception is propagated to the caller.</summary>
    private static async Task<TResult> RetryAsync<TResult>(
        Func<TResult> producer,
        Func<Exception, bool> shouldRetry,
        IRetryPolicy retryPolicy,
        CancellationToken cancellationToken = default)
    {
        var retryCount = 1;
        while (true)
        {
            try
            {
                return producer();
            }
            catch (Exception exception) when (shouldRetry(exception) && retryCount <= retryPolicy.MaxRetries)
            {
                await Task.Delay(retryPolicy.Delay(retryCount), cancellationToken).ConfigureAwait(false);
                retryCount++;
            }
        }
    }

#if NET_7_0_OR_GREATER
    private static bool IsWindows() => OperatingSystem.IsWindows();
#else
    private static bool IsWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#endif
}

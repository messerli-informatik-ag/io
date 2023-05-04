using Funcky.RetryPolicies;
using Messerli.TempDirectory;
using Xunit;

namespace Messerli.IO.Test;

public sealed class RetrierTest
{
    // This results in a maximum delay of ~ 5 s
    private static readonly IRetryPolicy RetryPolicy = new ExponentialBackOffRetryPolicy(maxRetries: 13, TimeSpan.FromMilliseconds(10));

    [Fact]
    public async Task RetriesOperationUntilFileIsNoLongerBusy()
    {
        using var tempDirectory = TempSubdirectory.Create();
        var filePath = Path.Combine(tempDirectory.FullName, "busy.txt");
        File.WriteAllText(filePath, contents: string.Empty);

        await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Write, FileShare.None);

        const int expectedRetries = 4;
        var retries = 0;
        await Retrier.RetryWhileFileIsInUseAsync(RetryPolicy, () =>
        {
            if (++retries == expectedRetries)
            {
                fileStream.Close();
            }

            using (File.OpenRead(filePath))
            {
            }
        });

        Assert.Equal(expectedRetries, retries);
    }
}

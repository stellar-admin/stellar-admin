using Microsoft.Playwright;

namespace StellarAdmin.ScenarioTesting.Runner;

// Every request the spec's pages make, for expectRequests
public sealed class SpecRequests
{
    private readonly List<IRequest> _requests = [];

    public SpecRequests(IBrowserContext context) =>
        context.Request += (_, request) =>
        {
            lock (_requests)
            {
                _requests.Add(request);
            }
        };

    // Waits until count requests match, or the timeout passes, and returns those that match. The page first
    // runs its pending tasks, so a request an earlier step set off has started
    public async Task<IReadOnlyList<IRequest>> WaitForAsync(
        IPage page,
        Func<string, bool> matches,
        int count,
        float timeout
    )
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeout);
        while (true)
        {
            await page.EvaluateAsync(
                "() => new Promise(resolve => requestAnimationFrame(() => setTimeout(resolve, 50)))"
            );
            var matching = Matching(matches);
            if (matching.Count == count || DateTime.UtcNow >= deadline)
            {
                return matching;
            }
        }
    }

    private List<IRequest> Matching(Func<string, bool> matches)
    {
        lock (_requests)
        {
            return [.. _requests.Where(request => matches(new Uri(request.Url).PathAndQuery))];
        }
    }
}

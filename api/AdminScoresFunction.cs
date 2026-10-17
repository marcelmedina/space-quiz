using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace SpaceQuiz.Api;

public sealed record AdminScoreEntry(string Id, string Name, int Score, int Total, long TimeMs, string Mode, DateTimeOffset Date);

public sealed record AdminScoresResponse(int Total, DateTimeOffset GeneratedAt, IEnumerable<AdminScoreEntry> Entries);

public sealed class AdminScoresFunction(ScoreRepository repository)
{
    private const int DefaultLimit = 200;
    private const int MaxLimit = 1000;

    // The "admin" route prefix is reserved by the Functions host, so the admin feed lives under /api/scores/all.
    [Function("AdminScores")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "options", Route = "scores/all")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        req.HttpContext.Response.Headers.AccessControlAllowOrigin = "*";
        req.HttpContext.Response.Headers.AccessControlAllowMethods = "GET, OPTIONS";
        req.HttpContext.Response.Headers.AccessControlAllowHeaders = "Content-Type";
        req.HttpContext.Response.Headers.CacheControl = "no-store";

        if (HttpMethods.IsOptions(req.Method)) return new NoContentResult();

        var limit = int.TryParse(req.Query["limit"], out var requested) ? Math.Clamp(requested, 1, MaxLimit) : DefaultLimit;
        var all = await repository.GetAllAsync(cancellationToken);
        var entries = all
            .OrderByDescending(e => e.SubmittedAt)
            .Take(limit)
            .Select(e => new AdminScoreEntry(e.RowKey, e.Name, e.Score, e.Total, e.TimeMs, e.Mode, e.SubmittedAt));

        return new OkObjectResult(new AdminScoresResponse(all.Count, DateTimeOffset.UtcNow, entries));
    }
}

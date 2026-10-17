using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace SpaceQuiz.Api;

public sealed record AdminScoreEntry(string Id, string Name, string Email, int Score, int Total, long TimeMs, string Mode, DateTimeOffset Date);

public sealed record AdminScoresResponse(int Total, DateTimeOffset GeneratedAt, IEnumerable<AdminScoreEntry> Entries);

public sealed class AdminScoresFunction(ScoreRepository repository)
{
    private const int DefaultLimit = 200;
    private const int MaxLimit = 1000;
    private static readonly byte[] PasswordHash = Convert.FromHexString("7db2a4aae0c33015a69abaaeb04224c16e2a91aefa143c6056c49772699b9a84");

    // The "admin" route prefix is reserved by the Functions host, so the admin feed lives under /api/scores/all.
    [Function("AdminScores")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "options", Route = "scores/all")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        req.HttpContext.Response.Headers.AccessControlAllowOrigin = "*";
        req.HttpContext.Response.Headers.AccessControlAllowMethods = "GET, OPTIONS";
        req.HttpContext.Response.Headers.AccessControlAllowHeaders = "Content-Type, X-Admin-Password";
        req.HttpContext.Response.Headers.CacheControl = "no-store";

        if (HttpMethods.IsOptions(req.Method)) return new NoContentResult();

        var password = req.Headers["X-Admin-Password"];
        if (password.Count != 1 || string.IsNullOrEmpty(password[0]) || password[0]!.Length > 256 ||
            !CryptographicOperations.FixedTimeEquals(SHA256.HashData(Encoding.UTF8.GetBytes(password[0]!)), PasswordHash))
            return new UnauthorizedObjectResult(new { error = "Incorrect or missing admin password." });

        var limit = int.TryParse(req.Query["limit"], out var requested) ? Math.Clamp(requested, 1, MaxLimit) : DefaultLimit;
        var all = await repository.GetAllAsync(cancellationToken);
        var entries = all
            .OrderByDescending(e => e.Score)
            .ThenBy(e => e.TimeMs)
            .ThenBy(e => e.SubmittedAt)
            .Take(limit)
            .Select(e => new AdminScoreEntry(e.RowKey, e.Name, e.Email, e.Score, e.Total, e.TimeMs, e.Mode, e.SubmittedAt));

        return new OkObjectResult(new AdminScoresResponse(all.Count, DateTimeOffset.UtcNow, entries));
    }
}

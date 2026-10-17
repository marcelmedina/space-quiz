using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace SpaceQuiz.Api;

public sealed record ScoreSubmission(string? Name, string? Email, int? Score, int? Total, double? TimeMs, string? Mode);

public sealed record LeaderboardEntry(string Name, int Score, int Total, long TimeMs, string Mode, DateTimeOffset Date);

public sealed class ScoresFunction(ScoreRepository repository, ILogger<ScoresFunction> logger)
{
    public const int QuestionCount = 10;
    private const int MaxNameLength = 80;
    private const int MaxEmailLength = 254;
    private const long MinTimeMs = 1_000;
    private const long MaxTimeMs = 86_400_000;
    private const int LeaderboardSize = 10;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Function("Scores")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "options", Route = "scores")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        // The quiz is a static page (GitHub Pages or file://), so allow any origin.
        // Platform CORS settings on the Function App, if configured, take precedence.
        req.HttpContext.Response.Headers.AccessControlAllowOrigin = "*";
        req.HttpContext.Response.Headers.AccessControlAllowMethods = "GET, POST, OPTIONS";
        req.HttpContext.Response.Headers.AccessControlAllowHeaders = "Content-Type";

        if (HttpMethods.IsOptions(req.Method)) return new NoContentResult();
        if (HttpMethods.IsGet(req.Method)) return await GetLeaderboard(cancellationToken);
        return await SubmitScore(req, cancellationToken);
    }

    private async Task<IActionResult> GetLeaderboard(CancellationToken cancellationToken)
    {
        var top = await repository.GetTopAsync(LeaderboardSize, cancellationToken);
        return new OkObjectResult(top.Select(ToEntry));
    }

    private async Task<IActionResult> SubmitScore(HttpRequest req, CancellationToken cancellationToken)
    {
        ScoreSubmission? submission;
        try
        {
            submission = await JsonSerializer.DeserializeAsync<ScoreSubmission>(req.Body, JsonOptions, cancellationToken);
        }
        catch (JsonException)
        {
            return Error("The score data is not valid JSON.");
        }

        if (submission is null) return Error("No score data was received.");

        var name = new string((submission.Name ?? "").Where(c => !char.IsControl(c)).ToArray()).Trim();
        if (name.Length is 0 or > MaxNameLength) return Error($"The name must be 1-{MaxNameLength} characters.");
        var email = (submission.Email ?? "").Trim();
        if (email.Length is 0 or > MaxEmailLength || email.Any(c => char.IsControl(c) || char.IsWhiteSpace(c)) ||
            !new EmailAddressAttribute().IsValid(email))
            return Error($"A valid email address of up to {MaxEmailLength} characters is required.");
        if (submission.Score is not { } score || score is < 0 or > QuestionCount)
            return Error($"The score must be a whole number from 0 to {QuestionCount}.");
        if (submission.TimeMs is not { } timeMs || !double.IsFinite(timeMs) || timeMs < MinTimeMs || timeMs > MaxTimeMs)
            return Error("The time must be between 1 second and 24 hours.");

        var entity = new ScoreEntity
        {
            Name = name,
            Email = email,
            Score = score,
            Total = QuestionCount,
            TimeMs = (long)Math.Round(timeMs),
            Mode = submission.Mode == "shuffled" ? "shuffled" : "ordered",
            SubmittedAt = DateTimeOffset.UtcNow
        };

        await repository.AddAsync(entity, cancellationToken);
        logger.LogInformation("Recorded score {Score}/{Total} in {TimeMs} ms", entity.Score, entity.Total, entity.TimeMs);

        return new ObjectResult(ToEntry(entity)) { StatusCode = StatusCodes.Status201Created };
    }

    private static LeaderboardEntry ToEntry(ScoreEntity e) => new(e.Name, e.Score, e.Total, e.TimeMs, e.Mode, e.SubmittedAt);

    private static BadRequestObjectResult Error(string message) => new(new { error = message });
}

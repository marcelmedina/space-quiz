using Azure;
using Azure.Data.Tables;

namespace SpaceQuiz.Api;

public sealed class ScoreEntity : ITableEntity
{
    public string PartitionKey { get; set; } = ScoreRepository.Partition;
    public string RowKey { get; set; } = Guid.NewGuid().ToString("N");
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string Name { get; set; } = "";
    public int Score { get; set; }
    public int Total { get; set; }
    public long TimeMs { get; set; }
    public string Mode { get; set; } = "ordered";
    public DateTimeOffset SubmittedAt { get; set; }
}

public sealed class ScoreRepository(TableClient table)
{
    public const string TableName = "scores";
    public const string Partition = "leaderboard";

    private bool _tableReady;

    private async Task EnsureTableAsync(CancellationToken cancellationToken)
    {
        if (_tableReady) return;
        await table.CreateIfNotExistsAsync(cancellationToken);
        _tableReady = true;
    }

    public async Task AddAsync(ScoreEntity entity, CancellationToken cancellationToken)
    {
        await EnsureTableAsync(cancellationToken);
        await table.AddEntityAsync(entity, cancellationToken);
    }

    public async Task<IReadOnlyList<ScoreEntity>> GetTopAsync(int count, CancellationToken cancellationToken)
    {
        await EnsureTableAsync(cancellationToken);
        var entries = new List<ScoreEntity>();
        await foreach (var entity in table.QueryAsync<ScoreEntity>(e => e.PartitionKey == Partition, cancellationToken: cancellationToken))
            entries.Add(entity);

        return entries
            .OrderByDescending(e => e.Score)
            .ThenBy(e => e.TimeMs)
            .ThenBy(e => e.SubmittedAt)
            .Take(count)
            .ToList();
    }
}

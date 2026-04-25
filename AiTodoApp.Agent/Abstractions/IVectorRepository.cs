namespace AiTodoApp.Agent.Abstractions;

public interface IVectorRepository
{
    Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        string userId,
        string question,
        CancellationToken cancellationToken);
}

public sealed record VectorSearchResult(string Text);

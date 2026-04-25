using AiTodoApp.Agent.Abstractions;
using AiTodoApp.Application.Interfaces;

namespace AiTodoApp.Agent.Services;

public sealed class NoOpVectorRepository(
    ISemanticSearchService semanticSearchService) : IVectorRepository
{
    public async Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        string userId,
        string question,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(userId, out var parsedUserId) || string.IsNullOrWhiteSpace(question))
        {
            return [];
        }

        var search = await semanticSearchService.SearchAsync(
            parsedUserId,
            question,
            5,
            cancellationToken);

        return search.Results
            .Select(x => new VectorSearchResult(
                x.TextBlock))
            .ToList();
    }
}

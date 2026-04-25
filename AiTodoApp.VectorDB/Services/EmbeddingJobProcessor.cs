using AiTodoApp.Messaging.Contracts;

namespace AiTodoApp.VectorDB.Services;

public class EmbeddingJobProcessor
{
    private readonly TextChunker _textChunker;
    private readonly OpenAiEmbeddingService _openAiEmbeddingService;
    private readonly QdrantService _qdrantService;
    private readonly IJobStatusMessageProducer _jobStatusMessageProducer;
    private readonly IConfiguration _configuration;

    public EmbeddingJobProcessor(
        TextChunker textChunker,
        OpenAiEmbeddingService openAiEmbeddingService,
        QdrantService qdrantService,
        IJobStatusMessageProducer jobStatusMessageProducer,
        IConfiguration configuration)
    {
        _textChunker = textChunker;
        _openAiEmbeddingService = openAiEmbeddingService;
        _qdrantService = qdrantService;
        _jobStatusMessageProducer = jobStatusMessageProducer;
        _configuration = configuration;
    }

    public async Task ProcessAsync(EmbeddingJobRequestedMessage message, CancellationToken cancellationToken)
    {
        await _jobStatusMessageProducer.PublishStatusAsync(
            new JobStatusChangedMessage(message.JobId, message.UserId, message.FileName, "Vectorizing", DateTime.UtcNow, null),
            cancellationToken);

        try
        {
            var fullPath = GetFullPath(message.RelativePath);
            var text = await File.ReadAllTextAsync(fullPath, cancellationToken);

            var chunkSize = int.TryParse(_configuration["Embedding:ChunkSize"], out var cs) ? cs : 800;
            var overlap = int.TryParse(_configuration["Embedding:ChunkOverlap"], out var co) ? co : 120;
            var chunks = _textChunker.Chunk(text, chunkSize, overlap);

            var vectors = new List<(int ChunkIndex, string ChunkText, IReadOnlyList<float> Vector)>();
            for (var i = 0; i < chunks.Count; i++)
            {
                var vector = await _openAiEmbeddingService.CreateEmbeddingAsync(chunks[i], cancellationToken);
                vectors.Add((i, chunks[i], vector));
            }

            await _qdrantService.UpsertChunksAsync(
                message.UserId,
                message.JobId,
                message.FileName,
                message.RelativePath,
                vectors,
                cancellationToken);

            await _jobStatusMessageProducer.PublishStatusAsync(
                new JobStatusChangedMessage(message.JobId, message.UserId, message.FileName, "UploadedAndVectorized", DateTime.UtcNow, null),
                cancellationToken);
        }
        catch (Exception ex)
        {
            await _jobStatusMessageProducer.PublishStatusAsync(
                new JobStatusChangedMessage(message.JobId, message.UserId, message.FileName, "Failed", DateTime.UtcNow, ex.Message),
                cancellationToken);
        }
    }

    private string GetFullPath(string relativePath)
    {
        var root = _configuration["Storage:RootPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "..", "AiTodoApp.Api");
        return Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }
}

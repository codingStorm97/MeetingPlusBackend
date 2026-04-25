using AiTodoApp.Application.DTOs;
using AiTodoApp.Application.Interfaces;
using AiTodoApp.Domain.Entities;
using MediatR;

namespace AiTodoApp.Application.Commands.CreateConversation;

public sealed class CreateConversationCommandHandler(
    IConversationRepository conversationRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateConversationCommand, ConversationThreadDto>
{
    public async Task<ConversationThreadDto> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
    {
        var entity = new Conversation
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Title = string.IsNullOrWhiteSpace(request.Title) ? "New conversation" : request.Title.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await conversationRepository.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ConversationThreadDto(entity.Id, entity.Title, entity.CreatedAt, []);
    }
}

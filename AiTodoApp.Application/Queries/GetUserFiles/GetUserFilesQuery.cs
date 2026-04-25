using AiTodoApp.Application.DTOs;
using MediatR;

namespace AiTodoApp.Application.Queries.GetUserFiles;

public record GetUserFilesQuery : IRequest<UserFilesResultDto>;

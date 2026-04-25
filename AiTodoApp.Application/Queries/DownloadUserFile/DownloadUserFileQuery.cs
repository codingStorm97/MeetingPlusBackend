using AiTodoApp.Application.DTOs;
using MediatR;

namespace AiTodoApp.Application.Queries.DownloadUserFile;

public record DownloadUserFileQuery(string FileName) : IRequest<FileDownloadResultDto?>;

using Microsoft.AspNetCore.Http;

namespace AiTodoApp.Api.Contracts;

public class UploadTextFileRequest
{
    public IFormFile? File { get; set; }
}

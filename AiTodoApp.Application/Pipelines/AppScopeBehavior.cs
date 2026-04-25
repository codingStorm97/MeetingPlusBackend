using AiTodoApp.Application.Interfaces;
using MediatR;

namespace AiTodoApp.Application.Pipelines;

public class AppScopeBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IApplicationScopeContext _applicationScopeContext;

    public AppScopeBehavior(IApplicationScopeContext applicationScopeContext)
    {
        _applicationScopeContext = applicationScopeContext;
    }

    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is IAppScopeRequest scopedRequest)
        {
            scopedRequest.UserId = _applicationScopeContext.UserId;
            scopedRequest.UserName = _applicationScopeContext.UserName;
            scopedRequest.UserFolderPath = _applicationScopeContext.UserFolderPath;
        }

        return next(cancellationToken);
    }
}

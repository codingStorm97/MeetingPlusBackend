FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["AiTodoApp.Api/AiTodoApp.Api.csproj", "AiTodoApp.Api/"]
COPY ["AiTodoApp.Application/AiTodoApp.Application.csproj", "AiTodoApp.Application/"]
COPY ["AiTodoApp.Infrastructure/AiTodoApp.Infrastructure.csproj", "AiTodoApp.Infrastructure/"]
COPY ["AiTodoApp.Domain/AiTodoApp.Domain.csproj", "AiTodoApp.Domain/"]

RUN dotnet restore "AiTodoApp.Api/AiTodoApp.Api.csproj"

COPY . .
WORKDIR "/src/AiTodoApp.Api"
RUN dotnet publish "AiTodoApp.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AiTodoApp.Api.dll"]

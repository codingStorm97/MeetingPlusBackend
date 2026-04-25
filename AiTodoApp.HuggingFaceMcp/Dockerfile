FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["AiTodoApp.HuggingFaceMcp/AiTodoApp.HuggingFaceMcp.csproj", "AiTodoApp.HuggingFaceMcp/"]
RUN dotnet restore "AiTodoApp.HuggingFaceMcp/AiTodoApp.HuggingFaceMcp.csproj"

COPY . .
WORKDIR "/src/AiTodoApp.HuggingFaceMcp"
RUN dotnet publish "AiTodoApp.HuggingFaceMcp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 9090

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AiTodoApp.HuggingFaceMcp.dll"]

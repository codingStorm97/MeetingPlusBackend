namespace AiTodoApp.Infrastructure.Queries;

public static class FileHistoryTableCreateQuery
{
    public const string Sql = """
        IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FileHistory')
        BEGIN
            CREATE TABLE [dbo].[FileHistory](
                [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                [UserId] UNIQUEIDENTIFIER NOT NULL,
                [UserName] NVARCHAR(150) NOT NULL,
                [FileName] NVARCHAR(260) NOT NULL,
                [RelativePath] NVARCHAR(512) NOT NULL,
                [UploadedAtUtc] DATETIME2 NOT NULL
            );

            CREATE INDEX [IX_FileHistory_UserId] ON [dbo].[FileHistory]([UserId]);
        END
        """;
}

namespace AiTodoApp.Infrastructure.Queries;

public static class JobTableCreateQuery
{
    public const string Sql = """
        IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'job')
        BEGIN
            CREATE TABLE [dbo].[job](
                [id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                [userId] UNIQUEIDENTIFIER NOT NULL,
                [jobStatus] NVARCHAR(50) NOT NULL,
                [time] DATETIME2 NOT NULL,
                [uploadFileId] UNIQUEIDENTIFIER NOT NULL,
                [uploadFileName] NVARCHAR(260) NOT NULL,
                [relativePath] NVARCHAR(512) NOT NULL,
                [error] NVARCHAR(1024) NULL,
                [attempts] INT NOT NULL DEFAULT 0
            );

            CREATE INDEX [IX_job_userId] ON [dbo].[job]([userId]);
            CREATE INDEX [IX_job_uploadFileId] ON [dbo].[job]([uploadFileId]);
        END
        """;
}

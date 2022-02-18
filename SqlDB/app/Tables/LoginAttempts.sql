CREATE TABLE [app].[LoginAttempts] (
	[ID] BIGINT NOT NULL IDENTITY(1, 1)
        CONSTRAINT [PK_LoginAttempts] PRIMARY KEY

    ,[SubmitDateUTC] DATETIME2 NOT NULL
        CONSTRAINT [DF_LoginAttempts_SubmitDateUTC] DEFAULT (SYSUTCDATETIME())

    ,[EmailAddress] NVARCHAR(200) NOT NULL

    ,[ValidationCode] UNIQUEIDENTIFIER NOT NULL
    ,[ValidateDateUTC] DATETIME2 NULL
);
GO

CREATE INDEX [IX_ValidationCode] ON [app].[LoginAttempts] ([ValidationCode]);
GO

CREATE TABLE dbo.LoginAttempts
(
    LoginAttemptID INT NOT NULL IDENTITY(1, 1)
    ,CONSTRAINT PK_LoginAttempts PRIMARY KEY (LoginAttemptID)

    ,_StartDateTime DATETIME2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL
    ,_EndDateTime DATETIME2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL
    ,PERIOD FOR SYSTEM_TIME (_StartDateTime, _EndDateTime)

    -- When the user submits their email address
    ,SubmitDateUTC DATETIME2 NOT NULL CONSTRAINT DF_LoginAttempts_SubmitDateUTC DEFAULT (SYSUTCDATETIME())
    ,EmailAddress NVARCHAR(200) NOT NULL

    -- The unique key that is sent to their email address to log in
    ,ValidationKey UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_LoginAttempts_ValidationKey DEFAULT (NEWID())

    -- When the user clicks their email link and gets logged in.
    ,ValidationDateUTC DATETIME2 NULL
) WITH (SYSTEM_VERSIONING = ON(HISTORY_TABLE = dbo.LoginAttempts_Hist));
GO

CREATE UNIQUE INDEX IX_LoginAttempts_ValidationKey ON dbo.LoginAttempts (ValidationKey);
GO

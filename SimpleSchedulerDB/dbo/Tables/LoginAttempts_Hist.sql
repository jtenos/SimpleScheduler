CREATE TABLE dbo.LoginAttempts_Hist
(
    LoginAttemptID INT NOT NULL
    ,_StartDateTime DATETIME2 NOT NULL
    ,_EndDateTime DATETIME2 NOT NULL
    ,SubmitDateUTC DATETIME2 NOT NULL
    ,EmailAddress NVARCHAR(200) NOT NULL
    ,ValidationKey UNIQUEIDENTIFIER NOT NULL
    ,ValidationDateUTC DATETIME2 NULL
);
GO

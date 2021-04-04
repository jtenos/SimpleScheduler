CREATE TABLE dbo.LoginAttempts
(
    LoginAttemptID BIGINT NOT NULL IDENTITY(1, 1)
    ,CONSTRAINT PK_LoginAttempts PRIMARY KEY (LoginAttemptID)

    -- Dates are YYYYMMDDHHMMSSFFF
    -- When the user submits their email address
    ,SubmitDateUTC BIGINT NOT NULL
    ,EmailAddress NVARCHAR(200) NOT NULL

    -- The unique key that is sent to their email address to log in
    ,ValidationKey NCHAR(32) NOT NULL
    ,INDEX IX_ValidationKey UNIQUE (ValidationKey)

    -- When the user clicks their email link and gets logged in.
    ,ValidationDateUTC BIGINT NULL
);
GO

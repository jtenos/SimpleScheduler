CREATE TABLE [LoginAttempts]
(
    LoginAttemptID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
    ,EmailAddress TEXT NOT NULL

    -- Dates are YYYYMMDDHHMMSSFFF
    ,SubmitDateUTC INTEGER NOT NULL

    -- The unique key that is sent to their email address to log in
    ,ValidationKey TEXT NOT NULL

    -- When the user clicks their email link and gets logged in.
    ,ValidationDateUTC INTEGER NULL
);

CREATE UNIQUE INDEX IX_LoginAttempts_ValidationKey
ON LoginAttempts (ValidationKey);

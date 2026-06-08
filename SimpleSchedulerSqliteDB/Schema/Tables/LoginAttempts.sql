CREATE TABLE IF NOT EXISTS LoginAttempts (
    ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
    ,SubmitDateUTC TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ', 'now'))
    ,EmailAddress TEXT NOT NULL
    ,ValidationCode TEXT NOT NULL
    ,ValidateDateUTC TEXT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_LoginAttempts_ValidationCode ON LoginAttempts (ValidationCode);

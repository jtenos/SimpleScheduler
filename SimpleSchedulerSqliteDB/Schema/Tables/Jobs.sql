CREATE TABLE IF NOT EXISTS Jobs (
    ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
    ,ScheduleID INTEGER NOT NULL REFERENCES Schedules (ID)
    ,InsertDateUTC TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ', 'now'))
    ,QueueDateUTC TEXT NOT NULL
    ,CompleteDateUTC TEXT NULL
    ,StatusCode TEXT NOT NULL DEFAULT ('NEW')
    -- If this job errors, this is the code to acknowledge the error. SQLite has no NEWID(), so the
    -- default builds a random GUID-formatted string (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).
    -- Uppercase to match how Microsoft.Data.Sqlite binds Guid parameters (used in equality lookups).
    ,AcknowledgementCode TEXT NOT NULL DEFAULT (
        substr(hex(randomblob(4)), 1, 8) || '-' ||
        substr(hex(randomblob(2)), 1, 4) || '-' ||
        substr(hex(randomblob(2)), 1, 4) || '-' ||
        substr(hex(randomblob(2)), 1, 4) || '-' ||
        substr(hex(randomblob(6)), 1, 12)
    )
    ,AcknowledgementDate TEXT NULL
    ,HasDetailedMessage INTEGER NOT NULL DEFAULT (0)
    ,CONSTRAINT CK_Jobs_StatusCode CHECK (
        StatusCode IN ('NEW', 'CAN', 'ERR', 'RUN', 'ACK', 'SUC')
    )
);

CREATE INDEX IF NOT EXISTS IX_Jobs_ScheduleID ON Jobs (ScheduleID);
CREATE INDEX IF NOT EXISTS IX_Jobs_StatusCode ON Jobs (StatusCode);
CREATE INDEX IF NOT EXISTS IX_Jobs_AcknowledgementCode ON Jobs (AcknowledgementCode);

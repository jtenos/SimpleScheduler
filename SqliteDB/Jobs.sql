CREATE TABLE Jobs
(
    JobID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
    ,ScheduleID INTEGER NOT NULL

    -- Dates are YYYYMMDDHHMMSSFFF
    ,InsertDateUTC INTEGER NOT NULL
    ,QueueDateUTC INTEGER NOT NULL
    ,CompleteDateUTC INTEGER NULL

    ,StatusCode TEXT NOT NULL DEFAULT 'NEW'

    ,DetailedMessage TEXT NULL

    -- If this job errors, this is the ID to acknowledge the error
    -- If the job does not error, this is ignored
    ,AcknowledgementID TEXT NOT NULL
    ,AcknowledgementDate BIGINT NULL


    ,FOREIGN KEY ([ScheduleID]) REFERENCES [Schedules] ([ScheduleID])
    ,CHECK (
        [StatusCode] = 'NEW' OR [StatusCode] = 'CAN' OR [StatusCode] = 'ERR'
        OR [StatusCode] = 'RUN' OR [StatusCode] = 'ACK' OR [StatusCode] = 'SUC'
    )
);
CREATE INDEX IX_Jobs_Status ON Jobs (StatusCode);
CREATE INDEX IX_Jobs_ScheduleID ON Jobs (ScheduleID);
CREATE INDEX IX_Jobs_AcknowledgementID ON Jobs (AcknowledgementID);

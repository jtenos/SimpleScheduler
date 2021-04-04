CREATE TABLE dbo.Jobs
(
    JobID BIGINT NOT NULL IDENTITY(1, 1)
    ,CONSTRAINT PK_Jobs PRIMARY KEY (JobID)

    ,ScheduleID BIGINT NOT NULL
    ,CONSTRAINT FK_Jobs_ScheduleID FOREIGN KEY (ScheduleID) REFERENCES dbo.Schedules (ScheduleID)
    ,INDEX IX_ScheduleID (ScheduleID)

    -- Dates are YYYYMMDDHHMMSSFFF
    ,InsertDateUTC BIGINT NOT NULL
    ,QueueDateUTC BIGINT NOT NULL
    ,CompleteDateUTC BIGINT NULL

    ,StatusCode NCHAR(3) NOT NULL CONSTRAINT DF_Jobs_StatusCode DEFAULT ('NEW')
    ,CONSTRAINT CK_Jobs_StatusCode CHECK (
        StatusCode = 'NEW' OR StatusCode = 'CAN' OR StatusCode = 'ERR'
        OR StatusCode = 'RUN' OR StatusCode = 'ACK' OR StatusCode = 'SUC'
    )
    ,INDEX IX_StatusCode (StatusCode)

    ,DetailedMessage NVARCHAR(MAX) NULL

    -- If this job errors, this is the ID to acknowledge the error
    -- If the job does not error, this is ignored
    ,AcknowledgementID NCHAR(32) NOT NULL
    ,INDEX IX_AcknowledgementID (AcknowledgementID)
    ,AcknowledgementDate BIGINT NULL
);
GO

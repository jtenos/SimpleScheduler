CREATE TABLE dbo.Jobs
(
    JobID INT NOT NULL IDENTITY(1, 1)
    ,CONSTRAINT PK_Jobs PRIMARY KEY (JobID)

    ,_StartDateTime DATETIME2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL
    ,_EndDateTime DATETIME2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL
    ,PERIOD FOR SYSTEM_TIME (_StartDateTime, _EndDateTime)

    ,ScheduleID INT NOT NULL
    ,CONSTRAINT FK_Jobs_ScheduleID FOREIGN KEY (ScheduleID) REFERENCES dbo.Schedules (ScheduleID)
    ,INDEX IX_ScheduleID (ScheduleID)

    ,InsertDateUTC DATETIME2 NOT NULL CONSTRAINT DF_Jobs_InsertDateUTC DEFAULT (sysutcdatetime())
    ,QueueDateUTC DATETIME2 NOT NULL
    ,CompleteDateUTC DATETIME2 NULL

    ,StatusCode NCHAR(3) NOT NULL CONSTRAINT DF_Jobs_StatusCode DEFAULT ('NEW')
    ,CONSTRAINT CK_Jobs_StatusCode CHECK (
        StatusCode = 'NEW' OR StatusCode = 'CAN' OR StatusCode = 'ERR'
        OR StatusCode = 'RUN' OR StatusCode = 'ACK' OR StatusCode = 'SUC'
    )

    ,DetailedMessage NVARCHAR(MAX) NULL

    -- If this job errors, this is the ID to acknowledge the error
    -- If the job doesn't error, this is ignored
    ,AcknowledgementID UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Jobs_AcknowledgementID DEFAULT (NEWID())
    ,INDEX IX_AcknowledgementID (AcknowledgementID)
    ,AcknowledgementDate DATETIME2 NULL
    ------------
) WITH (SYSTEM_VERSIONING = ON(HISTORY_TABLE = dbo.Jobs_Hist));
GO

CREATE INDEX IX_Jobs_Status ON dbo.Jobs (StatusCode) INCLUDE (ScheduleID);
GO

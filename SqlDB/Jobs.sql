CREATE TABLE dbo.Jobs
(
    JobID BIGINT NOT NULL IDENTITY(1, 1)
    ,CONSTRAINT PK_Jobs PRIMARY KEY (JobID)

    ,ScheduleID BIGINT NOT NULL
    ,CONSTRAINT FK_Jobs_ScheduleID FOREIGN KEY (ScheduleID) REFERENCES dbo.Schedules (ScheduleID)

    -- Dates are YYYYMMDDHHMMSSFFF
    ,InsertDateUTC BIGINT NOT NULL
    ,QueueDateUTC BIGINT NOT NULL
    ,CompleteDateUTC BIGINT NULL

    ,StatusCode NCHAR(3) NOT NULL CONSTRAINT DF_Jobs_StatusCode DEFAULT ('NEW')
    ,CONSTRAINT CK_Jobs_StatusCode CHECK (
        StatusCode = 'NEW' OR StatusCode = 'CAN' OR StatusCode = 'ERR'
        OR StatusCode = 'RUN' OR StatusCode = 'ACK' OR StatusCode = 'SUC'
    )

    ,DetailedMessage NVARCHAR(MAX) NULL

    -- If this job errors, this is the ID to acknowledge the error
    -- If the job does not error, this is ignored
    ,AcknowledgementID NCHAR(32) NOT NULL
    ,INDEX IX_AcknowledgementID (AcknowledgementID)
    ,AcknowledgementDate BIGINT NULL

    ,DetailedMessageSize BIGINT NOT NULL
        CONSTRAINT DF_Jobs_DetailedMessageSize DEFAULT (0)
);
GO

CREATE INDEX IX_ScheduleID ON dbo.Jobs (
    ScheduleID
) INCLUDE (
    InsertDateUTC, QueueDateUTC, CompleteDateUTC, StatusCode, DetailedMessageSize
);
GO

CREATE INDEX IX_StatusCode ON dbo.Jobs (
    StatusCode
) INCLUDE (
    InsertDateUTC, QueueDateUTC, CompleteDateUTC, DetailedMessageSize
);
GO

CREATE INDEX IX_QueueDateUTC_Desc ON dbo.Jobs (
	QueueDateUTC DESC
) INCLUDE (
    InsertDateUTC, CompleteDateUTC, StatusCode, DetailedMessageSize
);
GO

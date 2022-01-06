CREATE TABLE dbo.JobsArchive
(
    JobArchiveID BIGINT NOT NULL IDENTITY(1, 1)
    ,CONSTRAINT PK_JobsArchive PRIMARY KEY (JobArchiveID)

    ,JobID BIGINT NOT NULL
    ,ScheduleID BIGINT NOT NULL
    ,InsertDateUTC BIGINT NOT NULL
    ,QueueDateUTC BIGINT NOT NULL
    ,CompleteDateUTC BIGINT NULL

    ,StatusCode NCHAR(3) NOT NULL
    ,DetailedMessage VARBINARY(MAX) NULL -- Brotli-compressed
    ,AcknowledgementID NCHAR(32) NOT NULL
    ,AcknowledgementDate BIGINT NULL
    ,DetailedMessageSize BIGINT NOT NULL
);
GO

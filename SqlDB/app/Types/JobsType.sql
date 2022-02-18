CREATE TYPE [app].[JobsType] AS TABLE (
    [ID] BIGINT NOT NULL
    ,[ScheduleID] BIGINT NOT NULL
    ,[InsertDateUTC] DATETIME2 NOT NULL
    ,[QueueDateUTC] DATETIME2 NOT NULL
    ,[CompleteDateUTC] DATETIME2 NULL
    ,[StatusCode] NCHAR(3) NOT NULL
    ,[AcknowledgementCode] UNIQUEIDENTIFIER NOT NULL
    ,[AcknowledgementDate] DATETIME2 NULL
    ,[HasDetailedMessage] BIT NOT NULL
);
GO

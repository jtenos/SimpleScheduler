CREATE TABLE [app].[Jobs]
(
    [ID] BIGINT NOT NULL IDENTITY(1, 1)
        CONSTRAINT [PK_Jobs] PRIMARY KEY

    ,[ScheduleID] BIGINT NOT NULL
        CONSTRAINT [FK_Jobs_ScheduleID]
        FOREIGN KEY REFERENCES [app].[Schedules] ([ID])

    ,[InsertDateUTC] DATETIME2 NOT NULL
        CONSTRAINT [DF_Jobs_InsertDateUTC] DEFAULT (SYSUTCDATETIME())

    ,[QueueDateUTC] DATETIME2 NOT NULL
    ,[CompleteDateUTC] DATETIME2 NULL

    ,[StatusCode] NCHAR(3) NOT NULL
        CONSTRAINT [DF_Jobs_StatusCode] DEFAULT ('NEW')

    ,CONSTRAINT [CK_Jobs_StatusCode] CHECK (
        [StatusCode] = 'NEW' OR [StatusCode] = 'CAN' OR [StatusCode] = 'ERR'
        OR [StatusCode] = 'RUN' OR [StatusCode] = 'ACK' OR [StatusCode] = 'SUC'
    )

    -- If this job errors, this is the ID to acknowledge the error
    -- If the job does not error, this is ignored
    ,[AcknowledgementCode] UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_Jobs_AcknowledgementCode] DEFAULT (NEWID())

    ,[AcknowledgementDate] DATETIME2 NULL

    ,[HasDetailedMessage] BIT NOT NULL
        CONSTRAINT [DF_Jobs_HasDetailedMessage] DEFAULT (0)
);
GO

CREATE INDEX [IX_ScheduleID] ON [app].[Jobs] ([ScheduleID])
    INCLUDE ([InsertDateUTC], [QueueDateUTC], [CompleteDateUTC], [StatusCode], [AcknowledgementCode], [HasDetailedMessage]);
GO

CREATE INDEX [IX_StatusCode] ON [app].[Jobs] ([StatusCode])
    INCLUDE ([ScheduleID], [InsertDateUTC], [QueueDateUTC], [CompleteDateUTC], [AcknowledgementCode], [HasDetailedMessage]);
GO

CREATE INDEX [IX_AcknowledgementCode] ON [app].[Jobs] ([AcknowledgementCode]);
GO

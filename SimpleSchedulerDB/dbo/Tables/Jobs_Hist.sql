CREATE TABLE dbo.Jobs_Hist
(
    JobID INT NOT NULL
    ,_StartDateTime DATETIME2 NOT NULL
    ,_EndDateTime DATETIME2 NOT NULL
    ,ScheduleID INT NOT NULL
    ,InsertDateUTC DATETIME2 NOT NULL
    ,QueueDateUTC DATETIME2 NOT NULL
    ,CompleteDateUTC DATETIME2 NULL
    ,StatusCode NCHAR(3) NOT NULL
    ,DetailedMessage NVARCHAR(MAX) NULL
    ,AcknowledgementID UNIQUEIDENTIFIER NOT NULL
    ,AcknowledgementDate DATETIME2 NULL
);
GO
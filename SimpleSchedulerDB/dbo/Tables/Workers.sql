CREATE TABLE dbo.Workers
(
    WorkerID INT NOT NULL IDENTITY(1, 1)
    ,CONSTRAINT PK_Workers PRIMARY KEY (WorkerID)

    ,_StartDateTime DATETIME2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL
    ,_EndDateTime DATETIME2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL
    ,PERIOD FOR SYSTEM_TIME (_StartDateTime, _EndDateTime)

    ,IsActive BIT NOT NULL CONSTRAINT DF_Workers_IsActive DEFAULT (1)
    ,[Description] NVARCHAR(100) NOT NULL
    ,INDEX IX_Description UNIQUE ([Description])

    ,[FreeText] NVARCHAR(MAX) NOT NULL
    ,EmailOnSuccess NVARCHAR(MAX) NOT NULL
    
    ,ParentWorkerID INT NULL
    ,CONSTRAINT FK_Workers_ParentWorkerID FOREIGN KEY (ParentWorkerID) REFERENCES dbo.Workers (WorkerID)
    
    ,TimeoutMinutes INT NOT NULL
    ,OverdueMinutes INT NOT NULL
    ,DirectoryName NVARCHAR(1000) NOT NULL
    ,[Executable] NVARCHAR(1000) NOT NULL
    ,Arguments NVARCHAR(1000) NOT NULL
) WITH (SYSTEM_VERSIONING = ON(HISTORY_TABLE = dbo.Workers_Hist));
GO

CREATE TABLE dbo.Workers
(
    WorkerID BIGINT NOT NULL IDENTITY(1, 1)
    ,CONSTRAINT PK_Workers PRIMARY KEY (WorkerID)

    ,IsActive BIGINT NOT NULL CONSTRAINT DF_Workers_IsActive DEFAULT (1)
    ,WorkerName NVARCHAR(100) NOT NULL
    ,INDEX IX_WorkerName UNIQUE (WorkerName)

    ,DetailedDescription NVARCHAR(MAX) NOT NULL
    ,EmailOnSuccess NVARCHAR(MAX) NOT NULL
    
    ,ParentWorkerID BIGINT NULL
    ,CONSTRAINT FK_Workers_ParentWorkerID FOREIGN KEY (ParentWorkerID) REFERENCES dbo.Workers (WorkerID)
    
    ,TimeoutMinutes BIGINT NOT NULL
    ,DirectoryName NVARCHAR(1000) NOT NULL
    ,[Executable] NVARCHAR(1000) NOT NULL
    ,ArgumentValues NVARCHAR(1000) NOT NULL
);
GO

CREATE TABLE dbo.Workers_Hist
(
    WorkerID INT NOT NULL
    ,_StartDateTime DATETIME2 NOT NULL
    ,_EndDateTime DATETIME2 NOT NULL
    ,IsActive BIT NOT NULL
    ,[Description] NVARCHAR(100) NOT NULL
    ,[FreeText] NVARCHAR(MAX) NOT NULL
    ,EmailOnSuccess NVARCHAR(MAX) NOT NULL
    ,ParentWorkerID INT NULL
    ,TimeoutMinutes INT NOT NULL
    ,OverdueMinutes INT NOT NULL
    ,DirectoryName NVARCHAR(1000) NOT NULL
    ,[Executable] NVARCHAR(1000) NOT NULL
    ,Arguments NVARCHAR(1000) NOT NULL
);
GO

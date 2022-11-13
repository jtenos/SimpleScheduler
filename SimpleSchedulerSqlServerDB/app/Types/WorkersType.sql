CREATE TYPE [app].[WorkersType] AS TABLE (
	[ID] BIGINT NOT NULL
	,[IsActive] BIT NOT NULL
	,[WorkerName] NVARCHAR(100) NOT NULL
	,[DetailedDescription] NVARCHAR(MAX) NOT NULL
	,[EmailOnSuccess] NVARCHAR(MAX) NOT NULL
	,[ParentWorkerID] BIGINT NULL
	,[TimeoutMinutes] INT NOT NULL
	,[DirectoryName] NVARCHAR(1000) NOT NULL
	,[Executable] NVARCHAR(1000) NOT NULL
	,[ArgumentValues] NVARCHAR(1000) NOT NULL
);
GO

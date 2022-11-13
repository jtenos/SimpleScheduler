CREATE TABLE [app].[Workers] (
	[ID] BIGINT NOT NULL IDENTITY(1, 1)
		CONSTRAINT [PK_Workers] PRIMARY KEY

	,[IsActive] BIT NOT NULL
		CONSTRAINT [DF_Workers_IsActive] DEFAULT (1)
	
	,[WorkerName] NVARCHAR(100) NOT NULL
	,[DetailedDescription] NVARCHAR(MAX) NOT NULL
	,[EmailOnSuccess] NVARCHAR(MAX) NOT NULL
	
	,[ParentWorkerID] BIGINT NULL
		CONSTRAINT [FK_Workers_ParentWorkerID]
		FOREIGN KEY REFERENCES [app].[Workers] ([ID])
	
	,[TimeoutMinutes] INT NOT NULL
	,[DirectoryName] NVARCHAR(1000) NOT NULL
	,[Executable] NVARCHAR(1000) NOT NULL
	,[ArgumentValues] NVARCHAR(1000) NOT NULL
);
GO

CREATE UNIQUE INDEX [IX_WorkerName] ON [app].[Workers] (WorkerName);
GO

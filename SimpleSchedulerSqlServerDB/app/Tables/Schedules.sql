CREATE TABLE [app].[Schedules] (
	[ID] BIGINT NOT NULL IDENTITY(1, 1)
        CONSTRAINT [PK_Schedules] PRIMARY KEY
        
    ,[IsActive] BIT NOT NULL
        CONSTRAINT [DF_Schedules_IsActive] DEFAULT (1)

    ,[WorkerID] BIGINT NOT NULL
        CONSTRAINT [FK_Schedules_WorkerID]
        FOREIGN KEY REFERENCES [app].[Workers] ([ID])

    ,[Sunday] BIT NOT NULL
    ,[Monday] BIT NOT NULL
    ,[Tuesday] BIT NOT NULL
    ,[Wednesday] BIT NOT NULL
    ,[Thursday] BIT NOT NULL
    ,[Friday] BIT NOT NULL
    ,[Saturday] BIT NOT NULL

    ,[TimeOfDayUTC] TIME NULL
    ,[RecurTime] TIME NULL
    ,[RecurBetweenStartUTC] TIME NULL
    ,[RecurBetweenEndUTC] TIME NULL

    ,[OneTime] BIT NOT NULL
        CONSTRAINT [DF_Schedules_OneTime] DEFAULT (0)

    ,CONSTRAINT [CK_Schedules_Time] CHECK ([TimeOfDayUTC] IS NOT NULL OR [RecurTime] IS NOT NULL)
    ,CONSTRAINT [CK_Schedules_RecurStart] CHECK ([RecurBetweenStartUTC] IS NULL OR [RecurTime] IS NOT NULL)
    ,CONSTRAINT [CK_Schedules_RecurEnd] CHECK ([RecurBetweenEndUTC] IS NULL OR [RecurTime] IS NOT NULL)
);
GO

CREATE INDEX [IX_WorkerID] ON [app].[Schedules] ([WorkerID]);
GO

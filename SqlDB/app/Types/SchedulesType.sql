CREATE TYPE [app].[SchedulesType] AS TABLE (
	[ID] BIGINT NOT NULL
    ,[IsActive] BIT NOT NULL
    ,[WorkerID] BIGINT NOT NULL
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
);
GO

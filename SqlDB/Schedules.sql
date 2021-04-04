CREATE TABLE dbo.Schedules
(
    ScheduleID BIGINT NOT NULL IDENTITY(1, 1)
    ,CONSTRAINT PK_Schedules PRIMARY KEY (ScheduleID)

    ,IsActive BIGINT NOT NULL CONSTRAINT DF_Schedules_IsActive DEFAULT (1)

    ,WorkerID BIGINT NOT NULL
    ,CONSTRAINT FK_Schedules_WorkerID FOREIGN KEY (WorkerID) REFERENCES dbo.Workers (WorkerID)
    ,INDEX IX_WorkerID (WorkerID)

    ,Sunday BIGINT NOT NULL
    ,Monday BIGINT NOT NULL
    ,Tuesday BIGINT NOT NULL
    ,Wednesday BIGINT NOT NULL
    ,Thursday BIGINT NOT NULL
    ,Friday BIGINT NOT NULL
    ,Saturday BIGINT NOT NULL

    -- Times are HHMMSSFFF

    ,TimeOfDayUTC BIGINT NULL
    ,RecurTime BIGINT NULL
    ,RecurBetweenStartUTC BIGINT NULL
    ,RecurBetweenEndUTC BIGINT NULL

    ,OneTime BIGINT NOT NULL CONSTRAINT DF_Schedules_OneTime DEFAULT (0)

    ,CONSTRAINT CK_Schedules_Time CHECK (TimeOfDayUTC IS NOT NULL OR RecurTime IS NOT NULL)
    ,CONSTRAINT CK_Schedules_RecurStart CHECK (RecurBetweenStartUTC IS NULL OR RecurTime IS NOT NULL)
    ,CONSTRAINT CK_Schedules_RecurEnd CHECK (RecurBetweenEndUTC IS NULL OR RecurTime IS NOT NULL)
);
GO

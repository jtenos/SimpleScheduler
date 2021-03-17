CREATE TABLE dbo.Schedules
(
    ScheduleID INT NOT NULL IDENTITY(1, 1)
    ,CONSTRAINT PK_Schedules PRIMARY KEY (ScheduleID)

    ,_StartDateTime DATETIME2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL
    ,_EndDateTime DATETIME2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL
    ,PERIOD FOR SYSTEM_TIME (_StartDateTime, _EndDateTime)

    ,IsActive BIT NOT NULL CONSTRAINT DF_Schedules_IsActive DEFAULT (1)

    ,WorkerID INT NOT NULL
    ,CONSTRAINT FK_Schedules_WorkerID FOREIGN KEY (WorkerID) REFERENCES dbo.Workers (WorkerID)
    ,INDEX IX_WorkerID (WorkerID)

    ,Sunday BIT NOT NULL
    ,Monday BIT NOT NULL
    ,Tuesday BIT NOT NULL
    ,Wednesday BIT NOT NULL
    ,Thursday BIT NOT NULL
    ,Friday BIT NOT NULL
    ,Saturday BIT NOT NULL

    ,TimeOfDayUTC TIME NULL
    ,RecurTime TIME NULL
    ,RecurBetweenStartUTC TIME NULL
    ,RecurBetweenEndUTC TIME NULL

    ,OneTime BIT NOT NULL CONSTRAINT DF_Schedules_OneTime DEFAULT (0)

    ,CONSTRAINT CK_Schedules_Time CHECK (TimeOfDayUTC IS NOT NULL OR RecurTime IS NOT NULL)
    ,CONSTRAINT CK_Schedules_RecurStart CHECK (RecurBetweenStartUTC IS NULL OR RecurTime IS NOT NULL)
    ,CONSTRAINT CK_Schedules_RecurEnd CHECK (RecurBetweenEndUTC IS NULL OR RecurTime IS NOT NULL)
) WITH (SYSTEM_VERSIONING = ON(HISTORY_TABLE = dbo.Schedules_Hist));
GO

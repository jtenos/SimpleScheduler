CREATE TABLE Schedules
(
    ScheduleID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
    ,IsActive INTEGER NOT NULL DEFAULT 1

    ,WorkerID INTEGER NOT NULL

    ,Sunday INTEGER NOT NULL
    ,Monday INTEGER NOT NULL
    ,Tuesday INTEGER NOT NULL
    ,Wednesday INTEGER NOT NULL
    ,Thursday INTEGER NOT NULL
    ,Friday INTEGER NOT NULL
    ,Saturday INTEGER NOT NULL

    -- Times are HHMMSSFFF
    ,TimeOfDayUTC INTEGER NULL
    ,RecurTime INTEGER NULL
    ,RecurBetweenStartUTC INTEGER NULL
    ,RecurBetweenEndUTC INTEGER NULL

    ,OneTime INTEGER NOT NULL DEFAULT 0

    ,CHECK (TimeOfDayUTC IS NOT NULL OR RecurTime IS NOT NULL)
    ,CHECK (RecurBetweenStartUTC IS NULL OR RecurTime IS NOT NULL)
    ,CHECK (RecurBetweenEndUTC IS NULL OR RecurTime IS NOT NULL)
    ,FOREIGN KEY (WorkerID) REFERENCES Workers (WorkerID)
);

CREATE INDEX IX_Schedules_WorkerID ON Schedules (WorkerID);

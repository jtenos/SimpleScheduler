CREATE TABLE IF NOT EXISTS Workers (
    ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
    ,IsActive INTEGER NOT NULL DEFAULT (1)
    ,WorkerName TEXT NOT NULL
    ,DetailedDescription TEXT NOT NULL
    ,EmailOnSuccess TEXT NOT NULL
    ,ParentWorkerID INTEGER NULL REFERENCES Workers (ID)
    ,TimeoutMinutes INTEGER NOT NULL
    ,DirectoryName TEXT NOT NULL
    ,Executable TEXT NOT NULL
    ,ArgumentValues TEXT NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_Workers_WorkerName ON Workers (WorkerName);

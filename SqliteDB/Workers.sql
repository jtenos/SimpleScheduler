CREATE TABLE Workers
(
    WorkerID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
    ,IsActive INTEGER NOT NULL DEFAULT 1
    ,WorkerName TEXT NOT NULL
    ,DetailedDescription TEXT NOT NULL
    ,EmailOnSuccess TEXT NOT NULL  
    ,ParentWorkerID INTEGER NULL    
    ,TimeoutMinutes INTEGER NOT NULL
    ,DirectoryName TEXT NOT NULL
    ,Executable TEXT NOT NULL
    ,ArgumentValues TEXT NOT NULL

    ,FOREIGN KEY (ParentWorkerID) REFERENCES Workers (WorkerID)
);

CREATE UNIQUE INDEX IX_Workers_WorkerName
ON Workers (WorkerName);

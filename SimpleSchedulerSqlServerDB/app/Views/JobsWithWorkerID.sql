﻿CREATE VIEW [app].[JobsWithWorkerID]
AS
SELECT j.*, w.[ID] [WorkerID], w.[WorkerName]
FROM [app].[Jobs] j
JOIN [app].[Schedules] s ON j.[ScheduleID] = s.[ID]
JOIN [app].[Workers] w ON s.[WorkerID] = w.[ID];
GO

CREATE VIEW [app].[JobsWithWorkerID]
AS
SELECT
	 j.[ID]
	,j.[ScheduleID]
	,j.[InsertDateUTC]
	,j.[QueueDateUTC]
	,j.[CompleteDateUTC]
	,j.[StatusCode]
	,j.[AcknowledgementCode]
	,j.[AcknowledgementDate]
	,j.[HasDetailedMessage]
	,w.[ID] AS [WorkerID]
	,w.[WorkerName]
FROM [app].[Jobs] j
JOIN [app].[Schedules] s ON j.[ScheduleID] = s.[ID]
JOIN [app].[Workers] w ON s.[WorkerID] = w.[ID];
GO

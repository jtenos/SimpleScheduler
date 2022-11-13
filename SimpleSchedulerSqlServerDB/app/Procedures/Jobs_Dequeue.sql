CREATE PROCEDURE [app].[Jobs_Dequeue]
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
	SET XACT_ABORT, NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

		DECLARE @Now DATETIME2 = SYSUTCDATETIME();

		DECLARE @JobIDs TABLE ([ID] BIGINT);

		-- Retrieve up to five NEW records that are ready to start
		-- If that worker is already running, then exclude it
		DECLARE @FiveRecords TABLE ([JobID] BIGINT, [WorkerID] BIGINT);
		INSERT INTO @FiveRecords ([JobID], [WorkerID])
			SELECT j.[ID], s.[WorkerID]
			FROM [app].[Jobs] j
			JOIN [app].[Schedules] s ON j.[ScheduleID] = s.[ID]
			WHERE j.[StatusCode] = 'NEW'
			AND j.[QueueDateUTC] < @Now
			AND s.[WorkerID] NOT IN (
				SELECT s1.[WorkerID]
				FROM [app].[Jobs] j1
				JOIN [app].[Schedules] s1 ON j1.[ScheduleID] = s1.[ID]
				WHERE j1.[StatusCode] = 'RUN'
			)
			ORDER BY [QueueDateUTC]
			OFFSET 0 ROWS
			FETCH NEXT 5 ROWS ONLY;

		-- Further filter so that if you're trying to run the same worker twice, you'll
		-- only get one of them this time
		DECLARE @FiveRecordsFiltered TABLE ([JobID] BIGINT);
		INSERT INTO @FiveRecordsFiltered ([JobID])
			SELECT MIN([JobID])
			FROM @FiveRecords
			GROUP BY [WorkerID];

		UPDATE [app].[Jobs]
		SET [StatusCode] = 'RUN'
		OUTPUT INSERTED.[ID] INTO @JobIDs
		WHERE [ID] IN (SELECT [JobID] FROM @FiveRecordsFiltered);

		SELECT
			 [ID]
			,[ScheduleID]
			,[InsertDateUTC]
			,[QueueDateUTC]
			,[CompleteDateUTC]
			,[StatusCode]
			,[AcknowledgementCode]
			,[AcknowledgementDate]
			,[HasDetailedMessage]
			,[WorkerID]
			,[WorkerName]
		FROM [app].[JobsWithWorkerID]
		WHERE [ID] IN (SELECT [ID] FROM @JobIDs);

		SELECT
			 [ID]
			,[IsActive]
			,[WorkerName]
			,[DetailedDescription]
			,[EmailOnSuccess]
			,[ParentWorkerID]
			,[TimeoutMinutes]
			,[DirectoryName]
			,[Executable]
			,[ArgumentValues]
		FROM [app].[Workers]
		WHERE [ID] IN (
			SELECT s.[WorkerID]
			FROM [app].[Schedules] s
			JOIN [app].[Jobs] j ON s.[ID] = j.[ScheduleID]
			JOIN @JobIDs j1 ON j.[ID] = j1.[ID]
		);

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
		DECLARE @Msg NVARCHAR(2048) = ERROR_MESSAGE();
		RAISERROR(@Msg, 16, 1);
		RETURN 55555;
	END CATCH;
END;
GO

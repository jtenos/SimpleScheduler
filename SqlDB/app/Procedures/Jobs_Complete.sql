CREATE PROCEDURE [app].[Jobs_Complete] (
	@ID BIGINT
	,@Success BIT
	,@HasDetailedMessage BIT
)
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
	SET XACT_ABORT, NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

        UPDATE [app].[Jobs]
        SET
            [StatusCode] = CASE @Success WHEN 1 THEN 'SUC' WHEN 0 THEN 'ERR' END
            ,[HasDetailedMessage] = @HasDetailedMessage
        WHERE [ID] = @ID;

		IF @Success = 1
		BEGIN
			DECLARE @WorkerID BIGINT;
			SELECT @WorkerID = s.[WorkerID]
			FROM [app].[Jobs] j
			JOIN [app].[Schedules] s ON j.[ScheduleID] = s.[ID]
			WHERE j.[ID] = @ID;

			IF OBJECT_ID('tempdb..#ChildWorkers') IS NOT NULL
				DROP TABLE #ChildWorkers;

			CREATE TABLE #ChildWorkers ([ID] BIGINT);
			INSERT INTO #ChildWorkers ([ID])
				SELECT [ID]
				FROM [app].[Workers]
				WHERE [ParentWorkerID] = @WorkerID
				AND [IsActive] = 1;

			-- Add one-time schedules if they don't already exist
			INSERT INTO [app].[Schedules] (
				[IsActive], [WorkerID]
				,[Sunday], [Monday], [Tuesday], [Wednesday], [Thursday], [Friday], [Saturday]
				,[TimeOfDayUTC], [OneTime]
			)
			SELECT
				0, [ID]
				,1, 1, 1, 1, 1, 1, 1
				,'00:00', 1
			FROM #ChildWorkers
			WHERE [ID] NOT IN (
				SELECT TOP 1 [ID] FROM [app].[Schedules]
				WHERE [OneTime] = 1
			);

			-- Insert jobs for each of the one-time schedules
			INSERT INTO [app].[Jobs] ([ScheduleID], [QueueDateUTC])
				SELECT MAX([ID]), SYSUTCDATETIME()
				FROM [app].[Schedules]
				WHERE [OneTime] = 1
				AND [WorkerID] IN (SELECT [ID] FROM #ChildWorkers)
				GROUP BY [WorkerID]
		END;

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

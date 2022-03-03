CREATE PROCEDURE [app].[Jobs_RunNow]
	@WorkerIDs [app].[BigIntArray] READONLY
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
	SET XACT_ABORT, NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

		-- Add one-time schedules if they don't already exist
		INSERT INTO [app].[Schedules] (
			[IsActive], [WorkerID]
			,[Sunday], [Monday], [Tuesday], [Wednesday], [Thursday], [Friday], [Saturday]
			,[TimeOfDayUTC], [OneTime]
		)
		SELECT
			0, [Value]
			,1, 1, 1, 1, 1, 1, 1
			,'00:00', 1
		FROM @WorkerIDs
		WHERE [Value] NOT IN (
			SELECT TOP 1 [ID] FROM [app].[Schedules]
			WHERE [OneTime] = 1
		);

		-- Insert jobs for each of the one-time schedules
		INSERT INTO [app].[Jobs] ([ScheduleID], [QueueDateUTC])
			SELECT MAX([ID]), SYSUTCDATETIME()
			FROM [app].[Schedules]
			WHERE [OneTime] = 1
			AND [WorkerID] IN (SELECT [Value] FROM @WorkerIDs)
			GROUP BY [WorkerID];

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

CREATE PROCEDURE [app].[JobsWithWorker_Select]
	@ID BIGINT
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
	SET XACT_ABORT, NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

		SELECT * FROM [Jobs]
        WHERE [ID] = @ID;

		SELECT w.* FROM [app].[Workers] w
        JOIN [app].[Schedules] s ON w.[ID] = s.[WorkerID]
        JOIN [app].[Jobs] j ON s.[ID] = j.[ScheduleID]
        WHERE j.[ID] = @ID;

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

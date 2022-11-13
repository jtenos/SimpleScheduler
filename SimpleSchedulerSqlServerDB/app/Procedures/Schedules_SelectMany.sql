CREATE PROCEDURE [app].[Schedules_SelectMany]
	@IDs [app].[BigIntArray] READONLY
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
	SET XACT_ABORT, NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

		SELECT
			 [ID]
			,[IsActive]
			,[WorkerID]
			,[Sunday]
			,[Monday]
			,[Tuesday]
			,[Wednesday]
			,[Thursday]
			,[Friday]
			,[Saturday]
			,[TimeOfDayUTC]
			,[RecurTime]
			,[RecurBetweenStartUTC]
			,[RecurBetweenEndUTC]
			,[OneTime]
		FROM [app].[Schedules]
		WHERE [ID] IN (SELECT [ID] FROM @IDs);

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

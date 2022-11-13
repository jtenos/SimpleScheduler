CREATE PROCEDURE [app].[Jobs_SelectMostRecentBySchedule]
	@ScheduleID BIGINT
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
	SET XACT_ABORT, NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

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
		FROM [Jobs]
		WHERE ScheduleID = @ScheduleID
		ORDER BY QueueDateUTC DESC
		OFFSET 0 ROWS FETCH NEXT 1 ROW ONLY;

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

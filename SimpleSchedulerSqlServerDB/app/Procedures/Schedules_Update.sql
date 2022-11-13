CREATE PROCEDURE [app].[Schedules_Update]
	@ID BIGINT
	,@Sunday BIT
	,@Monday BIT
	,@Tuesday BIT
	,@Wednesday BIT
	,@Thursday BIT
	,@Friday BIT
	,@Saturday BIT
	,@TimeOfDayUTC TIME
	,@RecurTime TIME
	,@RecurBetweenStartUTC TIME
	,@RecurBetweenEndUTC TIME
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
	SET XACT_ABORT, NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

		UPDATE [app].[Schedules] SET
			[Sunday] = @Sunday
			,[Monday] = @Monday
			,[Tuesday] = @Tuesday
			,[Wednesday] = @Wednesday
			,[Thursday] = @Thursday
			,[Friday] = @Friday
			,[Saturday] = @Saturday
			,[TimeOfDayUTC] = @TimeOfDayUTC
			,[RecurTime] = @RecurTime
			,[RecurBetweenStartUTC] = @RecurBetweenStartUTC
			,[RecurBetweenEndUTC] = @RecurBetweenEndUTC
		WHERE [ID] = @ID;

		-- Clears out the job queue so it will create the next one at the right time
		DELETE FROM [app].[Jobs] WHERE [ScheduleID] = @ID AND [StatusCode] = 'NEW';

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

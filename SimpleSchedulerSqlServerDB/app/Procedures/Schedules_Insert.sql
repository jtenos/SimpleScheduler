CREATE PROCEDURE [app].[Schedules_Insert]
	@WorkerID BIGINT
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

		INSERT INTO [app].[Schedules] (
			[WorkerID]
			,[Sunday], [Monday], [Tuesday], [Wednesday], [Thursday], [Friday], [Saturday]
			,[TimeOfDayUTC], [RecurTime], [RecurBetweenStartUTC], [RecurBetweenEndUTC], [OneTime]
		) VALUES (
			@WorkerID
			,@Sunday, @Monday, @Tuesday, @Wednesday, @Thursday, @Friday, @Saturday
			,@TimeOfDayUTC, @RecurTime, @RecurBetweenStartUTC, @RecurBetweenEndUTC, CAST(0 AS BIT)
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

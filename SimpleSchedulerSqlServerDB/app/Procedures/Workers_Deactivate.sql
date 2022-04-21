CREATE PROCEDURE [app].[Workers_Deactivate]
    @ID BIGINT
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
	SET XACT_ABORT, NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

		UPDATE [app].[Schedules] SET [IsActive] = 0 WHERE [WorkerID] = @ID;

        DECLARE @WorkerName NVARCHAR(100);
        SELECT @WorkerName = [WorkerName] FROM [app].[Workers] WHERE [ID] = @ID;

        DECLARE @NewWorkerName NVARCHAR(150) = N'INACTIVE: ' + FORMAT(SYSUTCDATETIME(), 'yyyyMMddHHmmss')
            + ' ' + @WorkerName;

        SET @NewWorkerName = LTRIM(RTRIM(LEFT(@NewWorkerName, 100)));

		UPDATE [app].[Workers]
		SET [IsActive] = 0, [WorkerName] = @NewWorkerName
		WHERE [ID] = @ID;

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

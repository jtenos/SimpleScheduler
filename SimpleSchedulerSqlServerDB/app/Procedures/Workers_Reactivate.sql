﻿CREATE PROCEDURE [app].[Workers_Reactivate]
	@ID BIGINT
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
	SET XACT_ABORT, NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

		DECLARE @WorkerName NVARCHAR(100);
		SELECT @WorkerName = [WorkerName] FROM [app].[Workers] WHERE [ID] = @ID;

		IF @WorkerName LIKE 'INACTIVE: ______________ %'
			SET @WorkerName = LTRIM(RTRIM(SUBSTRING(@WorkerName, 26, 100)));

		UPDATE [app].[Workers]
		SET [IsActive] = 1, [WorkerName] = @WorkerName
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

-- TODO: Move this into regular Update proc

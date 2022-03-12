﻿CREATE PROCEDURE [app].[Schedules_SelectForWorker]
	@WorkerID BIGINT
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
	SET XACT_ABORT, NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

        SELECT * FROM [app].[Schedules] 
		WHERE [OneTime] = 0 
		AND [WorkerID] = @WorkerID 
		AND [IsActive] = 1;

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
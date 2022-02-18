﻿CREATE PROCEDURE [app].[Schedules_Deactivate] (
	@ID BIGINT
)
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
	SET XACT_ABORT, NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

		UPDATE [app].[Schedules]
		SET [IsActive] = 0
		WHERE [ID] = @ID;

		DELETE FROM [app].[Jobs] 
		WHERE [ScheduleID] = @ID
		AND [StatusCode] = 'NEW';

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

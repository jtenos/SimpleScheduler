﻿CREATE PROCEDURE [app].[Schedules_SelectAll]
	@IncludeInactive BIT = 0
	,@IncludeOneTime BIT = 0
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
	SET XACT_ABORT, NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

		IF @IncludeInactive = 1 AND @IncludeOneTime = 1
			SELECT * FROM [app].[Schedules];
		ELSE IF @IncludeInactive = 1 AND @IncludeOneTime = 0
			SELECT * FROM [app].[Schedules] WHERE [OneTime] = 0;
		ELSE
			SELECT * FROM [app].[Schedules] WHERE [OneTime] = 0 AND [IsActive] = 1;

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

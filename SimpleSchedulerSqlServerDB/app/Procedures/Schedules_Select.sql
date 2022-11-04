﻿CREATE PROCEDURE [app].[Schedules_Select]
	 @IDs NVARCHAR(MAX) = NULL -- [123,456]
	,@WorkerIDs NVARCHAR(MAX) = NULL -- [789,1234]
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
	SET XACT_ABORT, NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

        DECLARE @SQL NVARCHAR(MAX) = N' ';

		IF @IDs IS NOT NULL
		BEGIN
			SET @SQL += N'
				IF OBJECT_ID(''tempdb..#IDs'') IS NOT NULL
					DROP TABLE #IDs;

				SELECT [ID] INTO #IDs FROM OPENJSON(@IDs) WITH ([ID] BIGINT ''$'');
			';
		END;

		IF @WorkerIDs IS NOT NULL
		BEGIN
			SET @SQL += N'
				IF OBJECT_ID(''tempdb..#WorkerIDs'') IS NOT NULL
					DROP TABLE #WorkerIDs;

				SELECT [WorkerID] INTO #WorkerIDs FROM OPENJSON(@WorkerIDs) WITH ([WorkerID] BIGINT ''$'');
			';
		END;

		SET @SQL += N' SELECT * FROM [app].[Schedules] WHERE 1 = 1 ';

		IF @IDs IS NOT NULL
			SET @SQL += N' AND [ID] IN (SELECT [ID] FROM #IDs) ';
		IF @WorkerIDs IS NOT NULL
			SET @SQL += N' AND [WorkerID] IN (SELECT [WorkerID] FROM #WorkerIDs) ';

		SET @SQL += ';';

		EXECUTE sp_executesql
			@SQL
			,N'
				 @IDs NVARCHAR(MAX)
				,@WorkerIDs NVARCHAR(MAX)
			'
			,@IDs = @IDs
			,@WorkerIDs = @WorkerIDs;

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

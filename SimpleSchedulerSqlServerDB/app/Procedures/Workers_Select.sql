CREATE PROCEDURE [app].[Workers_Select]
	 @IDs NVARCHAR(MAX) = NULL -- JSON: [123,456]
	,@ParentWorkerID BIGINT = NULL
	,@WorkerName NVARCHAR(100) = NULL
	,@DirectoryName NVARCHAR(1000) = NULL
	,@Executable NVARCHAR(1000) = NULL
	,@ActiveOnly BIT = NULL
	,@InactiveOnly BIT = NULL
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
	SET XACT_ABORT, NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

		DECLARE @SQL NVARCHAR(MAX) = N'
			IF OBJECT_ID(''tempdb..#Workers'') IS NOT NULL
				DROP TABLE #Workers;
		';

		IF @IDs IS NOT NULL
		BEGIN
			SET @SQL += N'
				IF OBJECT_ID(''tempdb..#IDs'') IS NOT NULL
					DROP TABLE #IDs;

				SELECT [ID] INTO #IDs FROM OPENJSON(@IDs) WITH ([ID] BIGINT ''$'');
			';
		END;

		SET @SQL += N' SELECT * FROM [app].[Workers] WHERE 1 = 1 ';

		IF @ParentWorkerID IS NOT NULL
			SET @SQL += N' AND [ParentWorkerID] = @ParentWorkerID ';
		IF @WorkerName IS NOT NULL
			SET @SQL += N' AND [WorkerName] LIKE ''%'' + @WorkerName + ''%''';
		IF @DirectoryName IS NOT NULL
			SET @SQL += N' AND [DirectoryName] LIKE ''%'' + @DirectoryName + ''%''';
		IF @Executable IS NOT NULL
			SET @SQL += N' AND [Executable] LIKE ''%'' + @Executable + ''%''';
		IF @ActiveOnly = 1
			SET @SQL += N' AND [IsActive] = 1 ';
		IF @InactiveOnly = 1
			SET @SQL += N' AND [IsActive] = 0 ';
		IF @IDs IS NOT NULL
			SET @SQL += N' AND [ID] IN (SELECT [ID] FROM #IDs) ';

		SET @SQL += N';';

		EXECUTE sp_executesql
			@SQL
			,N'
				 @IDs NVARCHAR(MAX)
				,@ParentWorkerID BIGINT
				,@WorkerName NVARCHAR(100)
				,@DirectoryName NVARCHAR(1000)
				,@Executable NVARCHAR(1000)
			'
			,@IDs = @IDs
			,@ParentWorkerID = @ParentWorkerID
			,@WorkerName = @WorkerName
			,@DirectoryName = @DirectoryName
			,@Executable = @Executable;

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

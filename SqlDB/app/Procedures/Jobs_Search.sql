CREATE PROCEDURE [app].[Jobs_Search] (
	@StatusCode NCHAR(3) = NULL
    ,@WorkerID BIGINT = NULL
    ,@OverdueOnly BIT = 0
    ,@Offset INT = 0
    ,@NumRows INT = 100
)
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
	SET XACT_ABORT, NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

        IF OBJECT_ID('tempdb..#Jobs') IS NOT NULL
            DROP TABLE #Jobs;

	    SELECT * INTO #Jobs FROM [app].[Jobs] WHERE 1 = 0;

        DECLARE @SQL NVARCHAR(MAX) = N'
            INSERT INTO #Jobs
		    SELECT * FROM [app].[Jobs]
            WHERE 1 = 1
        ';

        IF @StatusCode IS NOT NULL
            SET @SQL += N'
                AND [StatusCode] = @StatusCode
            ';

        IF @WorkerID IS NOT NULL
            SET @SQL += N'
                AND [ScheduleID] IN (SELECT [ScheduleID] FROM [app].[Schedules] WHERE [WorkerID] = @WorkerID)
            ';

        IF @OverdueOnly = 1
            SET @SQL += N'
                AND [StatusCode] IN (''ERR'', ''NEW'', ''RUN'')
            ';

        SET @SQL += N'
            ORDER BY [QueueDateUTC] DESC
            OFFSET ' + CAST(@Offset AS NVARCHAR(10)) + N' ROWS
            FETCH NEXT ' + CAST(@NumRows AS NVARCHAR(10)) + N' ROWS ONLY;
        ';

        EXEC sp_executesql
            @SQL
            ,N'@StatusCode NCHAR(3), @WorkerID BIGINT'
            ,@StatusCode = @StatusCode
            ,@WorkerID = @WorkerID;

        SELECT * FROM #Jobs;

        IF EXISTS (SELECT TOP 1 1 FROM #Jobs)
        BEGIN

            DECLARE @Schedules [app].[SchedulesType];
            INSERT @Schedules
            SELECT * FROM [app].[Schedules]
            WHERE [ID] IN (SELECT [ScheduleID] FROM #Jobs);

            SELECT * FROM @Schedules;

            DECLARE @Workers [app].[WorkersType];
            INSERT @Workers
            SELECT * FROM [app].[Workers]
            WHERE [ID] IN (SELECT s.[WorkerID] FROM @Schedules s);

            SELECT * FROM @Workers;
        END;

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

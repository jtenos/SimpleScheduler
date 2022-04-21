CREATE PROCEDURE [app].[Jobs_Search]
	@StatusCode NCHAR(3) = NULL
    ,@WorkerID BIGINT = NULL
    ,@OverdueOnly BIT = 0
    ,@Offset INT = 0
    ,@NumRows INT = 100
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
	SET XACT_ABORT, NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

        DECLARE @SQL NVARCHAR(MAX) = N'
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

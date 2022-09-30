CREATE PROCEDURE [app].[Jobs_Search]
	@StatusCode NCHAR(3) = NULL
    ,@WorkerID BIGINT = NULL
    ,@WorkerName NVARCHAR(100) = NULL
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
		    SELECT * FROM [app].[JobsWithWorkerID]
            WHERE 1 = 1
        ';

        IF @StatusCode IS NOT NULL
            SET @SQL += N'
                AND [StatusCode] = @StatusCode
            ';

        IF @WorkerID IS NOT NULL
            SET @SQL += N'
                AND [WorkerID] = @WorkerID
            ';

        IF @WorkerName IS NOT NULL
            SET @SQL += N'
                AND [WorkerName] LIKE ''%'' + @WorkerName + ''%''
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
            ,N'@StatusCode NCHAR(3), @WorkerID BIGINT, @WorkerName NVARCHAR(100)'
            ,@StatusCode = @StatusCode
            ,@WorkerID = @WorkerID
            ,@WorkerName = @WorkerName;

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

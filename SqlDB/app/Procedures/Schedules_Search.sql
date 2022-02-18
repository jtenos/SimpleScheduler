CREATE PROCEDURE [app].[Schedules_Search] (
    @GetActive BIT
    ,@GetInactive BIT
    ,@GetOneTime BIT
)
)
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
	SET XACT_ABORT, NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

        IF OBJECT_ID('tempdb..#Schedules') IS NOT NULL
            DROP TABLE #Schedules;

	    SELECT * INTO #Schedules FROM [app].[Schedules] WHERE 1 = 0;

        DECLARE @SQL NVARCHAR(MAX) = N'
            INSERT INTO #Schedules
		    SELECT * FROM [app].[Schedules]
            WHERE 1 = 1
        ';

        IF @GetOneTime = 0
            SET @SQL += N'
                AND [OneTime] = 0
            ';

        IF @GetActive = 1 AND @GetInactive = 0
            SET @SQL += N'
                AND [IsActive] = 1
            ';
        
        IF @GetActive = 0 AND GetInactive = 1
            SET @SQL += N'
                AND [IsActive] = 0
            ';

        SET @SQL += N';';

        EXEC sp_executesql
            @SQL
            ,N'@GetActive BIT, @GetInactive BIT, @GetOneTime BIT'
            ,@GetActive = @GetActive
            ,@GetInactive = @GetInactive
            ,@GetOneTime = @GetOneTime;

        SELECT * FROM #Schedules;

        IF EXISTS (SELECT TOP 1 1 FROM #Schedules)
        BEGIN

            DECLARE @Workers [app].[WorkersType];
            INSERT @Workers
            SELECT * FROM [app].[Workers]
            WHERE [ID] IN (SELECT s.[WorkerID] FROM #Schedules s);

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

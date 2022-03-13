CREATE PROCEDURE [app].[Jobs_Dequeue]
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
	SET XACT_ABORT, NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

        DECLARE @Now DATETIME2 = SYSUTCDATETIME();

        DECLARE @Jobs [app].[JobsType];

        ;WITH five_records AS (
            SELECT *
            FROM [app].[Jobs] WITH (ROWLOCK, READPAST, UPDLOCK)
            WHERE [StatusCode] = 'NEW'
            AND [QueueDateUTC] < @Now
            ORDER BY [QueueDateUTC]
            OFFSET 0 ROWS
            FETCH NEXT 5 ROWS ONLY
        )
        UPDATE five_records
        SET [StatusCode] = 'RUN'
        OUTPUT INSERTED.* INTO @Jobs
        FROM five_records WITH (ROWLOCK, READPAST, UPDLOCK);

        SELECT * FROM [app].[JobsWithWorkerID]
        WHERE [ID] IN (SELECT [ID] FROM @Jobs);

        SELECT w.* FROM [app].[Workers] w
        JOIN [app].[Schedules] s ON w.[ID] = s.[WorkerID]
        JOIN [app].[Jobs] j ON s.[ID] = j.[ScheduleID]
        JOIN @Jobs j1 ON j.[ID] = j1.[ID];

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

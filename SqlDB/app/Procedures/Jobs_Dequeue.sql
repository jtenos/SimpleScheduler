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

        SELECT * FROM @Jobs;

        IF EXISTS (SELECT TOP 1 1 FROM @Jobs)
        BEGIN

            DECLARE @Schedules [app].[SchedulesType];
            INSERT @Schedules
            SELECT * FROM [app].[Schedules]
            WHERE [ID] IN (SELECT [ScheduleID] FROM @Jobs);

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

CREATE PROCEDURE [app].[Jobs_Complete]
	@ID BIGINT
	,@Success BIT
	,@HasDetailedMessage BIT
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
	SET XACT_ABORT, NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

        UPDATE [app].[Jobs]
        SET
            [StatusCode] = CASE @Success WHEN 1 THEN 'SUC' WHEN 0 THEN 'ERR' END
            ,[HasDetailedMessage] = @HasDetailedMessage
        WHERE [ID] = @ID;

		IF @Success = 1
		BEGIN
			DECLARE @WorkerID BIGINT;
			SELECT @WorkerID = s.[WorkerID]
			FROM [app].[Jobs] j
			JOIN [app].[Schedules] s ON j.[ScheduleID] = s.[ID]
			WHERE j.[ID] = @ID;

			DECLARE @ChildWorkerIDs [app].[BigIntArray];
			INSERT INTO @ChildWorkerIDs ([Value], [SortOrder])
				SELECT [ID], [ID]
				FROM [app].[Workers]
				WHERE [ParentWorkerID] = @WorkerID
				AND [IsActive] = 1;

			EXEC [app].[Jobs_RunNow]
				@WorkerIDs = @ChildWorkerIDs;

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

CREATE PROCEDURE [app].[Workers_CheckForCircularReference]
    @WorkerID BIGINT
    ,@IsCircularReference BIT OUTPUT
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
	SET XACT_ABORT, NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

        DECLARE @DescendantWorkerIDs TABLE (
            [ID] BIGINT
        );

        INSERT INTO @DescendantWorkerIDs VALUES (@WorkerID);

        DECLARE @ParentWorkerID BIGINT;
        SELECT @ParentWorkerID = [ParentWorkerID] FROM [app].[Workers] WHERE [ID] = @WorkerID;

        WHILE @ParentWorkerID IS NOT NULL
        BEGIN
            IF EXISTS (SELECT TOP 1 1 FROM @DescendantWorkerIDs WHERE [ID] = @ParentWorkerID)
            BEGIN
                SET @IsCircularReference = 1;
                RETURN;
            END;
            INSERT INTO @DescendantWorkerIDs VALUES (@ParentWorkerID);
            SELECT @ParentWorkerID = [ParentWorkerID] FROM [app].[Workers] WHERE [ID] = @ParentWorkerID;
        END;

        SET @IsCircularReference = 0;

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

-- TODO: Remove this proc and do it in code after running Workers_Select [all]

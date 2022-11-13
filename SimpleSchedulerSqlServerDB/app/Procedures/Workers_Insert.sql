CREATE PROCEDURE [app].[Workers_Insert]
    @WorkerName NVARCHAR(100)
    ,@DetailedDescription NVARCHAR(MAX)
    ,@EmailOnSuccess NVARCHAR(100)
    ,@ParentWorkerID BIGINT
    ,@TimeoutMinutes INT
    ,@DirectoryName NVARCHAR(1000)
    ,@Executable NVARCHAR(1000)
    ,@ArgumentValues NVARCHAR(1000)
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
	SET XACT_ABORT, NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

        IF EXISTS (
            SELECT TOP 1 1 FROM [app].[Workers] WHERE [WorkerName] = @WorkerName
        )
        BEGIN
            SELECT CAST(0 AS BIT) [Success], CAST(1 AS BIT) [NameAlreadyExists], CAST(0 AS BIT) [CircularReference];
            RETURN;
        END;

        DECLARE @ID BIGINT;

        INSERT INTO [app].[Workers] (
            [WorkerName], [DetailedDescription], [EmailOnSuccess], [ParentWorkerID]
            ,[TimeoutMinutes], [DirectoryName], [Executable], [ArgumentValues]
        ) VALUES (
            @WorkerName, @DetailedDescription, @EmailOnSuccess, @ParentWorkerID
            ,@TimeoutMinutes, @DirectoryName, @Executable, @ArgumentValues
        );

        SELECT @ID = SCOPE_IDENTITY();

        DECLARE @IsCircularReference BIT;
        EXEC [app].[Workers_CheckForCircularReference]
            @WorkerID = @ID
            ,@IsCircularReference = @IsCircularReference OUTPUT;

        IF @IsCircularReference = 1
        BEGIN
    		IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
            SELECT CAST(0 AS BIT) [Success], CAST(0 AS BIT) [NameAlreadyExists], CAST(1 AS BIT) [CircularReference];
            RETURN;
        END;

        SELECT @ID [ID], CAST(1 AS BIT) [Success], CAST(0 AS BIT) [NameAlreadyExists], CAST(0 AS BIT) [CircularReference];

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

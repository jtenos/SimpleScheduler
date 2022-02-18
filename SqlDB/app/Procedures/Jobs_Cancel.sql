CREATE PROCEDURE [app].[Jobs_Cancel] (
    @ID BIGINT
)
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
	SET XACT_ABORT, NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

		UPDATE [app].[Jobs]
        SET [StatusCode] = 'CAN'
        WHERE [ID] = @ID
        AND [StatusCode] = 'NEW';

        DECLARE @NewStatusCode NCHAR(3);
        SELECT @NewStatusCode = [StatusCode]
        FROM [app].[Jobs]
        WHERE [ID] = @ID;

        IF @NewStatusCode = 'CAN'
            SELECT CAST(1 AS BIT) [Success], CAST(0 AS BIT) [AlreadyCompleted], CAST(0 AS BIT) [AlreadyStarted];
        ELSE IF @NewStatusCode IN ('ERR', 'ACK', 'SUC')
            SELECT CAST(0 AS BIT) [Success], CAST(1 AS BIT) [AlreadyCompleted], CAST(1 AS BIT) [AlreadyStarted];
        ELSE IF @NewStatusCode = 'RUN'
            SELECT CAST(0 AS BIT) [Success], CAST(0 AS BIT) [AlreadyCompleted], CAST(1 AS BIT) [AlreadyStarted];

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

CREATE PROCEDURE [app].[Jobs_AcknowledgeError]
	@AcknowledgementCode UNIQUEIDENTIFIER
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
	SET XACT_ABORT, NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

		IF NOT EXISTS (
			SELECT TOP 1 1 FROM app.[Jobs]
			WHERE [AcknowledgementCode] = @AcknowledgementCode
		)
		BEGIN
			RAISERROR('Job not found', 16, 99);
			RETURN;
		END;

		IF EXISTS (
			SELECT TOP 1 1 FROM app.[Jobs]
			WHERE [AcknowledgementCode] = @AcknowledgementCode
			AND [StatusCode] = 'ACK'
		)
		BEGIN
			RAISERROR('Error already acknowledged', 16, 99);
			RETURN;
		END;

		UPDATE app.[Jobs]
		SET [StatusCode] = 'ACK'
		WHERE [AcknowledgementCode] = @AcknowledgementCode
		AND [StatusCode] = 'ERR';

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

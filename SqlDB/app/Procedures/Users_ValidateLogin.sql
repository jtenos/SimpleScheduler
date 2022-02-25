CREATE PROCEDURE [app].[Users_ValidateLogin]
    @ValidationCode UNIQUEIDENTIFIER
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
	SET XACT_ABORT, NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

        DECLARE @ID BIGINT;
        DECLARE @SubmitDateUTC DATETIME2;
        DECLARE @EmailAddress NVARCHAR(200);

        SELECT TOP 1
            @ID = [ID]
            ,@SubmitDateUTC = [SubmitDateUTC]
            ,@EmailAddress = [EmailAddress]
        FROM [app].[LoginAttempts]
        WHERE [ValidationCode] = @ValidationCode
		AND [ValidateDateUTC] IS NULL;

        IF @ID IS NULL
        BEGIN
            SELECT CAST(0 AS BIT) [Success], @EmailAddress [EmailAddress], CAST(1 AS BIT) [NotFound], CAST(0 AS BIT) [Expired];
            RETURN;
        END;

        DECLARE @MinDate DATETIME2 = DATEADD(MINUTE, -5, SYSUTCDATETIME());
        IF @SubmitDateUTC < @MinDate
        BEGIN
            SELECT CAST(0 AS BIT) [Success], @EmailAddress [EmailAddress], CAST(0 AS BIT) [NotFound], CAST(1 AS BIT) [Expired];
            RETURN;
        END;

        UPDATE [app].[LoginAttempts]
        SET [ValidateDateUTC] = SYSUTCDATETIME()
        WHERE [ID] = @ID;

        SELECT CAST(1 AS BIT) [Success], @EmailAddress [EmailAddress], CAST(0 AS BIT) [NotFound], CAST(0 AS BIT) [Expired];

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

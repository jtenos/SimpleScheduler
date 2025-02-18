﻿CREATE PROCEDURE [app].[Users_SubmitLogin]
	@EmailAddress NVARCHAR(200)
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
	SET XACT_ABORT, NOCOUNT ON;

	BEGIN TRY
		BEGIN TRANSACTION;

        IF NOT EXISTS (
            SELECT TOP 1 1 FROM [app].[Users] WHERE [EmailAddress] = @EmailAddress
        )
        BEGIN
            SELECT CAST(0 AS BIT) [Success], CAST('00000000-0000-0000-0000-000000000000' AS UNIQUEIDENTIFIER) [ValidationCode]
            RETURN;
        END;

        DECLARE @ValidationCode UNIQUEIDENTIFIER = NEWID();

        INSERT INTO [app].[LoginAttempts] (
            [EmailAddress], [ValidationCode]
        ) VALUES (
            @EmailAddress, @ValidationCode
        );

		SELECT CAST(1 AS BIT) [Success], @ValidationCode [ValidationCode];

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

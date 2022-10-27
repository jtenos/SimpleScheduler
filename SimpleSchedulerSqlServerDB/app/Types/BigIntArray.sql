CREATE TYPE [app].[BigIntArray] AS TABLE (
	[Value] BIGINT NOT NULL
	,[SortOrder] INT NOT NULL
);

-- TODO: Eliminate this and use JSON instead

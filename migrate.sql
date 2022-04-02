-- This script will migrate data from the previous version of SimpleScheduler:
-- This assumes the old database name is Sched_Dev, and the new one is Scheduler_Dev.
-- Replace database names with your environment:

USE [Sched_Dev]
GO
CREATE FUNCTION [dbo].[ParseDateFromNumber] (
	@Value BIGINT
)
RETURNS DATETIME2
AS
BEGIN
	DECLARE
		@Year INT, @Month INT, @Day INT
		,@Hour INT, @Minute INT, @Second INT, @Millisecond INT;

	DECLARE @ValAsString CHAR(17) = CAST(@Value AS CHAR(17));
	SET @Year = CAST(LEFT(@ValAsString, 4) AS INT);
	SET @Month = CAST(SUBSTRING(@ValAsString, 5, 2) AS INT);
	SET @Day = CAST(SUBSTRING(@ValAsString, 7, 2) AS INT);
	SET @Hour = CAST(SUBSTRING(@ValAsString, 9, 2) AS INT);
	SET @Minute = CAST(SUBSTRING(@ValAsString, 11, 2) AS INT);
	SET @Second = CAST(SUBSTRING(@ValAsString, 13, 2) AS INT);
	SET @Millisecond = CAST(RIGHT(@ValAsString, 3) AS INT);

	RETURN CAST (
		FORMAT(@Year, '0000')
		+ '-' + FORMAT(@Month, '00')
		+ '-' + FORMAT(@Day, '00')
		+ ' ' + FORMAT(@Hour, '00')
		+ ':' + FORMAT(@Minute, '00')
		+ ':' + FORMAT(@Second, '00')
		+ '.' + FORMAT(@Millisecond, '000')
	    AS DATETIME2
	);
END;
GO
CREATE FUNCTION [dbo].[ParseGuid](
	@Value NCHAR(32)
)
RETURNS UNIQUEIDENTIFIER
AS
BEGIN
	RETURN CAST(
		LEFT(@Value, 8)
		+ '-' + SUBSTRING(@Value, 9, 4)
		+ '-' + SUBSTRING(@Value, 13, 4)
		+ '-' + SUBSTRING(@Value, 17, 4)
		+ '-' + RIGHT(@Value, 12)
		AS UNIQUEIDENTIFIER
	);
END;
GO
CREATE FUNCTION [dbo].[ParseTimeFromNumber] (
	@Value BIGINT
)
RETURNS TIME
AS
BEGIN
	DECLARE
		@Hour INT, @Minute INT, @Second INT, @Millisecond INT;

	DECLARE @ValAsString CHAR(9) = FORMAT(@Value, '000000000');
	SET @Hour = CAST(LEFT(@ValAsString, 2) AS INT);
	SET @Minute = CAST(SUBSTRING(@ValAsString, 3, 2) AS INT);
	SET @Second = CAST(SUBSTRING(@ValAsString, 5, 2) AS INT);
	SET @Millisecond = CAST(RIGHT(@ValAsString, 3) AS INT);

	RETURN CAST (
		FORMAT(@Hour, '00')
		+ ':' + FORMAT(@Minute, '00')
		+ ':' + FORMAT(@Second, '00')
		+ '.' + FORMAT(@Millisecond, '000')
	    AS DATETIME2
	);
END;
GO


-- USERS
INSERT INTO [Scheduler_Dev].[app].[Users] ([EmailAddress])
SELECT [EmailAddress] FROM [Sched_Dev].[dbo].[Users];

-- LOGIN ATTEMPTS
INSERT INTO [Scheduler_Dev].[app].[LoginAttempts] (
	[SubmitDateUTC]
	,[EmailAddress]
	,[ValidationCode]
	,[ValidateDateUTC]
)
SELECT
	[Sched_Dev].[dbo].[ParseDateFromNumber]([SubmitDateUTC])
	,[EmailAddress]
	,[Sched_Dev].[dbo].[ParseGuid]([ValidationKey])
	,[Sched_Dev].[dbo].[ParseDateFromNumber]([ValidationDateUTC])
FROM [Sched_Dev].[dbo].[LoginAttempts];

-- WORKERS
SET IDENTITY_INSERT [Scheduler_Dev].[app].[Workers] ON;

INSERT INTO [Scheduler_Dev].[app].[Workers] (
	[ID], [IsActive], [WorkerName], [DetailedDescription]
	,[EmailOnSuccess], [ParentWorkerID], [TimeoutMinutes]
	,[DirectoryName], [Executable], [ArgumentValues]
)
SELECT
	[WorkerID], CASE [IsActive] WHEN 1 THEN 1 ELSE 0 END, [WorkerName], [DetailedDescription]
	,[EmailOnSuccess], [ParentWorkerID], [TimeoutMinutes]
	,[DirectoryName], [Executable], [ArgumentValues]
FROM [Sched_Dev].[dbo].[Workers];

SET IDENTITY_INSERT [Scheduler_Dev].[app].[Workers] OFF;

-- SCHEDULES
SET IDENTITY_INSERT [Scheduler_Dev].[app].[Schedules] ON;

INSERT INTO [Scheduler_Dev].[app].[Schedules] (
	[ID], [IsActive], [WorkerID]
	,[Sunday], [Monday], [Tuesday], [Wednesday], [Thursday], [Friday], [Saturday]
	,[TimeOfDayUTC], [RecurTime], [RecurBetweenStartUTC], [RecurBetweenEndUTC], [OneTime]
)
SELECT
	[ScheduleID], CASE [IsActive] WHEN 1 THEN 1 ELSE 0 END, [WorkerID]
	,CASE [Sunday] WHEN 1 THEN 1 ELSE 0 END
	,CASE [Monday] WHEN 1 THEN 1 ELSE 0 END
	,CASE [Tuesday] WHEN 1 THEN 1 ELSE 0 END
	,CASE [Wednesday] WHEN 1 THEN 1 ELSE 0 END
	,CASE [Thursday] WHEN 1 THEN 1 ELSE 0 END
	,CASE [Friday] WHEN 1 THEN 1 ELSE 0 END
	,CASE [Saturday] WHEN 1 THEN 1 ELSE 0 END
	,[Sched_Dev].[dbo].[ParseTimeFromNumber]([TimeOfDayUTC])
	,[Sched_Dev].[dbo].[ParseTimeFromNumber]([RecurTime])
	,[Sched_Dev].[dbo].[ParseTimeFromNumber]([RecurBetweenStartUTC])
	,[Sched_Dev].[dbo].[ParseTimeFromNumber]([RecurBetweenEndUTC])
	,CASE [OneTime] WHEN 1 THEN 1 ELSE 0 END
FROM [Sched_Dev].[dbo].[Schedules];

SET IDENTITY_INSERT [Scheduler_Dev].[app].[Schedules] OFF;

-- JOBS
SET IDENTITY_INSERT [Scheduler_Dev].[app].[Jobs] ON;
INSERT INTO [Scheduler_Dev].[app].[Jobs] (
	[ID], [ScheduleID], [InsertDateUTC], [QueueDateUTC], [CompleteDateUTC]
	,[StatusCode], [AcknowledgementCode], [AcknowledgementDate], [HasDetailedMessage]
)
SELECT
	[JobID]
	,[ScheduleID]
	,[Sched_Dev].[dbo].[ParseDateFromNumber]([InsertDateUTC])
	,[Sched_Dev].[dbo].[ParseDateFromNumber]([QueueDateUTC])
	,[Sched_Dev].[dbo].[ParseDateFromNumber]([CompleteDateUTC])
	,[StatusCode]
	,[Sched_Dev].[dbo].[ParseGuid]([AcknowledgementID])
	,[Sched_Dev].[dbo].[ParseDateFromNumber]([AcknowledgementDate])
	,CASE WHEN NULLIF(LTRIM(RTRIM([DetailedMessage])), '') IS NOT NULL THEN 1 ELSE 0 END
FROM [Sched_Dev].[dbo].[Jobs];
SET IDENTITY_INSERT [Scheduler_Dev].[app].[Jobs] OFF;

-- Configure and run the MigrateDetailedMessages application to pull
-- detailed messages into the new filesystem structure

/*
Post-Deployment Script Template                            
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.        
 Use SQLCMD syntax to include a file in the post-deployment script.            
 Example:      :r .\myfile.sql                                
 Use SQLCMD syntax to reference a variable in the post-deployment script.        
 Example:      :setvar TableName MyTable                            
               SELECT * FROM [$(TableName)]                    
--------------------------------------------------------------------------------------
*/
-- Junk script for testing data

if not exists (SELECT
        1
    FROM dbo.Workers)
BEGIN
INSERT dbo.Workers (IsActive, WorkerName, DetailedDescription, dbo.Workers.EmailOnSuccess, dbo.Workers.ParentWorkerID,
TimeoutMinutes,
OverdueMinutes, DirectoryName, dbo.Workers.Executable, dbo.Workers.ArgumentValues)
    VALUES (1, 'hello', 'hello there, this is my job', '', NULL, 20, 20, 'hello', 'hello.exe', 'config.json')


INSERT dbo.Workers (IsActive, WorkerName, DetailedDescription, dbo.Workers.EmailOnSuccess, dbo.Workers.ParentWorkerID,
TimeoutMinutes,
OverdueMinutes, DirectoryName, dbo.Workers.Executable, dbo.Workers.ArgumentValues)
    VALUES (1, 'good morning', '', 'joe.enos@azdhs.gov', NULL, 20, 20, 'good-morning', 'morning.exe', '')
END;

UPDATE dbo.Workers
SET ParentWorkerID = 1
WHERE WorkerID = 2
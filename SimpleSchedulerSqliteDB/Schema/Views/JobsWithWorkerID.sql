CREATE VIEW IF NOT EXISTS JobsWithWorkerID
AS
SELECT j.*, w.ID AS WorkerID, w.WorkerName
FROM Jobs j
JOIN Schedules s ON j.ScheduleID = s.ID
JOIN Workers w ON s.WorkerID = w.ID;

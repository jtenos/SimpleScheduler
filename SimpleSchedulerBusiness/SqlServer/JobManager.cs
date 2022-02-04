using System;
using SimpleSchedulerData;

namespace SimpleSchedulerBusiness.SqlServer
{
    public class JobManager
        : JobManagerBase
    {
        public JobManager(DatabaseFactory databaseFactory, IServiceProvider serviceProvider)
            : base(databaseFactory, serviceProvider) { }

        protected override string DequeueQuery => @"
            DECLARE @Result TABLE (JobID INT);
            ;WITH five_records AS (
                SELECT JobID, StatusCode
                FROM dbo.Jobs WITH (ROWLOCK, READPAST, UPDLOCK)
                WHERE StatusCode = 'NEW'
                AND QueueDateUTC < @Now
                ORDER BY QueueDateUTC
                OFFSET 0 ROWS
                FETCH NEXT 5 ROWS ONLY
            )
            UPDATE five_records
            SET StatusCode = 'RUN'
            OUTPUT INSERTED.JobID INTO @Result
            FROM five_records WITH (ROWLOCK, READPAST, UPDLOCK)

            SELECT j.* 
            FROM dbo.Jobs j
            JOIN @Result r on j.JobID = r.JobID;
        ";
    }
}

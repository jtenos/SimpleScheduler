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
            ;WITH three_records AS (
                SELECT JobID, StatusCode
                FROM dbo.Jobs WITH (ROWLOCK, READPAST, UPDLOCK)
                WHERE StatusCode = 'NEW'
                AND QueueDateUTC < @Now
                ORDER BY QueueDateUTC
                OFFSET 0 ROWS
                FETCH NEXT 3 ROWS ONLY
            )
            UPDATE three_records
            SET StatusCode = 'RUN'
            OUTPUT INSERTED.JobID INTO @Result
            FROM three_records WITH (ROWLOCK, READPAST, UPDLOCK)

            SELECT j.* 
            FROM dbo.Jobs j
            JOIN @Result r on j.JobID = r.JobID;
        ";
    }
}

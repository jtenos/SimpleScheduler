using System;
using SimpleSchedulerData;

namespace SimpleSchedulerBusiness.Sqlite
{
    public class JobManager
        : JobManagerBase
    {
        public JobManager(DatabaseFactory databaseFactory, IServiceProvider serviceProvider)
            : base(databaseFactory, serviceProvider) { }

        protected override string DequeueQuery => @"
            CREATE TABLE temp.Result(JobID INTEGER, StatusCode TEXT);
            INSERT INTO temp.Result
                SELECT JobID
                FROM Jobs
                WHERE StatusCode = 'NEW'
                AND QueueDateUTC < @Now
                ORDER BY QueueDateUTC
                LIMIT 3 OFFSET 0;

            UPDATE Jobs
            SET StatusCode = 'RUN'
            WHERE JobID IN (SELECT JobID FROM temp.Result);

            SELECT j.* 
            FROM Jobs j
            JOIN temp.Result r ON j.JobID = r.JobID;
        ";

    }
}

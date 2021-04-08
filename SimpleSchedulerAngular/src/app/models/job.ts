export type Job = {
    jobID: number,
    scheduleID: number,
    insertDateUTC: Date,
    queueDateUTC: Date,
    completeDateUTC: Date | null,
    statusCode: string, 
    detailedMessage: string | null,
    acknowledgementID: string, 
    acknowledgementDate: Date | null
};

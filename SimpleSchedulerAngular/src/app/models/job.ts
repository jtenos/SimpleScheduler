export type Job = {
    jobID: number,
    scheduleID: number,
    insertDateUTC: string,
    queueDateUTC: string,
    completeDateUTC: string | null,
    statusCode: string, 
    detailedMessage: string | null,
    acknowledgementID: string, 
    acknowledgementDate: Date | null,
    durationInSeconds: number | null
};

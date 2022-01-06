export type Job = {
    jobID: number,
    scheduleID: number,
    insertDateUTC: string,
    queueDateUTC: string,
    completeDateUTC: string | null,
    statusCode: string, 
    detailedMessage: string | null,
    detailedMessageSize: number,
    acknowledgementID: string, 
    acknowledgementDate: Date | null,
    friendlyDuration: string | null
};

import TimeSpan from "./timespan";

export type Schedule = {
    scheduleID: number,
    isActive: boolean,
    workerID: number,
    sunday: boolean,
    monday: boolean,
    tuesday: boolean,
    wednesday: boolean,
    thursday: boolean,
    friday: boolean,
    saturday: boolean,
    timeOfDayUTC: TimeSpan | null,
    recurTime: TimeSpan | null,
    recurBetweenStartUTC: TimeSpan | null,
    recurBetweenEndUTC: TimeSpan | null,
    oneTime: boolean
};

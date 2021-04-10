import { Worker } from "./worker";
import { Schedule } from "./schedule";

export type ScheduleDetail = {
    schedule: Schedule,
    worker: Worker
};

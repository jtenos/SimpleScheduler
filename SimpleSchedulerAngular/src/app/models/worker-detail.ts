import { Worker } from "./worker";
import { Schedule } from "./schedule";

export type WorkerDetail = {
    worker: Worker,
    parentWorker: Worker | null,
    schedules: Schedule[]
};

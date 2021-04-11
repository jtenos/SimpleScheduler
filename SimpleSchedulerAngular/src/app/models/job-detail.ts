import { Job } from "./job";
import { Schedule } from "./schedule";
import { Worker } from "./worker";

export type JobDetail = {
    job: Job,
    schedule: Schedule,
    worker: Worker
}
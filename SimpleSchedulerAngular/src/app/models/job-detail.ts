import { Job } from "./job";
import { Schedule } from "./schedule";
import { Worker } from "./worker";

export type JobDetail = {
    Job: Job,
    Schedule: Schedule,
    Worker: Worker
}
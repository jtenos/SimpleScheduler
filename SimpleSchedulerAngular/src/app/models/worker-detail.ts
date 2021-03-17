import Worker from "./worker";
import Schedule from "./schedule";

export default class WorkerDetail {
    constructor(public worker: Worker, public schedules: Schedule[]){}
}

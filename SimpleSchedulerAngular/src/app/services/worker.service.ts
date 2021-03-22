import { Injectable } from "@angular/core";
import Worker from "../models/worker";
import WorkerDetail from "../models/worker-detail";
import { ApiService } from "./api.service";
import Kvp from "../models/kvp";
@Injectable({
    providedIn: "root"
})
export class WorkerService {

    constructor(private apiService: ApiService) { }

    async getAllWorkers(): Promise<WorkerDetail[]> {
        return await this.apiService.get<WorkerDetail[]>("Workers", "GetAllWorkers", []);
    }

    async getWorker(workerID: number): Promise<Worker> {
        return await this.apiService.get<Worker>("Workers", "GetWorker", [new Kvp("workerID", workerID.toString())]);
    }

    async deleteWorker(workerID: number): Promise<boolean> {
        //this.workers = this.workers.filter(x => x.workerID !== workerID);
        return true;
    }
}

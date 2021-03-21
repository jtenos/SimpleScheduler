import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import WorkerDetail from "../models/worker-detail";
import { of } from "rxjs";
import { ApiService } from "./api.service";
import Kvp from "../models/kvp";
@Injectable({
    providedIn: "root"
})
export class WorkerService {

    constructor(private apiService: ApiService) { }

    async getAllWorkers(): Promise<WorkerDetail[]> {
        console.log("in getAllWorkers");
        return await this.apiService.get<WorkerDetail[]>("Workers", "GetAllWorkers", []);
    }

    async getWorker(workerID: number): Promise<WorkerDetail> {
        return await this.apiService.get<WorkerDetail>("Workers", "GetWorker", [new Kvp("workerID", workerID.toString())]);
    }

    async deleteWorker(workerID: number): Promise<boolean> {
        //this.workers = this.workers.filter(x => x.workerID !== workerID);
        return true;
    }
}

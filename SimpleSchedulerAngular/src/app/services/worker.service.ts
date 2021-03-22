import { Injectable } from "@angular/core";
import Worker from "../models/worker";
import WorkerDetail from "../models/worker-detail";
import { ApiService } from "./api.service";
import Kvp from "../models/kvp";
import { Router } from "@angular/router";
@Injectable({
    providedIn: "root"
})
export class WorkerService {

    constructor(private apiService: ApiService, private router: Router) { }


    async getAllWorkers(): Promise<WorkerDetail[]> {
        return await this.apiService.get<WorkerDetail[]>("Workers", "GetAllWorkers", []);
    }

    async getWorker(workerID: number): Promise<Worker> {
        return await this.apiService.get<Worker>("Workers", "GetWorker", [new Kvp("workerID", workerID.toString())]);
    }

    async deleteWorker(workerID: number): Promise<boolean> {
        await this.apiService.post("Workers", "DeleteWorker", { workerID });
        return true;
    }

    async reactivateWorker(workerID: number): Promise<boolean> {
        await this.apiService.post("Workers", "ReactivateWorker", { workerID });
        return true;
    }

    async saveWorker(worker: Worker): Promise<void> {
        try {
            await this.apiService.post("Workers", "SaveWorker", worker);
            this.router.navigateByUrl("workers");
        } catch (ex) {
            alert(ex);
        }
    }
}

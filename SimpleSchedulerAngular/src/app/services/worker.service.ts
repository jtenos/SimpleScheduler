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

    getAllWorkers(): Observable<WorkerDetail[]> {
        return this.apiService.get<WorkerDetail[]>("Workers", []);
    }

    getWorker(workerID: number): Observable<Worker[]> {
        return this.apiService.get<Worker[]>("Workers", [new Kvp("workerID", workerID.toString())]);
    }

    deleteWorker(workerID: number): Observable<boolean> {
        //this.workers = this.workers.filter(x => x.workerID !== workerID);
        return of(true);
    }
}

import { Injectable } from "@angular/core";
import { Job } from "../models/job";
import { JobDetail } from "../models/job-detail";
import { ApiService } from "./api.service";
import Kvp from "../models/kvp";
import { Router } from "@angular/router";
@Injectable({
    providedIn: "root"
})
export class JobService {

    constructor(private apiService: ApiService, private router: Router) { }

    async getJobs(parms: {
        workerID: number | null,
        statusCode: string | null
    }): Promise<JobDetail[]> {
        const kvps: Kvp<string, string>[] = [];
        if (parms.workerID) {
            kvps.push(new Kvp<string, string>("workerID", parms.workerID.toString()));
        }
        if (parms.statusCode) {
            kvps.push(new Kvp<string, string>("statusCode", parms.statusCode));
        }
        return await this.apiService.get<JobDetail[]>("Jobs", "GetJobs", kvps);
    }

    async getDetailedMessage(jobID: number): Promise<string> {
        return (await this.apiService.post("Jobs", "GetDetailedMessage", {
            jobID
        })).message;
    }

    async cancelJob(jobID: number): Promise<string> {
        try {
            await this.apiService.post("Jobs", "CancelJob", { jobID });
            return "";
        } catch (ex) {
            return ex.message;
        }
    }

    async acknowledgeError(jobID: number): Promise<string> {
        try {
            await this.apiService.post("Jobs", "AcknowledgeError", { jobID });
            return "";
        } catch (ex) {
            return ex.message;
        }
    }
}

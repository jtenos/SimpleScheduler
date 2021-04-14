import { Component, OnInit } from '@angular/core';
import { JobService } from 'src/app/services/job.service';
import { WorkerService } from 'src/app/services/worker.service';
import { Worker } from "../../models/worker";
import { JobDetail } from "../../models/job-detail";

@Component({
    selector: 'app-jobs',
    templateUrl: './jobs.component.html',
    styles: [`:host {
        width: 100%;
    
        .card {
            width: 100%;
        }
      }
    `]
})
export class JobsComponent implements OnInit {

    jobDetails!: JobDetail[];
    workerID: number | null = null;
    statusCode: string | null = null;
    allWorkers!: Worker[];
    header: string = "";

    loading = false;
    constructor(private jobService: JobService, private workerService: WorkerService) {
    }

    ngOnInit(): void {
        this.refreshJobs();
    }

    refresh() {
        this.refreshJobs();
    }

    async refreshJobs(): Promise<void> {
        this.loading = true;
        this.jobDetails = await this.jobService.getJobs({
          workerID: this.workerID,
          statusCode: this.statusCode
        });
        this.allWorkers = (await this.workerService.getAllWorkers()).map(w => w.worker);
        this.allWorkers.sort((x, y) => x.workerName.localeCompare(y.workerName));
        this.loading = false;
    }
}

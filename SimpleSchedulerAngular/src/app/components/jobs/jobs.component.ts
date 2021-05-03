import { Component, OnInit } from '@angular/core';
import { JobService } from 'src/app/services/job.service';
import { WorkerService } from 'src/app/services/worker.service';
import { Worker } from "../../models/worker";
import { JobDetail } from "../../models/job-detail";
import { ActivatedRoute } from '@angular/router';

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
    constructor(private route: ActivatedRoute, private jobService: JobService, private workerService: WorkerService) {
    }

    ngOnInit(): void {
        this.route.params.subscribe(async params => {
            if (params.workerID) {
                this.workerID = params.workerID;
            }
            this.refreshJobs();
        });
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
        this.allWorkers.sort((x, y) => {
            if (x.isActive && y.isActive) {
                return x.workerName.localeCompare(y.workerName)
            }
            if (!x.isActive && !y.isActive) {
                return x.workerName.localeCompare(y.workerName)
            }
            if (x.isActive && !y.isActive) {
                return -1;
            }
            if (!x.isActive && y.isActive) {
                return 1;
            }
            return 0;
        });
        this.loading = false;
    }
}

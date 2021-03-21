import { Component, OnInit } from '@angular/core';
import { WorkerService } from 'src/app/services/worker.service';
import WorkerDetail from "../../models/worker-detail";

@Component({
    selector: 'app-workers',
    templateUrl: './workers.component.html',
    styles: [`:host {
        width: 100%;
        .card {
            width: 100%;
            &:nth-child(n + 2) {
                margin-top: 20px;
            }
        }
      }`]
})
export class WorkersComponent implements OnInit {

    workerDetails!: WorkerDetail[];

    loading = false;
    active = true;

    constructor(private workerService: WorkerService) {
    }

    ngOnInit(): void {
        this.refreshWorkers();
    }

    toggleActive(): void {
        this.active = !this.active;
    }

    async refreshWorkers(): Promise<void> {
        this.loading = true;
        console.log("in refreshWorkers");
        this.workerDetails = await this.workerService.getAllWorkers();
        this.loading = false;
    }
}

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { WorkerService } from 'src/app/services/worker.service';
import { WorkerDetail } from "../../models/worker-detail";

@Component({
    selector: 'app-worker-table',
    templateUrl: './worker-table.component.html'
})
export class WorkerTableComponent implements OnInit {

    @Input()
    workerDetails!: WorkerDetail[];

    @Input()
    active!: boolean;

    @Output()
    refreshWorkers = new EventEmitter<boolean>();

    constructor(private workerService: WorkerService) { }

    ngOnInit(): void {
    }

    async deleteWorker(workerID: number): Promise<void> {
        if (confirm("Are you sure?")) {
            await this.workerService.deleteWorker(workerID);
            this.refreshWorkers.emit(true);
        }
    }

    async reactivateWorker(workerID: number) : Promise<void> {
        if (confirm("Are you sure?")) {
            await this.workerService.reactivateWorker(workerID);
            this.refreshWorkers.emit(true);
        }
    }

    async runWorker(workerID: number): Promise<void> {
        
    }
}

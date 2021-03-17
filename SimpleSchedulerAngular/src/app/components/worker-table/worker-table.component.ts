import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { WorkerService } from 'src/app/services/worker.service';
import WorkerDetail from "../../../models/worker-detail";

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

    deleteWorker(workerID: number): void {
        if (confirm("Are you sure?")) {
            this.workerService.deleteWorker(workerID).subscribe(() => {
                this.refreshWorkers.emit(true);
            });
        }
    }
}

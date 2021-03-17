import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { WorkerService } from 'src/app/services/worker.service';
import Worker from "../../../models/worker";

@Component({
    // tslint:disable-next-line: component-selector
    selector: '[app-worker-action-button-cells]',
    templateUrl: './worker-action-button-cells.component.html',
    styleUrls: ['./worker-action-button-cells.component.scss']
})
export class WorkerActionButtonCellsComponent implements OnInit {

    @Input()
    worker!: Worker;
    isActive = false;

    @Output()
    refreshWorkers = new EventEmitter<boolean>();

    constructor(private workerService: WorkerService) { }

    ngOnInit(): void {
        this.isActive = this.worker.isActive;
    }

}

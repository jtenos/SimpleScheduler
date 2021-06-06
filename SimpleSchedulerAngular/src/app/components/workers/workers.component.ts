import { Component, OnInit, ViewChild } from '@angular/core';
import { connectableObservableDescriptor } from 'rxjs/internal/observable/ConnectableObservable';
import { WorkerService } from 'src/app/services/worker.service';
import { WorkerDetail } from "../../models/worker-detail";
import { WorkerTableComponent } from '../worker-table/worker-table.component';

@Component({
    selector: 'app-workers',
    templateUrl: './workers.component.html',
    styles: [`:host {
        width: 100%;
        .card {
            width: 100%;
        }
      }`]
})
export class WorkersComponent implements OnInit {

    workerDetails!: WorkerDetail[];

    loading = false;
    active = true;

    @ViewChild(WorkerTableComponent)
    child!: WorkerTableComponent;

    constructor(private workerService: WorkerService) {
    }

    ngOnInit(): void {
        this.refreshWorkers();
    }

    toggleActive(): void {
        this.active = !this.active;

        // TODO: https://stackoverflow.com/questions/45943112/angular-input-data-not-immediately-available-on-child-component-why
        // Use ngOnChanges instead of a timeout
        setTimeout(() => this.child.toggleActive(), 50);
    }

    refresh() {
        this.refreshWorkers();
    }

    doFilter(value: string) {
        this.child.doFilter(value);
    }

    async refreshWorkers(): Promise<void> {
        this.loading = true;
        this.workerDetails = await this.workerService.getAllWorkers();
        this.workerDetails.sort((a, b) => a.worker.workerName > b.worker.workerName ? 1 : -1);
        this.loading = false;
    }
}

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Schedule } from 'src/app/models/schedule';
import TimeSpan from 'src/app/models/timespan';
import { ScheduleService } from 'src/app/services/schedule.service';
import { WorkerService } from 'src/app/services/worker.service';
import { WorkerDetail } from "../../models/worker-detail";
import { SchedulesComponent } from '../schedules/schedules.component';

@Component({
    selector: 'app-worker-table',
    templateUrl: './worker-table.component.html'
})
export class WorkerTableComponent implements OnInit {

    @Input()
    workerDetails!: WorkerDetail[];

    @Input()
    active!: boolean;

    @Input()
    filterWorkerName: string | undefined;

    @Output()
    refreshWorkers = new EventEmitter<boolean>();

    constructor(private workerService: WorkerService, 
        private scheduleService: ScheduleService) { }

    ngOnInit(): void {
    }

    async deleteWorker(workerID: number): Promise<void> {
        if (confirm("Are you sure?")) {
            const resultMessage = await this.workerService.deleteWorker(workerID);
            if (resultMessage) {
                alert(resultMessage);
                return;
            }
            this.refreshWorkers.emit(true);
        }
    }

    async reactivateWorker(workerID: number): Promise<void> {
        if (confirm("Are you sure?")) {
            await this.workerService.reactivateWorker(workerID);
            this.refreshWorkers.emit(true);
        }
    }

    async runWorker(workerID: number): Promise<void> {
        await this.workerService.runNow(workerID);
        this.refreshWorkers.emit(true);
    }

    getFormattedSchedule(schedule: Schedule): string {
        return this.scheduleService.formatSchedule(schedule);
    }
}

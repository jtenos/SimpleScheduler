import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Schedule } from 'src/app/models/schedule';
import TimeSpan from 'src/app/models/timespan';
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
        let days = "";
        if (schedule.sunday && schedule.monday && schedule.tuesday && schedule.wednesday && schedule.thursday && schedule.friday && schedule.saturday) {
            days = "Every day";
        } else if (!schedule.sunday && schedule.monday && schedule.tuesday && schedule.wednesday && schedule.thursday && schedule.friday && !schedule.saturday) {
            days = "Weekdays";
        } else {
            days += schedule.sunday ? "Su " : "__ ";
            days += schedule.monday ? "Mo " : "__ ";
            days += schedule.tuesday ? "Tu " : "__ ";
            days += schedule.wednesday ? "We " : "__ ";
            days += schedule.thursday ? "Th " : "__ ";
            days += schedule.friday ? "Fr " : "__ ";
            days += schedule.saturday ? "Sa " : "__ ";
        }

        let times = "";

        // TimeSpan objects are simple objects, converting back to the classes
        if (schedule.recurTime) {
            schedule.recurTime = new TimeSpan(schedule.recurTime.hours, schedule.recurTime.minutes);
        }
        if (schedule.recurBetweenEndUTC) {
            schedule.recurBetweenEndUTC = new TimeSpan(schedule.recurBetweenEndUTC.hours, schedule.recurBetweenEndUTC.minutes);
        }
        if (schedule.recurBetweenStartUTC) {
            schedule.recurBetweenStartUTC = new TimeSpan(schedule.recurBetweenStartUTC.hours, schedule.recurBetweenStartUTC.minutes);
        }
        if (schedule.timeOfDayUTC) {
            schedule.timeOfDayUTC = new TimeSpan(schedule.timeOfDayUTC.hours, schedule.timeOfDayUTC.minutes);
        }

        if (schedule.timeOfDayUTC) {
            times = `at ${schedule.timeOfDayUTC.asFormattedTimeOfDay()}`;
        } else if (schedule.recurTime) {
            times = schedule.recurTime.asFormattedTimeSpan();
        }

        if (schedule.recurBetweenStartUTC && schedule.recurBetweenEndUTC) {
            times += ` between ${schedule.recurBetweenStartUTC.asFormattedTimeOfDay()} and ${schedule.recurBetweenEndUTC.asFormattedTimeOfDay()}`;
        } else if (schedule.recurBetweenStartUTC) {
            times += ` starting at ${schedule.recurBetweenStartUTC.asFormattedTimeOfDay()}`;
        } else if (schedule.recurBetweenEndUTC) {
            times += ` until ${schedule.recurBetweenEndUTC.asFormattedTimeOfDay()}`;
        }

        return `${days} ${times}`;
    }
}

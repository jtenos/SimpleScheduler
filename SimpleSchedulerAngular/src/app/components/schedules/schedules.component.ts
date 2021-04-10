import { Component, OnInit } from '@angular/core';
import { ScheduleService } from 'src/app/services/schedule.service';
import { ScheduleDetail } from "../../models/schedule-detail";

@Component({
    selector: 'app-schedules',
    templateUrl: './schedules.component.html',
    styleUrls: ['./schedules.component.scss']
})
export class SchedulesComponent implements OnInit {

    scheduleDetails!: ScheduleDetail[];

    loading = false;
    active = true;

    constructor(private scheduleService: ScheduleService) {
    }

    ngOnInit(): void {
        this.refreshSchedules();
    }

    toggleActive(): void {
        this.active = !this.active;
    }

    refresh() {
        this.refreshSchedules();
    }

    async refreshSchedules(): Promise<void> {
        this.loading = true;
        this.scheduleDetails = await this.scheduleService.getAllSchedules();
        this.scheduleDetails.sort((a, b) => a.worker.workerName > b.worker.workerName ? 1 : -1);
        this.loading = false;
    }
}

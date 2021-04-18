import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Schedule } from 'src/app/models/schedule';
import TimeSpan from 'src/app/models/timespan';
import { ScheduleService } from 'src/app/services/schedule.service';
import { ScheduleDetail } from "../../models/schedule-detail";

@Component({
    selector: 'app-schedule-table',
    templateUrl: './schedule-table.component.html'
})
export class ScheduleTableComponent implements OnInit {

    @Input()
    scheduleDetails!: ScheduleDetail[];

    @Input()
    active!: boolean;

    @Output()
    refreshSchedules = new EventEmitter<boolean>();

    constructor(private scheduleService: ScheduleService) { }

    ngOnInit(): void {
    }

    async deleteSchedule(scheduleID: number): Promise<void> {
        if (confirm("Are you sure?")) {
            await this.scheduleService.deleteSchedule(scheduleID);
            this.refreshSchedules.emit(true);
        }
    }

    async reactivateSchedule(scheduleID: number): Promise<void> {
        if (confirm("Are you sure?")) {
            await this.scheduleService.reactivateSchedule(scheduleID);
            this.refreshSchedules.emit(true);
        }
    }

    getFormattedSchedule(schedule: Schedule) : string {
        return this.scheduleService.formatSchedule(schedule);
    }

    getFormattedDays(schedule: Schedule): string {
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
        return days;
    }

    getFormattedTime(schedule: Schedule): string {
        let times = "";

        // TimeSpan objects are simple objects, converting back to the classes
        if (schedule.recurTime) {
            schedule.recurTime = TimeSpan.fromObject(schedule.recurTime);
        }
        if (schedule.recurBetweenEndUTC) {
            schedule.recurBetweenEndUTC = TimeSpan.fromObject(schedule.recurBetweenEndUTC);
        }
        if (schedule.recurBetweenStartUTC) {
            schedule.recurBetweenStartUTC = TimeSpan.fromObject(schedule.recurBetweenStartUTC);
        }
        if (schedule.timeOfDayUTC) {
            schedule.timeOfDayUTC = TimeSpan.fromObject(schedule.timeOfDayUTC);
        }

        if (schedule.timeOfDayUTC) {
            times = `at ${schedule.timeOfDayUTC.asFormattedTimeOfDay()} (UTC)`;
        } else if (schedule.recurTime) {
            times = schedule.recurTime.asFormattedTimeSpan();
        }

        if (schedule.recurBetweenStartUTC && schedule.recurBetweenEndUTC) {
            times += ` between ${schedule.recurBetweenStartUTC.asFormattedTimeOfDay()} and ${schedule.recurBetweenEndUTC.asFormattedTimeOfDay()} (UTC)`;
        } else if (schedule.recurBetweenStartUTC) {
            times += ` starting at ${schedule.recurBetweenStartUTC.asFormattedTimeOfDay()} (UTC)`;
        } else if (schedule.recurBetweenEndUTC) {
            times += ` until ${schedule.recurBetweenEndUTC.asFormattedTimeOfDay()} (UTC)`;
        }

        return times;
    }
}

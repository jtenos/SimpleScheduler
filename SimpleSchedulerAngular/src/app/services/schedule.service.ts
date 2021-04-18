import { Injectable } from "@angular/core";
import { Schedule } from "../models/schedule";
import { ScheduleDetail } from "../models/schedule-detail";
import { ApiService } from "./api.service";
import Kvp from "../models/kvp";
import { Router } from "@angular/router";
import TimeSpan from "../models/timespan";

@Injectable({
    providedIn: "root"
})
export class ScheduleService {

    constructor(private apiService: ApiService, private router: Router) { }

    async getAllSchedules(): Promise<ScheduleDetail[]> {
        return await this.apiService.get<ScheduleDetail[]>("Schedules", "GetAllSchedules", []);
    }

    async getSchedule(scheduleID: number): Promise<ScheduleDetail> {
        return await this.apiService.get<ScheduleDetail>("Schedules", "GetSchedule", [new Kvp("scheduleID", scheduleID.toString())]);
    }

    async deleteSchedule(scheduleID: number): Promise<boolean> {
        await this.apiService.post("Schedules", "DeleteSchedule", { scheduleID });
        return true;
    }

    async reactivateSchedule(scheduleID: number): Promise<boolean> {
        await this.apiService.post("Schedules", "ReactivateSchedule", { scheduleID });
        return true;
    }

    async saveSchedule(schedule: Schedule): Promise<void> {
        const result = await this.apiService.post("Schedules", "SaveSchedule", schedule);
        if (result.success) {
            this.router.navigateByUrl("schedules");
        } else {
            alert(result.message);
        }
    }

    formatSchedule(schedule: Schedule) {
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

        return `${days} [${times}]`;
    }
}

import { Injectable } from "@angular/core";
import { Schedule } from "../models/schedule";
import { ScheduleDetail } from "../models/schedule-detail";
import { ApiService } from "./api.service";
import Kvp from "../models/kvp";
import { Router } from "@angular/router";
@Injectable({
    providedIn: "root"
})
export class ScheduleService {

    constructor(private apiService: ApiService, private router: Router) { }

    async getAllSchedules(): Promise<ScheduleDetail[]> {
        return await this.apiService.get<ScheduleDetail[]>("Schedules", "GetAllSchedules", []);
    }

    async getSchedule(scheduleID: number): Promise<Schedule> {
        return await this.apiService.get<Schedule>("Schedules", "GetSchedule", [new Kvp("scheduleID", scheduleID.toString())]);
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
}

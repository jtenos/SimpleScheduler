import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { ActivatedRoute } from "@angular/router";
import { Schedule } from "../../models/schedule";
import { ScheduleService } from 'src/app/services/schedule.service';
import { Worker } from "../../models/worker";
import { WorkerService } from 'src/app/services/worker.service';
import TimeSpan from 'src/app/models/timespan';

@Component({
    selector: 'app-edit-schedule',
    templateUrl: './edit-schedule.component.html'
})
export class EditScheduleComponent implements OnInit {

    loading = false;

    scheduleID!: number;
    schedule!: Schedule;
    allWorkers: Worker[] = [];

    scheduleForm = this.formBuilder.group({
        scheduleID: [""],
        isActive: [""],
        workerID: [""],
        sunday: [""],
        monday: [""],
        tuesday: [""],
        wednesday: [""],
        thursday: [""],
        friday: [""],
        saturday: [""],
        timeType: [""],
        timeOfDayUTC: [""],
        recurTime: [""],
        recurBetweenStartUTC: [""],
        recurBetweenEndUTC: [""],
        oneTime: [""]
    });

    constructor(private route: ActivatedRoute,
        private scheduleService: ScheduleService,
        private workerService: WorkerService,
        private formBuilder: FormBuilder) { }

    ngOnInit(): void {
        this.route.params.subscribe(async params => {
            this.loading = true;
            this.scheduleID = +params.scheduleID;

            const setScheduleForm = () => {
                this.scheduleForm.setValue({
                    scheduleID: this.schedule.scheduleID,
                    isActive: this.schedule.isActive,
                    workerID: this.schedule.workerID,
                    sunday: this.schedule.sunday,
                    monday: this.schedule.monday,
                    tuesday: this.schedule.tuesday,
                    wednesday: this.schedule.wednesday,
                    thursday: this.schedule.thursday,
                    friday: this.schedule.friday,
                    saturday: this.schedule.saturday,
                    timeType: this.schedule.timeOfDayUTC ? "time-of-day" : this.schedule.recurTime ? "recur" : "",
                    timeOfDayUTC: this.schedule.timeOfDayUTC,
                    recurTime: this.schedule.recurTime,
                    recurBetweenStartUTC: this.schedule.recurBetweenStartUTC,
                    recurBetweenEndUTC: this.schedule.recurBetweenEndUTC,
                    oneTime: this.schedule.oneTime
                });
                this.loading = false;
            }

            this.allWorkers = await this.workerService.getAllActiveWorkers();
            if (!this.scheduleID) {
                this.schedule = this.getEmptySchedule();
                setScheduleForm();
                this.loading = false;
            } else {
                this.schedule = (await this.scheduleService.getSchedule(this.scheduleID)).schedule;
                setScheduleForm();
            }
        });
    }

    async scheduleFormSubmit() {
        this.schedule = {
            scheduleID: +this.scheduleForm.value.scheduleID,
            isActive: !!this.scheduleForm.value.isActive,
            workerID: +this.scheduleForm.value.workerID,
            sunday: !!this.scheduleForm.value.sunday,
            monday: !!this.scheduleForm.value.monday,
            tuesday: !!this.scheduleForm.value.tuesday,
            wednesday: !!this.scheduleForm.value.wednesday,
            thursday: !!this.scheduleForm.value.thursday,
            friday: !!this.scheduleForm.value.friday,
            saturday: !!this.scheduleForm.value.saturday,
            timeOfDayUTC: TimeSpan.parse(this.scheduleForm.value.timeOfDayUTC),
            recurTime: TimeSpan.parse(this.scheduleForm.value.recurTime),
            recurBetweenStartUTC: TimeSpan.parse(this.scheduleForm.value.recurBetweenStartUTC),
            recurBetweenEndUTC: TimeSpan.parse(this.scheduleForm.value.recurBetweenEndUTC),
            oneTime: this.scheduleForm.value.oneTime
        };
        await this.scheduleService.saveSchedule(this.schedule);
    }

    private getEmptySchedule(): Schedule {
        return {
            scheduleID: 0,
            isActive: true,
            workerID: 0,
            sunday: true,
            monday: true,
            tuesday: true,
            wednesday: true,
            thursday: true,
            friday: true,
            saturday: true,
            timeOfDayUTC: null,
            recurTime: null,
            recurBetweenStartUTC: null,
            recurBetweenEndUTC: null,
            oneTime: false
        };
    }
}

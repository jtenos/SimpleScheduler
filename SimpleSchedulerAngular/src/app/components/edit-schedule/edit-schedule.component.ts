import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { ActivatedRoute } from "@angular/router";
import { Schedule } from "../../models/schedule";
import { ScheduleService } from 'src/app/services/schedule.service';
import { Worker } from "../../models/worker";
import { WorkerService } from 'src/app/services/worker.service';
import TimeSpan from 'src/app/models/timespan';
import { ThrowStmt } from '@angular/compiler';

@Component({
    selector: 'app-edit-schedule',
    templateUrl: './edit-schedule.component.html'
})
export class EditScheduleComponent implements OnInit {

    loading = false;

    scheduleID!: number;
    schedule!: Schedule;
    allWorkers: Worker[] = [];
    hourOptions: {text: string, value: number}[] = [];
    minuteOptions: {text: string, value: number}[] = [];

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
        recurTimeMinutes: [""],
        recurTimeHours: [""], 
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
                    recurTimeHours: this.schedule.recurTime?.hours ? this.schedule.recurTime.hours : 0,
                    recurTimeMinutes: this.schedule.recurTime?.minutes ? this.schedule.recurTime.minutes : 0,
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

            this.hourOptions.length = 0;
            for (let i = 0; i <= 23; ++i) {
                this.hourOptions.push({text: `${i} ${i === 1 ? "hour" : "hours"}`, value: i})
            }
            this.minuteOptions.length = 0;
            for (let i = 0; i <= 59; ++i) {
                this.minuteOptions.push({text: `${i} ${i === 1 ? "minute" : "minutes"}`, value: i});
            }
        });
    }

    async scheduleFormSubmit() {

        function getTimeSpan(input: string | null) {
            if (!input) { return null; }
            let result: TimeSpan;
            if (!input.match(/^[0-9]{2}\:[0-9]{2}$/)) {
                throw "Invalid time";
            }
            return TimeSpan.parse(`${input.substring(0, 2)}${input.substring(3, 5)}`);
        }

        let timeOfDayUTC: TimeSpan | null = null;
        if (this.scheduleForm.value.timeType === "time-of-day") {
            try {
                timeOfDayUTC = getTimeSpan(this.scheduleForm.value.timeOfDayUTC);
            } catch (ex) {
                return alert(ex);
            }
        }

        let recurTime: TimeSpan | null = null;
        let recurBetweenStartUTC: TimeSpan | null = null;
        let recurBetweenEndUTC: TimeSpan | null = null;
        if (this.scheduleForm.value.timeType === "recur") {
            try {
                recurTime = new TimeSpan(this.scheduleForm.value.recurTimeHours, this.scheduleForm.value.recurTimeMinutes);
                if (this.scheduleForm.value.recurBetweenStartUTC) {
                    recurBetweenStartUTC = getTimeSpan(this.scheduleForm.value.recurBetweenStartUTC);
                }
                if (this.scheduleForm.value.recurBetweenEndUTC) {
                    recurBetweenEndUTC = getTimeSpan(this.scheduleForm.value.recurBetweenEndUTC);
                }
            } catch (ex) {
                return alert(ex);
            }
        }

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
            timeOfDayUTC: timeOfDayUTC,
            recurTime: recurTime,
            recurBetweenStartUTC: recurBetweenStartUTC,
            recurBetweenEndUTC: recurBetweenEndUTC,
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

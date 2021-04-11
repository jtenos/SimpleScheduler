import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Job } from 'src/app/models/job';
import TimeSpan from 'src/app/models/timespan';
import { JobService } from 'src/app/services/job.service';
import { JobDetail } from "../../models/job-detail";
const moment = require("moment");

@Component({
    selector: 'app-job-table',
    templateUrl: './job-table.component.html'
})
export class JobTableComponent implements OnInit {

    @Input()
    jobDetails!: JobDetail[];

    @Output()
    refreshJobs = new EventEmitter<boolean>();

    constructor(private jobService: JobService) { }

    ngOnInit(): void {
    }

    formatDateTime(date: string) {
        if (!date) { return ""; }
        const theDate = Date.parse(date);
        const mom = moment(theDate);
        return `${mom.format("MMM DD YYYY")}\n${mom.format("HH:mm:ss")}`
    }
}

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Job } from 'src/app/models/job';
import TimeSpan from 'src/app/models/timespan';
import { JobService } from 'src/app/services/job.service';
import { JobDetail } from "../../models/job-detail";
import * as moment from "moment";
import * as he from "he";
import * as numeral from "numeral";

@Component({
    selector: 'app-job-table',
    templateUrl: './job-table.component.html'
})
export class JobTableComponent implements OnInit {

    @Input()
    jobDetails!: JobDetail[];

    @Input()
    filterWorkerName: string | undefined;

    @Output()
    refreshJobs = new EventEmitter<boolean>();

    constructor(private jobService: JobService) { }

    ngOnInit(): void {
    }

    formatDateTime(date: string | null) {
        if (!date) { return ""; }
        const theDate = Date.parse(date);
        const mom = moment(theDate);
        return `${mom.format("MMM DD YYYY")}\n${mom.format("HH:mm:ss")}`
    }

    formatDuration(duration: number | null) {
        if (!duration) {
            return "";
        }
        return numeral(duration).format("0,0.000");
    }

    async alertDetail(e: MouseEvent, jobID: number) {
        e.preventDefault();
        e.stopPropagation();
        const detailedMessage = await this.jobService.getDetailedMessage(jobID);
        alert(detailedMessage);
    }

    // TODO: Does this get blocked by popup blocker?
    async popUpDetail(e: MouseEvent, jobID: number) {
        e.preventDefault();
        e.stopPropagation();
        const detailedMessage = await this.jobService.getDetailedMessage(jobID);
        const newWindow = window.open("about:blank", "_blank", "width=400,height=400,resizable");
        let val: string = he.encode(detailedMessage, { strict: true });
        val = JSON.stringify(val);
        newWindow?.document.write(`<div style="white-space:pre-line;"></div>`);
        newWindow?.document.write(`<script>document.getElementsByTagName("div")[0].innerText = ${val}</script>`);
    }

    async cancelJob(jobID: number): Promise<void> {
        const message: string = await this.jobService.cancelJob(jobID);
        if (message) {
            return alert(message);
        }
        this.refreshJobs.emit(true);
    }

    async acknowledgeError(jobID: number): Promise<void> {
        const message: string = await this.jobService.acknowledgeError(jobID);
        if (message) {
            return alert(message);
        }
        this.refreshJobs.emit(true);
    }
}

import {
  AfterViewInit,
  Component,
  EventEmitter,
  Input,
  OnInit,
  Output,
  ViewChild,
} from "@angular/core";
import { JobService } from "src/app/services/job.service";
import { JobDetail } from "../../models/job-detail";
import * as moment from "moment";
import * as he from "he";
import { MatTableDataSource } from "@angular/material/table";
import { MatSort } from "@angular/material/sort";

@Component({
  selector: "app-job-table",
  templateUrl: "./job-table.component.html",
  styles: [`
        table { width: 100%; }
    `],
})
export class JobTableComponent implements OnInit, AfterViewInit {
  @Input()
  jobDetails!: JobDetail[];

  @Input()
  filterWorkerName: string | undefined;

  @Output()
  refreshJobs = new EventEmitter<boolean>();

  dataSource = new MatTableDataSource<JobDetail>();
  displayedColumns: string[] = [
    "cancelJob",
    "workerName",
    "insertDateUTC",
    "queueDateUTC",
    "completeDateUTC",
    "statusCode",
    "message",
  ];

  @ViewChild(MatSort)
  sort!: MatSort;

  constructor(private jobService: JobService) {
  }

  ngOnInit(): void {
    this.dataSource.data = this.jobDetails;
    this.dataSource.filterPredicate = (data: JobDetail, filter: string) =>
      data.worker.workerName.toLocaleLowerCase().includes(
        filter.toLocaleLowerCase(),
      );
  }

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
  }

  doFilter(value: string) {
    this.dataSource.filter = value;
  }

  formatDate(date: string) {
    if (!date) {
      return "";
    }
    const theDate = Date.parse(date);
    const mom = moment(theDate);
    return `${mom.format("MMM DD YYYY")}`;
  }

  formatTime(date: string) {
    if (!date) {
      return "";
    }
    const theDate = Date.parse(date);
    const mom = moment(theDate);
    return `${mom.format("HH:mm:ss")} (UTC)`;
  }

  formatDateTime(date: string) {
    if (!date) {
      return "";
    }
    return `${this.formatDate(date)}\n${this.formatTime(date)}`;
  }

  async showDetailAlert(jobID: number) {
    const detailedMessage = await this.jobService.getDetailedMessage(jobID);
    // TODO: nicer alert
    alert(detailedMessage);
  }

  async showDetailPopup(jobID: number) {
    const detailedMessage = await this.jobService.getDetailedMessage(jobID);
    const newWindow = window.open(
      "about:blank",
      "_blank",
      "width=400,height=400,resizable",
    );
    let val: string = he.encode(detailedMessage, { strict: true });
    val = JSON.stringify(val);
    newWindow?.document.write(`<div style="white-space:pre-line;"></div>`);
    newWindow?.document.write(
      `<script>document.getElementsByTagName("div")[0].innerText = ${val}</script>`,
    );
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

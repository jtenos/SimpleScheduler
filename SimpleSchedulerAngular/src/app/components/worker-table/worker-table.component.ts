import {
  AfterViewInit,
  Component,
  EventEmitter,
  Input,
  OnInit,
  Output,
  ViewChild,
} from "@angular/core";
import { Schedule } from "src/app/models/schedule";
import { ScheduleService } from "src/app/services/schedule.service";
import { WorkerService } from "src/app/services/worker.service";
import { WorkerDetail } from "../../models/worker-detail";
import { MatTableDataSource } from "@angular/material/table";
import { MatSort } from "@angular/material/sort";

@Component({
  selector: "app-worker-table",
  templateUrl: "./worker-table.component.html",
  styles: [`
        table { width: 100%; }
  `],
})
export class WorkerTableComponent implements OnInit, AfterViewInit {
  @Input()
  workerDetails!: WorkerDetail[];

  @Input()
  active!: boolean;

  @Input()
  filterWorkerName: string | undefined;

  @Output()
  refreshWorkers = new EventEmitter<boolean>();

  dataSource = new MatTableDataSource<WorkerDetail>();
  displayedColumns: string[] = [
    "actions",
    "name",
    "executable",
    "schedules",
  ];

  @ViewChild(MatSort)
  sort!: MatSort;

  constructor(
    private workerService: WorkerService,
    private scheduleService: ScheduleService,
  ) {}

  ngOnInit(): void {
    this.dataSource.data = this.workerDetails;
    const component = this;
    this.dataSource.filterPredicate = function (data: WorkerDetail, filter: string) {
      const filterValue = JSON.parse(filter).value;
      const result = data.worker.workerName.toLocaleLowerCase().includes(
        filterValue.toLocaleLowerCase(),
      ) && data.worker.isActive === component.active;
      return result;
    };
  }

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
  }

  doFilter(value: string) {
    this.dataSource.filter = JSON.stringify({ value, active: this.active });
  }

  toggleActive() {
    const filterValue = JSON.parse(this.dataSource.filter || `{}`).value || "";
    this.doFilter(filterValue);
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
    return this.scheduleService.formatSchedule(schedule);
  }
}

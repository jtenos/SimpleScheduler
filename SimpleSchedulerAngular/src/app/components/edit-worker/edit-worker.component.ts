import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { ActivatedRoute } from "@angular/router";
import Worker from "../../../models/worker";
import Schedule from 'src/app/models/schedule';
import { WorkerService } from 'src/app/services/worker.service';
import WorkerDetail from "../../../models/worker-detail";

@Component({
    selector: 'app-edit-worker',
    templateUrl: './edit-worker.component.html'
})
export class EditWorkerComponent implements OnInit {

    loading = false;

    workerID!: number;
    workerDetail!: WorkerDetail;

    workerForm = this.formBuilder.group({
        workerID: [""],
        isActive: [""],
        description: [""],
        freeText: [""],
        emailOnSuccess: [""],
        directoryName: [""],
        executable: [""],
        argumentValues: [""],
        parentWorkerID: [""],
        timeoutMinutes: [""],
        overdueMinutes: [""]
    });

    constructor(private route: ActivatedRoute, private workerService: WorkerService, private formBuilder: FormBuilder) { }

    ngOnInit(): void {
        this.route.params.subscribe(params => {
            this.loading = true;
            this.workerID = +params.workerID;

            const setWorkerForm = () => {
                this.workerForm.setValue({
                    workerID: this.workerDetail.worker.workerID,
                    isActive: this.workerDetail.worker.isActive,
                    description: this.workerDetail.worker.description,
                    freeText: this.workerDetail.worker.freeText,
                    emailOnSuccess: this.workerDetail.worker.emailOnSuccess,
                    directoryName: this.workerDetail.worker.directoryName,
                    executable: this.workerDetail.worker.executable,
                    argumentValues: this.workerDetail.worker.argumentValues,
                    parentWorkerID: this.workerDetail.worker.parentWorkerID || "",
                    timeoutMinutes: this.workerDetail.worker.timeoutMinutes,
                    overdueMinutes: this.workerDetail.worker.overdueMinutes
                });
                this.loading = false;
            }

            if (!this.workerID) {
                const newWorker = new Worker(0, true, "", 20, 20, "", "", "", "", "", undefined);
                const newSchedule = new Schedule(0, true, 0, true, true, true, true, true, true, true, false);
                this.workerDetail = new WorkerDetail(newWorker, [newSchedule]);
                setWorkerForm();
                this.loading = false;
            } else {
                this.workerService.getWorker(this.workerID).subscribe(workerDetail => {
                    this.workerDetail = workerDetail;
                    setWorkerForm();
                });
            }
        });
    }
}

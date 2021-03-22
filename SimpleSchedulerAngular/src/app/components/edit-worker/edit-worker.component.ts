import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { ActivatedRoute } from "@angular/router";
import Worker from "../../models/worker";
import { WorkerService } from 'src/app/services/worker.service';
import WorkerDetail from 'src/app/models/worker-detail';

@Component({
    selector: 'app-edit-worker',
    templateUrl: './edit-worker.component.html'
})
export class EditWorkerComponent implements OnInit {

    loading = false;

    workerID!: number;
    worker!: Worker;
    allWorkers: WorkerDetail[] = [];

    workerForm = this.formBuilder.group({
        workerID: [""],
        isActive: [""],
        workerName: [""],
        detailedDescription: [""],
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
        this.route.params.subscribe(async params => {
            this.loading = true;
            this.workerID = +params.workerID;

            const setWorkerForm = () => {
                this.workerForm.setValue({
                    workerID: this.worker.workerID,
                    isActive: this.worker.isActive,
                    workerName: this.worker.workerName,
                    detailedDescription: this.worker.detailedDescription,
                    emailOnSuccess: this.worker.emailOnSuccess,
                    directoryName: this.worker.directoryName,
                    executable: this.worker.executable,
                    argumentValues: this.worker.argumentValues,
                    parentWorkerID: this.worker.parentWorkerID || null,
                    timeoutMinutes: this.worker.timeoutMinutes,
                    overdueMinutes: this.worker.overdueMinutes
                });
                this.loading = false;
            }

            if (!this.workerID) {
                this.worker = new Worker(0, true, "", 20, 20, "", "", "", "", "", undefined);
                setWorkerForm();
                this.loading = false;
            } else {
                this.worker = await this.workerService.getWorker(this.workerID);
                this.allWorkers = await this.workerService.getAllWorkers();
                this.allWorkers.sort((a, b) => a.worker.workerName > b.worker.workerName ? 1 : -1);
                setWorkerForm();
            }
        });
    }

    async workerFormSubmit() {
        this.worker = new Worker(+this.workerForm.value.workerID,
            this.workerForm.value.isActive, this.workerForm.value.workerName, 
            +this.workerForm.value.timeoutMinutes, +this.workerForm.value.overdueMinutes,
            this.workerForm.value.detailedDescription, this.workerForm.value.emailOnSuccess,
            this.workerForm.value.directoryName, this.workerForm.value.executable,
            this.workerForm.value.argumentValues, +this.workerForm.value.parentWorkerID || null);
         this.workerForm.value;
        await this.workerService.saveWorker(this.worker);
    }
}

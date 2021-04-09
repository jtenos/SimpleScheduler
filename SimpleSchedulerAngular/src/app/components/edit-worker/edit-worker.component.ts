import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { ActivatedRoute } from "@angular/router";
import { Worker } from "../../models/worker";
import { WorkerService } from 'src/app/services/worker.service';
import { WorkerDetail } from 'src/app/models/worker-detail';

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
        timeoutMinutes: [""]
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
                    timeoutMinutes: this.worker.timeoutMinutes
                });
                this.loading = false;
            }

            this.allWorkers = await this.workerService.getAllWorkers();
            this.allWorkers.sort((a, b) => a.worker.workerName > b.worker.workerName ? 1 : -1);
            if (!this.workerID) {
                this.worker = this.getEmptyWorker();
                setWorkerForm();
                this.loading = false;
            } else {
                this.worker = await this.workerService.getWorker(this.workerID);
                setWorkerForm();
            }
        });
    }

    async workerFormSubmit() {
        this.worker = {
            workerID: +this.workerForm.value.workerID,
            isActive: this.workerForm.value.isActive,
            workerName: this.workerForm.value.workerName,
            detailedDescription: this.workerForm.value.detailedDescription,
            emailOnSuccess: this.workerForm.value.emailOnSuccess,
            parentWorkerID: +this.workerForm.value.parentWorkerID || null,
            timeoutMinutes: +this.workerForm.value.timeoutMinutes,
            directoryName: this.workerForm.value.directoryName,
            executable: this.workerForm.value.executable,
            argumentValues: this.workerForm.value.argumentValues
        };
        await this.workerService.saveWorker(this.worker);
    }

    private getEmptyWorker(): Worker {
        return {
            workerID: 0,
            isActive: true,
            workerName: "",
            detailedDescription: "",
            emailOnSuccess: "",
            parentWorkerID: null,
            timeoutMinutes: 20,
            directoryName: "",
            executable: "",
            argumentValues: ""
        };
    }
}

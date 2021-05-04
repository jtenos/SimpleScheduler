import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, Validators } from '@angular/forms';
import { SchedulerErrorStateMatcher } from 'src/app/scheduler-error-state-matcher';
import { LoginService } from 'src/app/services/login.service';

@Component({
    selector: 'app-login',
    templateUrl: './login.component.html',
    styles: [`
        mat-card {
            width: 400px;
        }
    `]
})
export class LoginComponent implements OnInit {

    loading = true;
    submitting = false;
    submitted = false;
    message = "";

    emailFormControl = new FormControl("", [Validators.required]);

    matcher = new SchedulerErrorStateMatcher();

    constructor(private loginService: LoginService, private formBuilder: FormBuilder) { }

    ngOnInit(): void {
        this.loading = false;
    }

    async onSubmit() {
        this.submitting = true;
        const { success, message } = await this.loginService.submitEmail(this.emailFormControl.value as string);
        this.submitting = false;
        if (success) {
            this.submitted = true;
        } else {
            this.message = message;
        }
    }
}

import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { LoginService } from 'src/app/services/login.service';

@Component({
    selector: 'app-login',
    templateUrl: './login.component.html'
})
export class LoginComponent implements OnInit {

    loading = false;
    submitting = false;
    submitted = false;
    message = "";

    loginForm = this.formBuilder.group({
        emailAddress: [""]
    });

    constructor(private loginService: LoginService, private formBuilder: FormBuilder) { }

    ngOnInit(): void {
        this.loading = false;
    }

    async onSubmit(formData: any) {
        this.submitting = true;
        const { success, message } = await this.loginService.submitEmail(formData["emailAddress"] as string);
        this.submitting = false;
        if (success) {
            this.submitted = true;
        } else {
            this.message = message;
        }
    }
}

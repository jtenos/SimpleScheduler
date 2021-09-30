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
    
    allowUserDropdown = false;

    loginForm = this.formBuilder.group({
        emailAddress: [""],
        emailAddressDropDown: [""]
    });

    userEmails: string[] = []

    constructor(private loginService: LoginService, private formBuilder: FormBuilder) { }

    ngOnInit(): void {
        this.loading = true;

        (async() => {
            console.log("Calling user email service");
            this.userEmails = await this.loginService.getAllUserEmails();
            this.allowUserDropdown = !!this.userEmails.length;
            this.loading = false;
        })();
    }

    async onSubmit(formData: any) {
        this.submitting = true;
        const emailAddress = formData["emailAddress"] as string || formData["emailAddressDropDown"] as string;
        const { success, message } = await this.loginService.submitEmail(emailAddress);
        this.submitting = false;
        if (success) {
            this.submitted = true;
        } else {
            this.message = message;
        }
    }
}

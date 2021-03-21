import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { LoginService } from 'src/app/services/login.service';

@Component({
    selector: 'app-login',
    templateUrl: './login.component.html'
})
export class LoginComponent implements OnInit {

    loading = false;
    loginForm = this.formBuilder.group({
        emailAddress: [""]
    });

    constructor(private route: ActivatedRoute, private loginService: LoginService, private formBuilder: FormBuilder) { }

    ngOnInit(): void {
        this.loading = false;
    }

    submitEmail() {

    }
}

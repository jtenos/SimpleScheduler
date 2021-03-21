import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from "@angular/router";
import Worker from "../../models/worker";
import { LoginService } from 'src/app/services/login.service';

@Component({
    selector: 'app-validate-user',
    templateUrl: './validate-user.component.html'
})
export class ValidateUserComponent implements OnInit {

    loading = false;

    constructor(private route: ActivatedRoute, private loginService: LoginService) { }

    ngOnInit(): void {
        console.log("ngOnInit");
        this.route.params.subscribe(async params => {
            const userResult = await this.loginService.validateUser(params.validationCode);
            console.log(userResult);
        });
    }
}

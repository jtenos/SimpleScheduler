import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from "@angular/router";
import { LoginService } from 'src/app/services/login.service';

@Component({
    selector: 'app-validate-user',
    templateUrl: './validate-user.component.html'
})
export class ValidateUserComponent implements OnInit {

    loading = false;
    message? = "";

    constructor(private route: ActivatedRoute, private router: Router, private loginService: LoginService) { }

    ngOnInit(): void {
        console.log("ngOnInit");
        this.route.params.subscribe(async params => {
            const userResult = await this.loginService.validateUser(params.validationCode);
            if (userResult.success) {
                this.router.navigateByUrl("jobs");
                this.loading = false;
            } else {
                this.message = userResult.message;
                this.loading = false;
            }
        });
    }
}

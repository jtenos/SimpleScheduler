import { Injectable } from '@angular/core';
import { ApiService } from "./api.service";

@Injectable({
    providedIn: "root"
})
export class LoginService {
    constructor(private apiService: ApiService) {
    }

    async submitEmail(emailAddress: string) {
        const submitResult = await this.apiService.post("Login", "SubmitEmail", { emailAddress });
        console.log(submitResult);
    }
}

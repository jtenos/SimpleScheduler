import { Injectable } from '@angular/core';
import { PostResult } from '../models/post-result';
import { ApiService } from "./api.service";

@Injectable({
    providedIn: "root"
})
export class LoginService {
    
    constructor(private apiService: ApiService) {
    }

    async submitEmail(emailAddress: string) {
        try {
            const submitResult = await this.apiService.post("Login", "SubmitEmail", { emailAddress });
            alert(submitResult.message);
        }
        catch (ex) {
            if (ex?.error?.message) {
                return alert(ex.error.message);
            }
            return alert("Unknown error. Please try again");
        }
    }

    async validateUser(validationCode: string): Promise<PostResult> {
        try {
            return await this.apiService.post("Login", "ValidateEmail", { validationCode });
        } catch (ex) {
            console.log("ERROR");
            return { success: false, message: ex?.message };
        }
    }
}

import { Injectable } from '@angular/core';
import { PostResult } from '../models/post-result';
import { ApiService } from "./api.service";

@Injectable({
    providedIn: "root"
})
export class LoginService {

    constructor(private apiService: ApiService) {
    }

    async submitEmail(emailAddress: string): Promise<{success: boolean, message: string}> {
        const submitResult = await this.apiService.post("Login", "SubmitEmail", { emailAddress });
        return {
            success: submitResult.success,
            message: submitResult.message || "Unknown error. Please try again"
        };
    }

    async getAllUserEmails(): Promise<string[]> {
        try {
            return await this.apiService.get<string[]>("Login", "GetAllUserEmails", []);
        } catch (ex) {
            console.error(ex || "Unknown error");
            return [];
        }
    }

    async validateUser(validationCode: string): Promise<PostResult> {
        try {
            const userValidationResult = await this.apiService.post("Login", "ValidateEmail", { validationCode });
            if ((userValidationResult as any).emailAddress) {
                localStorage.emailAddress = (userValidationResult as any).emailAddress;
            }
            return userValidationResult;
        } catch (ex) {
            console.log("ERROR");
            return { success: false, message: ex?.message };
        }
    }
}

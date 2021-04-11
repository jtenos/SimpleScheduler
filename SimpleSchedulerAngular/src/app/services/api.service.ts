import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { mergeMap } from "rxjs/operators";
import Config from '../models/config';
import Kvp from '../models/kvp';
import { ConfigService } from './config.service';
import { PostResult } from "../models/post-result";
import { Router } from '@angular/router';

@Injectable({
    providedIn: 'root'
})
export class ApiService {

    constructor(private configService: ConfigService, private http: HttpClient, private router: Router) {
    }

    async get<T>(controllerName: string, actionName: string, parameters: Kvp<string, string>[]): Promise<T> {
        console.log("in get");
        let params = new HttpParams();
        if (parameters) {
            for (const kvp of parameters) {
                params = params.append(kvp.key, kvp.value || "");
            }
        }

        console.log("calling getConfig");
        const config = await this.configService.getConfig();
        console.log(`config=${config}`);
        let url = `${config.apiUrl}/${controllerName}`;
        if (actionName) {
            url += `/${actionName}`;
        }
        console.log(`url=${url}`);
        console.log(`params=${params}`);
        try {
            return await this.http.get<T>(url, { params }).toPromise();
        } catch (ex) {
            if (ex?.status === 401) {
                this.router.navigateByUrl("login");
            } else {
                alert(ex);
                alert(JSON.stringify(ex));
            }
            return new Promise<T>((resolve, reject) => {});
        }
    }

    async post(controllerName: string, actionName: string, body: any): Promise<PostResult> {
        const config = await this.configService.getConfig();
        let url = `${config.apiUrl}/${controllerName}`;
        if (actionName) {
            url += `/${actionName}`;
        }
        try {
            return await this.http.post<PostResult>(url, body).toPromise();
        } catch (ex) {
            if (ex?.status === 401) {
                this.router.navigateByUrl("login");
            }
            return { success: false, message: ex.error?.message || ex.error as string || ex.message || "Unknown error. Please try again." };
        }
    }
}

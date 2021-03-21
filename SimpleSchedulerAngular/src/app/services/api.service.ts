import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { mergeMap } from "rxjs/operators";
import Config from '../models/config';
import Kvp from '../models/kvp';
import { ConfigService } from './config.service';
import { PostResult } from "../models/post-result";

@Injectable({
    providedIn: 'root'
})
export class ApiService {

    constructor(private configService: ConfigService, private http: HttpClient) {
    }

    get<T>(controllerName: string, actionName: string, parameters: Kvp<string, string>[]): Observable<T> {
        let params = new HttpParams();
        if (parameters) {
            for (const kvp of parameters) {
                params = params.append(kvp.key, kvp.value || "");
            }
        }

        return new Observable<T>(subscriber => {
            this.configService.getConfig().subscribe(config => {
                let url = `${config.apiUrl}/${controllerName}`;
                if (actionName) {
                    url += `/${actionName}`;
                }
                this.http.get<T>(url, { params })
                    .subscribe(t => {
                        subscriber.next(t);
                        subscriber.complete();
                    });
            });
        });
    }

    post(controllerName: string, actionName: string, body: any): Observable<PostResult> {
        return new Observable<PostResult>(subscriber => {
            this.configService.getConfig().subscribe(config => {
                let url = `${config.apiUrl}/${controllerName}`;
                if (actionName) {
                    url += `/${actionName}`;
                }
                this.http.post(url, body)
                    .subscribe(t => {
                        subscriber.next(t);
                        subscriber.complete();
                    });
            });
        });
    }
}

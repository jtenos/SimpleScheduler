import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { mergeMap } from "rxjs/operators";
import Config from '../models/config';
import Kvp from '../models/kvp';
import { ConfigService } from './config.service';

@Injectable({
    providedIn: 'root'
})
export class ApiService {

    constructor(private configService: ConfigService, private http: HttpClient) {
    }

    get<T>(controllerName: string, parameters: Kvp<string, string>[]): Observable<T> {
        let params = new HttpParams();
        if (parameters) {
            for (const kvp of parameters) {
                params = params.append(kvp.key, kvp.value || "");
            }
        }

        return new Observable<T>(subscriber => {
            this.configService.getConfig().subscribe(config => {
                this.http.get<T>(`${config.apiUrl}/${controllerName}`, { params })
                    .subscribe(t => {
                        subscriber.next(t);
                        subscriber.complete();
                    });
            });
        });
    }
}

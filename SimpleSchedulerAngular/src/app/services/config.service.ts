import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { of } from "rxjs";
import Config from '../models/config';

@Injectable({
    providedIn: 'root'
})
export class ConfigService {

    private static config: Config;

    constructor(private http: HttpClient) {
    }

    getConfig(): Observable<Config> {
        if (ConfigService.config) {
            return of(ConfigService.config);
        }
        return this.http.get<Config>("assets/config.json");
    }
}

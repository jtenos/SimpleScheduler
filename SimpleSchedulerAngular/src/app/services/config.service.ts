import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import Config from '../models/config';

@Injectable({
    providedIn: 'root'
})
export class ConfigService {

    private static config: Config;

    constructor(private http: HttpClient) {
    }

    async getConfig(): Promise<Config> {
        if (ConfigService.config) {
            return ConfigService.config;
        }
        ConfigService.config = await this.http.get<Config>("assets/config.json").toPromise();
        return ConfigService.config;
    }
}

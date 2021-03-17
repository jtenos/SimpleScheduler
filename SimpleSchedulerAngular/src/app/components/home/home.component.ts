import { Component, OnInit } from '@angular/core';
import { ApiService } from 'src/app/services/api.service';


class Weather {
    constructor(date: Date, temperatureC: number) {
        this.date = date;
        this.temperatureC = temperatureC;
    }
    date?: Date;
    temperatureC?: number;
}


@Component({
    selector: 'app-home',
    templateUrl: './home.component.html',
    styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {

    constructor(private apiService: ApiService) { }

    ngOnInit(): void {

        this.apiService.get<Weather[]>("WeatherForecast", []).subscribe(weather => console.log(weather));
    }

}

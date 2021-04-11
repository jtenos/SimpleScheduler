import { Component } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { ApiService } from 'src/app/services/api.service';
import * as moment from "moment";

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html'
})
export class AppComponent {
    title = 'sch';
    clockStarted = false;

    constructor(router: Router, apiService: ApiService) {
        router.events.subscribe(e => {
            if (!this.clockStarted) {
                if (e instanceof NavigationEnd) {
                    async function refreshClock() {
                        const nowFormatted: string = await apiService.get("Hello", "GetUtcNow", []);
                        (document.querySelector("#utc-time") as any).textContent = nowFormatted;
                    }
                    function addSecond() {
                        const currentClock = moment((document.querySelector("#utc-time") as any).textContent as string, "MMM DD YYYY HH:mm:ss");
                        const newClock = currentClock.add(1, "s");
                        (document.querySelector("#utc-time") as any).textContent = newClock.format("MMM DD YYYY HH:mm:ss");
                    }
                    refreshClock();
                    setInterval(refreshClock, 20000); // Every 20 seconds, re-sync with the server
                    setInterval(addSecond, 1000); // Every one second, add a second to the clock to give the illusion of synching
                    this.clockStarted = true;
                }
            }
            if (localStorage.emailAddress) {
                (document.querySelector("#user-email") as any).textContent = localStorage.emailAddress;
            }
        });
    }
}

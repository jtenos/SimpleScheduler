import { Component, OnInit } from "@angular/core";
import { NavigationEnd, Router } from "@angular/router";
import { ApiService } from "src/app/services/api.service";
import * as moment from "moment";
import { JobService } from "src/app/services/job.service";

@Component({
  selector: "app-root",
  templateUrl: "./app.component.html",
  styleUrls: ["./app.component.scss"],
})
export class AppComponent implements OnInit {
  title = "sch";
  clockStarted = false;
  envPopulated = false;
  emailAddress = "todo";
  environmentName = "todo";
  isJobError = false;

  ngOnInit() {
    setInterval(async () => {
      const errorJobs = await this.jobService.getJobs({
        workerID: null,
        statusCode: "ERR",
      });
      this.isJobError = !!errorJobs.length;
    }, 15000);
  }

  constructor(router: Router, apiService: ApiService, private jobService: JobService) {
    router.events.subscribe((e) => {
      if (!this.clockStarted) {
        if (e instanceof NavigationEnd) {
          async function refreshClock() {
            const nowFormatted: string = await apiService.get(
              "Hello",
              "GetUtcNow",
              [],
            );
            (document.querySelector("#utc-time") as any).textContent =
              nowFormatted;
          }
          function addSecond() {
            const currentClock = moment(
              (document.querySelector("#utc-time") as any)
                .textContent as string,
              "MMM DD YYYY HH:mm:ss",
            );
            const newClock = currentClock.add(1, "s");
            (document.querySelector("#utc-time") as any).textContent = newClock
              .format("MMM DD YYYY HH:mm:ss");
          }
          refreshClock();
          setInterval(refreshClock, 20000); // Every 20 seconds, re-sync with the server
          setInterval(addSecond, 1000); // Every one second, add a second to the clock to give the illusion of synching
          this.clockStarted = true;
        }
      }
      if (localStorage.emailAddress) {
        (document.querySelector("#user-email") as any).textContent =
          localStorage.emailAddress;
      }
      if (!this.envPopulated) {
        async function getEnv() {
          const envName: string = await apiService.get(
            "Hello",
            "GetEnvironmentName",
            [],
          );
          document.querySelectorAll(".env-name").forEach(node => node.textContent = envName);
        }
        getEnv();
        this.envPopulated = true;
      }
    });
  }
}

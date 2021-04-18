import { Component, OnInit } from '@angular/core';
import { JobService } from 'src/app/services/job.service';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent implements OnInit {

  emailAddress = "todo";
  environmentName = "todo";
  isJobError = false;

  constructor(private jobService: JobService) { }

  ngOnInit(): void {
      setInterval(async () => {
        const errorJobs = await this.jobService.getJobs({ workerID: null, statusCode: "ERR" });
        this.isJobError = !!errorJobs.length;
      }, 15000);
  }

}

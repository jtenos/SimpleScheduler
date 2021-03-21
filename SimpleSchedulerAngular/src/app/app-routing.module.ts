import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { EditWorkerComponent } from './components/edit-worker/edit-worker.component';
import { HomeComponent } from './components/home/home.component';
import { JobsComponent } from './components/jobs/jobs.component';
import { LoginComponent } from './components/login/login.component';
import { SchedulesComponent } from './components/schedules/schedules.component';
import { ValidateUserComponent } from "./components/validate-user/validate-user.component";
import { WorkersComponent } from './components/workers/workers.component';

const routes: Routes = [
  { path: "", component: HomeComponent },
  { path: "login", component: LoginComponent },
  { path: "jobs", component: JobsComponent },
  { path: "schedules", component: SchedulesComponent },
  { path: "validate-user/:validationCode", component: ValidateUserComponent },
  { path: "workers", component: WorkersComponent },
  { path: "workers/edit/:workerID", component: EditWorkerComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }

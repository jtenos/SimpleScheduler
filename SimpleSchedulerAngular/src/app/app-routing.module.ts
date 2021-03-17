import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { HomeComponent } from './components/home/home.component';
import { JobsComponent } from './components/jobs/jobs/jobs.component';
import { SchedulesComponent } from './components/schedules/schedules/schedules.component';
import { EditWorkerComponent } from './components/workers/edit-worker/edit-worker.component';
import { WorkersComponent } from './components/workers/workers/workers.component';

const routes: Routes = [
  { path: "", component: HomeComponent },
  { path: "jobs", component: JobsComponent },
  { path: "schedules", component: SchedulesComponent },
  { path: "workers", component: WorkersComponent },
  { path: "workers/edit/:workerID", component: EditWorkerComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
